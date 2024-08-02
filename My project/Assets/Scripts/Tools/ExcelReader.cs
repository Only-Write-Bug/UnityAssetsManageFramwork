using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Util;
using Debug = UnityEngine.Debug;

namespace Tools
{
    public class CustomExcelAttribute : Attribute
    {
        
    }
    
    public class ExcelReader
    {
        private static ExcelReader _init = null;
        public static ExcelReader init => _init ??= new ExcelReader();

        public void LoadAllExcels()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if (type.GetCustomAttributes(typeof(CustomExcelAttribute), true).Length > 0)
                {
                    var initProperty = type.GetProperty("init", BindingFlags.Static | BindingFlags.Public);
                    if (initProperty!= null)
                    {
                        var loadMethod = type.GetMethod("Load");
                        if (loadMethod != null)
                        {
                            loadMethod.Invoke(initProperty.GetValue(null), null);
                        }
                    }
                }
            }
        }

        public Dictionary<long, JsonUtil.JsonDataBase> LoadExcelDataJson()
        {
            var stackTrace = new StackTrace();
            var callingClassType = stackTrace.GetFrame(1).GetMethod().DeclaringType;

            RelatedNameFactory(callingClassType, out var dataStructName, out var dataJsonName);
            LoadExcelDataFormJson(dataStructName, dataJsonName, out var dic);

            return dic;
        }

        private void RelatedNameFactory(System.Type type, out string dataStructName, out string dataJsonName)
        {
            dataStructName = type.ToString() + "_Data";
            dataJsonName = type.ToString().Replace("_Excel", "_data");
        }

        private void LoadExcelDataFormJson(string dataStructName, string dataJsonName,
            out Dictionary<long, JsonUtil.JsonDataBase> dic)
        {
            dic = new Dictionary<long, JsonUtil.JsonDataBase>();
            
            var dataStructType = ObjectUtil.GetType(dataStructName);
            if (dataStructType == null)
            {
                Debug.LogError($"ExcelReader {dataStructName} Type is not found");
                return;
            }
            
            var excelJsonFilePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Resources\JSON\Excel"),$"{dataJsonName}.json");
            var jsonDataArray = JsonUtil.ParseJsonFile<dynamic>(excelJsonFilePath);
            if (jsonDataArray != null)
            {
                foreach (var rawData in jsonDataArray as JArray)
                {
                    var data = rawData.ToObject(dataStructType);
                    if (ObjectUtil.HasPublicField(data, "ID"))
                    {
                        dic.TryAdd((long)data.GetType().GetField("ID").GetValue(data), (JsonUtil.JsonDataBase)data);
                    }
                }
            }
        }
    }
}