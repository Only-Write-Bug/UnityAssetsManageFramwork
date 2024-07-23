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
    }
}