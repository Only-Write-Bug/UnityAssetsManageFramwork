using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Util
{
    public static class JsonUtil
    {
        public abstract class JsonDataBase
        {
            
        }
        public class JsonDataModel : JsonDataBase
        {
            public string key = null;
            public List<object> children = new List<object>();
        }
        
        /// <summary>
        /// 生成json文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="generatePath"></param>
        public static void GenerateJsonFile<T>(T data, string generatePath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(generatePath, jsonString);
        }

        /// <summary>
        /// 解析json文件
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public static object ParseJsonFile<T>(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                return null;

            return ParseJsonString<T>(File.ReadAllText(jsonPath));
        }
        
        /// <summary>
        /// 解析json字符串
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T ParseJsonString<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        
        public static void SaveJsonFile<T>(T data, string jsonPath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonString);
        }
    }
}