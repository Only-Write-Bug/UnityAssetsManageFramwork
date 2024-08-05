using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public static class DataStructUtil
    {
        /// <summary>
        /// 翻转2维数组
        /// </summary>
        /// <param name="sourceArray"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[][] Reverse2DArray<T>(T[][] sourceArray)
        {
            if (sourceArray == null)
                return null;
            
            var sourceArrayRow = sourceArray.Length;
            var sourceArrayCol = sourceArray[0].Length;
            var resultArray = new T[sourceArrayCol][];

            for (var i = 0; i < sourceArrayCol; i++)
            {
                resultArray[i] = new T[sourceArrayRow];
                for (var j = 0; j < sourceArrayRow; j++)
                {
                    resultArray[i][j] = sourceArray[j][i];
                }
            }

            return resultArray;
        }

        /// <summary>
        /// 队列的深拷贝
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Queue<T> DeepCopy<T>(this Queue<T> self) where T : class, new()
        {
            var result = new Queue<T>();

            foreach (var item in self)
            {
                result.Enqueue(item);
            }
            
            return result;
        }

        public static string Splicing(this string self, string other, ref string result, char splicingChar = '_')
        {
            var resultSB = new StringBuilder();
            resultSB.Append(self);
            resultSB.Append(splicingChar);
            resultSB.Append(other);
            return result = resultSB.ToString();
        }
    }
}