using System;
using System.IO;
using System.Text;
using Unity.VisualScripting;
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

        /// <summary>
        /// 获取前一个节点目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="curNode"></param>
        /// <returns></returns>
        public static string GetLastDirectory(string path, string curNode)
        {
            var tmpSB = new StringBuilder();
            
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(curNode))
            {
                return "";
            }
    
            var directories = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int curNodeIndex = Array.IndexOf(directories, curNode);
            
            if (curNodeIndex > 0)
            {
                tmpSB.Append(directories[curNodeIndex - 1]);
            }
            
            return tmpSB.ToString();
        }
    }
}