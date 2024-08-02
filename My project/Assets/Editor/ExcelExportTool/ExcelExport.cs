using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using ExcelDataReader;
using UnityEngine;
using UnityEngine.Rendering;
using Util;

public class ExcelExport
{
    private const string _ExcelDirectory = "Excel";
    private const string _ExcelModifyDirtyDirectory = @"Excel\ModifyTimeJSON";
    private const string _ExcelScriptDirectory = @"Assets\Scripts\Common\Excel";
    private const string _ExcelDataJsonDirectory = @"Assets\Resources\JSON\Excel";

    private static string _excelPath = null;
    private static string _ExcelModifyDirtyPath = null;
    private static string _excelScriptPath = null;
    private static string _excelDataJsonPath = null;

    private static string _curExportExcelPath = null;
    private static string[][] _curExcelData = null;
    
    private static float _compileWaitTime = 0f;
    private static readonly float compileWaitDuration = 3f;

    [MenuItem("Editor Tool/test")]
    public static void Test()
    {
        CommonPath_Excel.init.Load();
    }

    [MenuItem("Editor Tool/Excel Export")]
    public static void Export()
    {
        if (!CheckDirectory())
            return;
        var updatedExcel = GetUpdatedExcel();
        foreach (var excelPath in updatedExcel)
        {
            var excelData = DataStructUtil.Reverse2DArray(ReadExcel(excelPath));
            if (excelData == null)
                continue;
            _curExcelData = excelData;
            if (GenerateDataStructScript(excelPath, excelData))
            {
                _curExportExcelPath = excelPath;
                AssetDatabase.Refresh();
                EditorApplication.update += OnScriptForcedCompile;
            }
            else
            {
                TryDeleteExcelModifyTimeJson(excelPath);
                TryDeleteExcelDataJson(excelPath);
                _curExcelData = null;
            }
        }
    }

    private static bool CheckDirectory()
    {
        _excelPath = Path.Combine(Directory.GetCurrentDirectory(), _ExcelDirectory);
        _ExcelModifyDirtyPath = Path.Combine(Directory.GetCurrentDirectory(), _ExcelModifyDirtyDirectory);
        _excelScriptPath = Path.Combine(Directory.GetCurrentDirectory(), _ExcelScriptDirectory);
        _excelDataJsonPath = Path.Combine(Directory.GetCurrentDirectory(), _ExcelDataJsonDirectory);

        return IOUtil.TryCreateDirectory(_excelPath) && IOUtil.TryCreateDirectory(_ExcelModifyDirtyPath) &&
               IOUtil.TryCreateDirectory(_ExcelScriptDirectory) && IOUtil.TryCreateDirectory(_excelDataJsonPath);
    }

    private static void OnScriptForcedCompile()
    {
        if (!EditorApplication.isCompiling)
        {
            _compileWaitTime += Time.deltaTime;
            if (_compileWaitTime >= compileWaitDuration)
            {
                if (GenerateExcelDataJson(_curExportExcelPath))
                {
                    EditorApplication.update -= OnScriptForcedCompile;
                    _curExportExcelPath = null;
                    _curExcelData = null;
                    Debug.Log("Excel Export Over!!!");
                }
                _compileWaitTime = 0f; // 重置等待时间
            }
        }
    }

    private static List<string> GetUpdatedExcel()
    {
        var updatedExcel = new List<string>();

        foreach (var path in Directory.GetFiles(_ExcelDirectory, "*.xlsx"))
        {
            var excelFileName = Path.GetFileNameWithoutExtension(path);
            var jsonFileName = JsonNameFactory(excelFileName);

            var targetFiles = Directory.GetFiles(_ExcelModifyDirtyPath, jsonFileName);
            if (targetFiles.Length <= 0)
            {
                updatedExcel.Add(path);
                CreateModifyTimeJson(path);
                continue;
            }
            else
            {
                CheckExcelRecodingTime(path, ref updatedExcel);
            }
        }

        return updatedExcel;
    }

