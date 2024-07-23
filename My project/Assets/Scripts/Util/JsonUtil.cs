using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Util
{
    public static class JsonUtil
    {
        public class JsonDataBase
        {
            public string key = null;
            public List<object> children = new List<object>();
        }

        /// <summary>
        /// 解析json文件
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public static JsonDataBase ParseJsonFile(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                return null;

            return ParseJsonString(File.ReadAllText(jsonPath));
        }
        
        /// <summary>
        /// 解析json字符串
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static JsonDataBase ParseJsonString(string jsonString)
        {
            return JsonConvert.DeserializeObject<JsonDataBase>(jsonString);
        }
        
        public static void SaveJsonFile(JsonDataBase data, string jsonPath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonString);
        }
    }
}