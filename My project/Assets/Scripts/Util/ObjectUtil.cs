using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Util
{
    public static class ObjectUtil
    {
        /// <summary>
        /// 是否具有公共访问字段
        /// </summary>
        /// <param name="o"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool HasPublicField(object o, string fieldName)
        {
            return o.GetType().GetField(fieldName) != null;
        }

        /// <summary>
        /// 尝试设置公共字段的值
        /// </summary>
        /// <param name="o"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryModifyField(object o, string fieldName, object value)
        {
            if (!HasPublicField(o, fieldName))
            {
                return false;
            }

            try
            {
                var fieldInfo = o.GetType().GetField(fieldName);
                if (fieldInfo.FieldType.IsValueType)
                {
                    object convertedValue = Convert.ChangeType(value, fieldInfo.FieldType);
                    var typedReference = __makeref(o);
                    fieldInfo.SetValueDirect(typedReference, convertedValue);
                }
                else
                {
                    fieldInfo.SetValue(o, value);
                }
            }
            catch (Exception e)
            {
                #if DEBUG
                    Debug.LogError($"set {o.ToString()} field name -> {fieldName} error :: " + e);
                #endif
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 是否具有公共访问字段
        /// </summary>
        /// <param name="o"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool HasPublicProperty(object o, string propertyName)
        {
            return o.GetType().GetProperty(propertyName) != null;
        }

        /// <summary>
        /// 尝试设置公共字段的值
        /// </summary>
        /// <param name="o"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryModifyProperty(object o, string propertyName, object value)
        {
            if (!HasPublicProperty(o, propertyName))
            {
                return false;
            }

            try
            {
                o.GetType().GetProperty(propertyName).SetValue(value, 0);
            }
            catch (Exception e)
            {
                Debug.LogError($"set {o.ToString()} property name -> {propertyName} error :: " + e);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 是否具有指定类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool HasType(string typeName)
        {
            ComplementaryTypeShorthand(ref typeName);
            var type = Type.GetType(typeName);

            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        break;
                }
            }

            return type != null;
        }

        /// <summary>
        /// 根据类型名称获取类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);

            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        break;
                }
            }

            return type;
        }

        /// <summary>
        /// 补全类型名简写
        /// </summary>
        /// <param name="typeName"></param>
        public static void ComplementaryTypeShorthand(ref string typeName)
        {
            typeName = typeName switch
            {
                "byte" => "System.Byte",
                "sbyte" => "System.SByte",
                "short" => "System.Int16",
                "ushort" => "System.UInt16",
                "int" => "System.Int32",
                "uint" => "System.UInt32",
                "long" => "System.Int64",
                "ulong" => "System.UInt64",
                "float" => "System.Single",
                "double" => "System.Double",
                "decimal" => "System.Decimal",
                "bool" => "System.Boolean",
                "char" => "System.Char",
                "string" => "System.String",
                "object" => "System.Object",
                _ => typeName
            };
        }

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeepCopy<T>(this T self) where T : class
        {
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, self);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}