    private static string JsonNameFactory(string excelFileName)
    {
        return $"LastModifyTime_{excelFileName}.json";
    }

    private static void CreateModifyTimeJson(string excelPath)
    {
        var jsonData = new JsonUtil.JsonDataModel
        {
            key = Path.GetFileNameWithoutExtension(excelPath)
        };
        jsonData.children.Add(new { lastRecordingTime = DateTime.Now.ToString("O") });

        JsonUtil.GenerateJsonFile(jsonData, GetExcelModifyTimeJson(excelPath));
    }

    private static string GetExcelModifyTimeJson(string excelPath)
    {
        return Path.Combine(_ExcelModifyDirtyPath, JsonNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
    }

    private static void CheckExcelRecodingTime(string excelPath, ref List<string> updatedExcel)
    {
        var jsonFilePath = GetExcelModifyTimeJson(excelPath);
        var jsonData = JsonUtil.ParseJsonFile<JsonUtil.JsonDataModel>(jsonFilePath) as JsonUtil.JsonDataModel;

        if (jsonData == null || jsonData.children.Count == 0)
        {
            Debug.LogError("Excel Export Error :: No valid JSON data found.");
            return;
        }

        var firstChild = jsonData.children[0] as JObject;

        if (firstChild == null)
        {
            Debug.LogError("Excel Export Error :: The first child is not a valid JObject.");
            return;
        }

        var lastRecordingTime = firstChild.GetValue("lastRecordingTime").ToObject<DateTime>();

        if (File.GetLastWriteTime(excelPath) > lastRecordingTime)
        {
            firstChild["lastRecordingTime"] = DateTime.Now;
            JsonUtil.SaveJsonFile(jsonData, jsonFilePath);
            updatedExcel.Add(excelPath);
        }
    }

    private static string[][] ReadExcel(string excelFilePath)
    {
        using (var stream = File.Open(Path.Combine(ExcelExport._excelPath, Path.GetFileName(excelFilePath)),
                   FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var excelData = new List<string[]>();
                do
                {
                    while (reader.Read())
                    {
                        var rowResult = new List<string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            rowResult.Add(reader.GetValue(i) != null ? reader.GetValue(i).ToString() : "");
                        }

                        excelData.Add(rowResult.ToArray());
                    }
                } while (reader.NextResult());

                return excelData.Count <= 0 ? null : excelData.ToArray();
            }
        }
    }

    private static string ScriptNameFactory(string excelFileName)
    {
        return $"{excelFileName}_excel.cs";
    }

