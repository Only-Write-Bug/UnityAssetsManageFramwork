using System;
using System.Collections.Generic;

namespace Tools
{
    public class ObjectPoolManager
    {
        private static ObjectPoolManager _init = null;
        public static ObjectPoolManager init => _init ??= new ObjectPoolManager();

        private Dictionary<Type, ObjectPoolBase> _poolDic = new Dictionary<Type, ObjectPoolBase>();

        public ObjectPool<T> TryGetObjectPool<T>() where T : class, IRecycle, new()
        {
            var type = typeof(T);
            if (_poolDic.TryGetValue(type, out var pool))
            {
                if (pool != null)
                {
                    return pool as ObjectPool<T>;
                }
            }

            var curPool = new ObjectPool<T>();
            _poolDic[type] = curPool;
            return curPool;
        }

        public void DestroyObjectPool<T>() where T : class, IRecycle, new()
        {
            var type = typeof(T);
            if (_poolDic.ContainsKey(type))
            {
                _poolDic[type].Dispose();
                _poolDic.Remove(type);
            }
        }
    }
}