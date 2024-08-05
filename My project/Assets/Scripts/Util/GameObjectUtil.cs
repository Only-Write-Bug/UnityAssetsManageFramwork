using System;
using System.Collections.Generic;
using System.Reflection;
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
            public List<CustomComponentData> components = new List<CustomComponentData>();
            public List<CustomSerializeGameObjectData> children = new List<CustomSerializeGameObjectData>();
        }

        [System.Serializable]
        public class CustomComponentData
        {
            public string type;
            public Dictionary<string, object> properties = new Dictionary<string, object>();
        }
        
        public static CustomSerializeGameObjectData CustomSerialize(this GameObject obj)
        {
            var data = new CustomSerializeGameObjectData();

            data.name = obj.name;
            data.id = obj.GetInstanceID();
            
            foreach (var component in obj.GetComponents<Component>())
            {
                data.components.Add(component.CustomSerialize());
            }
            
            for (var i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i).gameObject;
                data.children.Add(child.CustomSerialize());
            }

            return data;
        }

        public static CustomComponentData CustomSerialize(this Component component)
        {
            var data = new CustomComponentData();

            data.type = component.GetType().ToString();
            foreach (var property in component.GetType().GetProperties())
            {
                // if (property.IsDefined(typeof(ObsoleteAttribute), true))
                //     continue;
                // if (property.GetIndexParameters().Length > 0)
                //     continue;
                // if(!property.CanRead)
                //     continue;
                // if(property.Name.Equals("rigidbody") || property.Name.Equals("rigidbody2D") || property.Name.Equals("particleSystem"))
                //     continue;

                data.properties[property.PropertyType.ToString()] = property.GetValue(component);
            }

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