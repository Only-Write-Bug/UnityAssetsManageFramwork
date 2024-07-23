using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using ExcelDataReader;
using UnityEngine;
using UnityEngine.Rendering;
using Util;

public class ExcelExport
{
    private const string ExcelDirectory = "Excel";
    private const string ExcelModifyDirtyDirectory = @"Excel\ModifyTimeJSON";
    private const string ExcelScriptDirectory = @"Assets\Scripts\Common\Excel";
    private const string ExcelDataJsonDirectory = @"Assets\Resources\JSON\Excel";

    private static string excelPath = null;
    private static string ExcelModifyDirtyPath = null;
    private static string excelScriptPath = null;
    private static string excelDataJsonPath = null;

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
            if (!GenerateDataStructScript(excelPath, excelData))
            {
                continue;
            }
        }

        AssetDatabase.Refresh();
    }

    private static bool CheckDirectory()
    {
        excelPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelDirectory);
        ExcelModifyDirtyPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelModifyDirtyDirectory);
        excelScriptPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelScriptDirectory);
        excelDataJsonPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelDataJsonDirectory);

        return IOUtil.TryCreateDirectory(excelPath) && IOUtil.TryCreateDirectory(ExcelModifyDirtyPath) &&
               IOUtil.TryCreateDirectory(ExcelScriptDirectory) && IOUtil.TryCreateDirectory(excelDataJsonPath);
    }

    private static List<string> GetUpdatedExcel()
    {
        var updatedExcel = new List<string>();

        foreach (var path in Directory.GetFiles(ExcelDirectory, "*.xlsx"))
        {
            var excelFileName = Path.GetFileNameWithoutExtension(path);
            var jsonFileName = JsonNameFactory(excelFileName);

            var targetFiles = Directory.GetFiles(ExcelModifyDirtyPath, jsonFileName);
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
        var jsonData = new JsonUtil.JsonDataBase
        {
            key = Path.GetFileNameWithoutExtension(excelPath)
        };
        jsonData.children.Add(new { lastRecordingTime = DateTime.Now.ToString("O") });

        GenerateTool.GenerateJSON(jsonData, GetExcelModifyTimeJson(excelPath));
    }

    private static string GetExcelModifyTimeJson(string excelPath)
    {
        return Path.Combine(ExcelModifyDirtyPath, JsonNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
    }

    private static void CheckExcelRecodingTime(string excelPath, ref List<string> updatedExcel)
    {
        var jsonFilePath = GetExcelModifyTimeJson(excelPath);
        var jsonData = JsonUtil.ParseJsonFile(jsonFilePath);

        if (jsonData == null || jsonData.children.Count == 0)
        {
            Console.WriteLine("No valid JSON data found.");
            return;
        }

        var firstChild = jsonData.children[0] as JObject;

        if (firstChild == null)
        {
            Console.WriteLine("The first child is not a valid JObject.");
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
        using (var stream = File.Open(Path.Combine(ExcelExport.excelPath, Path.GetFileName(excelFilePath)),
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
        return Path.Combine(excelScriptPath, ScriptNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
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
        content.AppendLine("using System.Collections.Generic;");

        content.AppendLine($"public class {excelName}_excel_data");
        content.AppendLine("{");
        for (var i = 0; i < excelData.Length; i++)
        {
            if (!ObjectUtil.HasType(excelData[i][1]))
            {
                Debug.LogError($"Excel Export Error :: {excelName} has not defined type");
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

        content.AppendLine($"public class {excelName}_excel");
        content.AppendLine("{");
        content.AppendLine($"\tprivate var _cacheData = new Dictionary<{excel_id.type}, {excelName}_excel_data>();");
        
        content.AppendLine("}");

        File.WriteAllText(excelScriptPath, content.ToString());

        return true;
    }
}