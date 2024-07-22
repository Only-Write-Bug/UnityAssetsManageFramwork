using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using Util;

namespace Editor
{
    public static class GenerateTool
    {
        public static void GenerateJSON(JsonUtil.JsonDataBase data, string generatePath)
        {
            var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(generatePath, jsonString);
        }
    }
}