using System.IO;
using UnityEditor;
using UnityEngine;
using Selection = UnityEditor.Selection;

namespace Editor.PrefabExportTool
{
    public static class PrefabExportTool
    {
        [MenuItem("Assets/Export Prefab", false, priority = 1)]
        public static void ExportPrefab()
        {
            CommonPath_Excel.init.Load();
            var prefabJsonPath = Path.Combine(Directory.GetCurrentDirectory(), CommonPath_Excel.init.GetDataById(100001).path);
            foreach (var o in Selection.objects)
            {
                
            }
        }
    }
}