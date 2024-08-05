using System.IO;
using Newtonsoft.Json.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Util;
using Formatting = Newtonsoft.Json.Formatting;
using Selection = UnityEditor.Selection;

namespace Editor.PrefabExportTool
{
    public static class PrefabExportTool
    {
        [MenuItem("Assets/Export Prefab", false, priority = 1)]
        public static void ExportPrefab()
        {
            CommonPath_Excel.init.Load();
            var prefabJsonDirectory = Path.Combine(Directory.GetCurrentDirectory(), CommonPath_Excel.init.GetDataById(100001).path);
            IOUtil.TryCreateDirectory(prefabJsonDirectory);
            
            foreach (var o in Selection.objects)
            {
                var prefabPath = AssetDatabase.GetAssetPath(o);
                if(Path.GetExtension(prefabPath) != ".prefab")
                    return;
                var prefabKey = Path.GetFileNameWithoutExtension(prefabPath);
                IOUtil.GetLastDirectory(prefabPath, Path.GetFileName(prefabPath)).ToLower().Splicing(prefabKey, ref prefabKey);
                
                var prefabJsonPath = Path.Combine(prefabJsonDirectory, prefabKey + ".json");
                if(File.Exists(prefabJsonPath))
                    File.Delete(prefabJsonPath);
                
                var jsonContext = (o as GameObject).CustomSerialize().Serialize().json;
                File.WriteAllText(prefabJsonPath, JObject.Parse(jsonContext).ToString(Formatting.Indented));
            }
        }
    }
}