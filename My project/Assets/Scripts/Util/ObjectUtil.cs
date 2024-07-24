using System;

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
                o.GetType().GetField(fieldName).SetValue(value, 0);
            }
            catch (Exception e)
            {
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
    }
}