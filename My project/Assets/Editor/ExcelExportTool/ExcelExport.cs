using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Editor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Util;

public class ExcelExport
{
    private const string ExcelDirectory = "Excel";
    private const string ExcelModifyDirtyDirectory = @"Excel\ModifyTimeJSON";
    private const string ExcelScriptDirectory = @"Assets\Scripts\Common\Excel";

    private static string excelPath = null;
    private static string ExcelModifyDirtyPath = null;
    private static string excelScriptPath = null;

    [MenuItem("Editor Tool/Excel Export")]
    public static void Export()
    {
        if (!CheckDirectory())
            return;
        GetUpdatedExcel();
    }

    private static bool CheckDirectory()
    {
        excelPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelDirectory);
        ExcelModifyDirtyPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelModifyDirtyDirectory);
        excelScriptPath = Path.Combine(Directory.GetCurrentDirectory(), ExcelScriptDirectory);

        return IOUtil.TryCreateDirectory(excelPath) && IOUtil.TryCreateDirectory(ExcelModifyDirtyPath) &&
               IOUtil.TryCreateDirectory(ExcelScriptDirectory);
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
                CheckExcelRecodingTime(path);
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
        jsonData.children.Add(new { lastRecordingTime = DateTime.Now.ToString("O")});

        GenerateTool.GenerateJSON(jsonData, GetExcelModifyTimeJson(excelPath));
    }

    private static string GetExcelModifyTimeJson(string excelPath)
    {
        return Path.Combine(ExcelModifyDirtyPath, JsonNameFactory(Path.GetFileNameWithoutExtension(excelPath)));
    }

    private static void CheckExcelRecodingTime(string excelPath)
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
        }
    }
}