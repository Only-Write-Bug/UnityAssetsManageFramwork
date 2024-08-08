using System.IO;
using UnityEngine;
using Util;

namespace AssetManager.PrefabManager
{
    public class PrefabManager
    {
        private static PrefabManager _init = null;
        public static PrefabManager init => _init ??= new PrefabManager();

        private PrefabManager()
        {
        }

        /// <summary>
        /// 创建预制体
        /// 必须使用AssetsKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public GameObject CreatePrefab(string key, GameObject parent = null)
        {
            if (!File.Exists(key))
            {
                Debug.Log("Create Prefab Error :: Unknown resource location, check the key!");
                return null;
            }

            GameObjectUtil.ParsePrefabJson(key);
            
            return null;
        }
    }
}