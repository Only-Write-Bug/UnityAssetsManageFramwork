using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

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

            public Dictionary<string, (string type, object obj)> properties =
                new Dictionary<string, (string type, object obj)>();
        }

        public static void Destroy(this GameObject self)
        {
            Object.Destroy(self);
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
            foreach (var property in component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.IsDefined(typeof(ObsoleteAttribute), true))
                    continue;
                if (property.GetIndexParameters().Any())
                    continue;
                if (!property.CanRead)
                    continue;

                try
                {
                    var value = property.GetValue(component);
                    if (value != null && value.GetType().IsClass &&
                        value.GetType().Assembly == typeof(GameObject).Assembly)
                        continue;
                    data.properties[property.Name] = (property.PropertyType.ToString(), value);
                }
                catch (Exception e)
                {
                    Debug.Log($"Prefab Export Error :: {component.gameObject}.{component} :: {e}");
                }
            }

            return data;
        }

        public static CustomSerializeGameObjectData ParsePrefabJson(string filePath)
        {
            var serializationData = new SerializationData(File.ReadAllText(filePath));
            var customGameObjectData = new object();
            serializationData.DeserializeInto(ref customGameObjectData);
            return customGameObjectData as CustomSerializeGameObjectData;
        }

        public static GameObject DeserializeGameObject(this CustomSerializeGameObjectData data)
        {
            var go = new GameObject(data.name);

            if (data.components.Count > 0)
            {
                foreach (var customComponentData in data.components)
                {
                    var component = customComponentData.DeserializeComponent();
                    go.AddComponent(component.GetType());
                }
            }

            return go;
        }

        public static Component DeserializeComponent(this CustomComponentData data)
        {
            Type componentType = Type.GetType(data.type);
            Component component = null;

            if (componentType != null)
            {
                component = Object.Instantiate(Activator.CreateInstance(componentType)) as Component;

                // 设置组件的字段值
                if (data.properties != null)
                {
                    foreach (var pair in data.properties)
                    {
                        var fieldInfo = componentType.GetField(pair.Key,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(component, pair.Value);
                        }
                        else
                        {
                            var propertyInfo = componentType.GetProperty(pair.Key,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (propertyInfo != null && propertyInfo.CanWrite)
                            {
                                propertyInfo.SetValue(component, pair.Value);
                            }
                        }
                    }
                }
            }

            return component;
        }
    }
}