    private static string GetExcelScript(string excelPath)
    {
        return Path.Combine(_excelScriptPath, ScriptNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
    }

    private static bool GenerateDataStructScript(string excelPath, string[][] excelData)
    {
        var excelScriptPath = GetExcelScript(excelPath);
        var content = new StringBuilder();
        var excelName = Path.GetFileNameWithoutExtension(excelPath);

        (bool hasID, string type) excel_id = new ValueTuple<bool, string>();
        excel_id.hasID = false;

        if (File.Exists(excelScriptPath))
        {
            File.Delete(excelScriptPath);
        }

        content.AppendLine("using System;");
        content.AppendLine("using System.Collections.Generic;");
        content.AppendLine("using Tools;");
        content.AppendLine("using Util;");
        
        content.AppendLine("");

        content.AppendLine($"public class {ExcelDataStructNameFactory(excelPath)} : JsonUtil.JsonDataBase");
        content.AppendLine("{");
        for (var i = 0; i < excelData.Length; i++)
        {
            if (!ObjectUtil.HasType(excelData[i][1]))
            {
                Debug.LogError($"Excel Export Error :: {excelName} has not defined type :: {excelData[i][1]}");
                return false;
            }

            content.AppendLine($"\tpublic {excelData[i][1]} {excelData[i][0]};");

            if (excelData[i][0].ToUpper().Equals("ID"))
            {
                excel_id.hasID = true;
                excel_id.type = excelData[i][1];
            }
        }


        if (!excel_id.hasID)
        {
            Debug.LogError($"Excel Export Error :: {excelName} don't has ID field");
            return false;
        }

        content.AppendLine("}");

        content.AppendLine("");
        
        content.AppendLine($"public class {excelName}_Excel");
        content.AppendLine("{");
        content.AppendLine($"\tprivate static {excelName}_Excel _init = null;");
        content.AppendLine($"\tpublic static {excelName}_Excel init => _init ??= new {excelName}_Excel();\n");
        content.AppendLine(
            $"\tprivate Dictionary<{excel_id.type}, {ExcelDataStructNameFactory(excelPath)}> _cacheData = new Dictionary<{excel_id.type}, {ExcelDataStructNameFactory(excelPath)}>();");

        content.AppendLine("\tpublic void Load()");
        content.AppendLine("\t{");
        content.AppendLine("\t\tforeach (var data in ExcelReader.init.LoadExcelDataJson())");
        content.AppendLine($"\t\t\t_cacheData.TryAdd(data.Key, data.Value as {ExcelDataStructNameFactory(excelPath)});");
        content.AppendLine("\t}");
        content.AppendLine("}");

        File.WriteAllText(excelScriptPath, content.ToString());

        return true;
    }

    private static string ExcelDataStructNameFactory(string excelPath)
    {
        var excelFileName = Path.GetFileNameWithoutExtension(excelPath);
        return $"{excelFileName}_Excel_Data";
    }

    private static string DataJsonNameFactory(string excelFileName)
    {
        return $"{excelFileName}_data.json";
    }

    private static string GetExcelDataJson(string excelPath)
    {
        return Path.Combine(_excelDataJsonPath, DataJsonNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
    }

    private static int getDataStructRetryCount = 0;
    private static bool GenerateExcelDataJson(string excelPath)
    {
        TryDeleteExcelDataJson(excelPath);
        
        var dataStructType = ObjectUtil.GetType(ExcelDataStructNameFactory(excelPath));
        if (dataStructType == null)
        {
            if (getDataStructRetryCount++ >= 10)
            {
                EditorApplication.update -= OnScriptForcedCompile;
                Debug.LogError($"Excel Export Error :: Not found {dataStructType}");
                getDataStructRetryCount = 0;
                return false;
            }
            Debug.LogWarning($"Excel Export :: Retry found {dataStructType}");
            return false;
        }

        GenerateExcelDataObject(dataStructType, out var dataContainer);

        JsonUtil.GenerateJsonFile(dataContainer, GetExcelDataJson(excelPath));
        return true;
    }

    private static void GenerateExcelDataObject(Type dataType, out List<object> dataContainer)
    {
        dataContainer = new List<object>();

        if (_curExcelData.Length < 1 || _curExcelData[0].Length < 2)
            return;
        for (var i = 2; i < _curExcelData[0].Length; i++)
        {
            var data = Activator.CreateInstance(dataType);
            for (var j = 0; j < _curExcelData.Length; j++)
            {
                ObjectUtil.TryModifyField(data, _curExcelData[j][0], _curExcelData[j][i]);
            }
            dataContainer.Add(data);
        }
    }

    private static void TryDeleteExcelModifyTimeJson(string excelPath)
    {
        var targetExcelModifyTimeJsonPath = GetExcelDataJson(excelPath);

        if (File.Exists(targetExcelModifyTimeJsonPath))
        {
            File.Delete(targetExcelModifyTimeJsonPath);
        }
    }

    private static void TryDeleteExcelDataJson(string excelPath)
    {
        var targetExcelDataJsonPath = GetExcelDataJson(excelPath);

        if (File.Exists(targetExcelDataJsonPath))
        {
            File.Delete(targetExcelDataJsonPath);
        }
    }
}