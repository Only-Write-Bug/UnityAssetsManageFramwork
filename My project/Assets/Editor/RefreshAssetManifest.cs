using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class RefreshAssetManifest
    {
        private static HashSet<string> _elements = new HashSet<string>();
        
        [MenuItem("AssetManager/Refresh Asset Manifest")]
        public static void Refresh()
        {
            var manifestXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Resources\manifest.xml");

            if(File.Exists(manifestXmlFilePath))
                File.Delete(manifestXmlFilePath);
            
            var root = new XElement("AssetManifest");
            var xml = new XDocument();
            xml.Add(root);
            
            GetAllPrefabJson(root);

            _elements.Clear();
            xml.Save(manifestXmlFilePath);

            RefreshAssetsKeyScript(root);
            AssetDatabase.Refresh();
            Debug.Log("AssetManifest refreshed");
        }

        private static void GetAllPrefabJson(XElement root)
        {
            var prefabJsonDirectory = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Resources\JSON\Prefabs");
            if (!Directory.Exists(prefabJsonDirectory))
            {
                Debug.LogError("Refresh Manifest Error :: not found prefab json directory");
                return;
            }

            foreach (var json in Directory.GetFiles(prefabJsonDirectory, "*.json", SearchOption.AllDirectories))
            {
                var key = Path.GetFileNameWithoutExtension(json);
                if (_elements.Contains(key))
                {
                    Debug.LogError($"Manifest has same key :: {key} :: Error file :: {json}");
                    continue;
                }
                root.Add(new XElement(key, new XAttribute("path", json)));
            }
        }

        private static void RefreshAssetsKeyScript(XElement root)
        {
            var assetKeyScript = Path.Combine(Directory.GetCurrentDirectory(), @"Assets\Scripts\Common\Key\AssetsKey.cs");

            if (File.Exists(assetKeyScript))
            {
                File.Delete(assetKeyScript);
            }

            var content = new StringBuilder();
            content.AppendLine("public class AssetsKey");
            content.AppendLine("{");

            foreach (var element in root.Elements())
            {
                content.AppendLine($"\tpublic static readonly string {element.Name} = @\"{element.Attribute("path").Value}\";");
            }
            
            content.AppendLine("}");

            File.WriteAllText(assetKeyScript, content.ToString());
        }
    }
}