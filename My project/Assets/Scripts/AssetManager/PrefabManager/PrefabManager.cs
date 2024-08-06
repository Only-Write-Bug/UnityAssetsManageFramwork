using UnityEngine;

namespace AssetManager.PrefabManager
{
    public class PrefabManager
    {
        private PrefabManager _init = null;
        public PrefabManager init => _init ??= new PrefabManager();

        private PrefabManager()
        {
        }

        public GameObject CreatePrefab(string key)
        {
            return null;
        }
    }
}