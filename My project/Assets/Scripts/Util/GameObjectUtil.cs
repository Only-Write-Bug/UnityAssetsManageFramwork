using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static class GameObjectUtil
    {
        [System.Serializable]
        public class CustomSerializeGameObjectData
        {
            public string name;
            public int id;
            public List<CustomComponentData> components;
            public List<CustomSerializeGameObjectData> children;
        }

        [System.Serializable]
        public class CustomComponentData
        {
            public string type;
            public Dictionary<string, object> properties;
        }
        
        public static CustomSerializeGameObjectData CustomSerialize(this GameObject obj)
        {
            var data = new CustomSerializeGameObjectData();

            data.name = obj.name;
            data.id = obj.GetInstanceID();
            
            data.components = new List<CustomComponentData>();
            foreach (var component in obj.GetComponents<Component>())
            {
                Debug.Log(component);
            }

            return data;
        }

        public static CustomComponentData CustomSerialize(this Component component)
        {
            var data = new CustomComponentData();

            return data;
        }

        public static GameObject DeserializeGameObject(CustomSerializeGameObjectData data)
        {
            return null;
        }

        public static Component DeserializeComponent(CustomComponentData data)
        {
            return null;
        }
    }
}