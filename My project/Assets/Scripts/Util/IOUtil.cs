using System;
using System.IO;
using UnityEngine;

namespace Util
{
    public static class IOUtil
    {
        /// <summary>
        /// 文件夹是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HasDirectory(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 尝试创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryCreateDirectory(string path)
        {
            if (!HasDirectory(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"CreteDirectory {path} Error :: " + e);
                    return false;
                }
            }

            return true;
        }
    }
}