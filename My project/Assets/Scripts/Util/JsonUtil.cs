using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            File.Delete(generatePath);
            File.WriteAllText(generatePath, jsonString);
        }
        
        /// <summary>
        /// 生成json文件(忽略循环调用)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="generatePath"></param>
        /// <typeparam name="T"></typeparam>
        public static void GenerateJsonFileIgnoreLoop<T>(T data, string generatePath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
            if(File.Exists(generatePath))
                File.Delete(generatePath);
            
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
        
        /// <summary>
        /// 保存Json文件
        /// </summary>
        /// <param name="data"></param>
        /// <param name="jsonPath"></param>
        /// <typeparam name="T"></typeparam>
        public static void SaveJsonFile<T>(T data, string jsonPath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonString);
        }
    }

    public class CustomGameObjectDictionaryConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = (Dictionary<string, object>)value;
            var jObject = new JObject();
            foreach (var kvp in dict)
            {
                jObject.Add(kvp.Key, JToken.FromObject(kvp.Value, serializer));
            }
            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var dict = new Dictionary<string, object>();
            foreach (var kvp in jObject)
            {
                dict[kvp.Key] = kvp.Value.ToObject<object>(serializer);
            }
            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, object>);
        }
    }
}