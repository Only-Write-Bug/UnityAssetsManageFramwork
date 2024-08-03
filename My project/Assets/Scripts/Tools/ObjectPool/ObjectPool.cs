using System.Collections.Generic;

namespace Tools
{
    public abstract class ObjectPoolBase
    {
        public abstract void Dispose();
    }
    public class ObjectPool<T> : ObjectPoolBase where T : class, IRecycle, new()
    {
        ~ObjectPool()
        {
            Dispose();
        }
        public ObjectPool()
        {
            FillPool();
        }
        
        private int _curSize = 4;
        public int size
        {
            get => _curSize;
            set
            {
                _curSize = value;
                if (_pool.Count > size)
                {
                    while (_pool.Count > size)
                    {
                        _pool.Dequeue().Recycle();
                    }
                }
                else
                {
                    FillPool();
                }
            }
        }

        private Queue<T> _pool = new Queue<T>();

        public override void Dispose()
        {
            while (_pool.Count > 0)
            {
                _pool.Dequeue().Recycle();
            }

            _pool = null;
        }

        public T RequestObj()
        {
            if (_pool.Count <= 0)
            {
                var item = new T();
                item.Recycle();
                return item;
            }

            return _pool.Dequeue();
        }

        public void RecycleObj(T obj)
        {
            obj.Recycle();
            if (_pool.Count < size)
            {
                _pool.Enqueue(obj);
            }
        }

        private void FillPool()
        {
            while (_pool.Count < size)
            {
                var item = new T();
                item.Recycle();
                _pool.Enqueue(item);
            }
        }
    }
}