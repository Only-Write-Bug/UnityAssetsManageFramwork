using System;
using System.Collections.Generic;
using Util;

namespace Tools.EventCenter
{
    public class Event
    {
        private class SubscribeData : IRecycle
        {
            private static long _subscriberBaseId = 0;
            private long _id;
            public long id => _id;

            public SubscribeData()
            {
                this._id = _subscriberBaseId++;
            }

            public Action<object> callback = null;
            public INVOKE_COUNT InvokeCount;

            public void Recycle()
            {
                callback = null;
                _id = _subscriberBaseId++;
            }
        }

        private static long _eventBaseId = 0;

        ~Event()
        {
            Dispose();
            ObjectPoolManager.init.DestroyObjectPool<SubscribeData>();
            _trashCan.Clear();
        }

        public Event(string name)
        {
            this._name = name;
            this._id = _eventBaseId++;
            _subscriberPool = ObjectPoolManager.init.TryGetObjectPool<SubscribeData>();
        }

        private long _id;
        public long id => _id;

        private string _name = null;
        public string name => _name;

        private ObjectPool<SubscribeData> _subscriberPool = null;

        private Queue<SubscribeData> _highResponseQueue = new Queue<SubscribeData>();
        private Queue<SubscribeData> _midResponseQueue = new Queue<SubscribeData>();
        private Queue<SubscribeData> _lowResponseQueue = new Queue<SubscribeData>();

        private HashSet<long> _trashCan = new HashSet<long>();

        public void Publish(object parameters)
        {
            Distribute(_highResponseQueue, parameters);
            Distribute(_midResponseQueue, parameters);
            Distribute(_lowResponseQueue, parameters);
        }

        public long Subscribe(Action<object> callback, RESPONSIVENESS responseLevel, INVOKE_COUNT invokeCount)
        {
            var subscriber = _subscriberPool.RequestObj();
            subscriber.callback = callback;
            subscriber.InvokeCount = invokeCount;

            switch (responseLevel)
            {
                case RESPONSIVENESS.HIGH:
                    _highResponseQueue.Enqueue(subscriber);
                    break;
                case RESPONSIVENESS.MID:
                    _midResponseQueue.Enqueue(subscriber);
                    break;
                case RESPONSIVENESS.LOW:
                    _lowResponseQueue.Enqueue(subscriber);
                    break;
            }

            return subscriber.id;
        }

        public void UnSubscribe(long subscriberId)
        {
            _trashCan.Add(subscriberId);
        }

        public void Dispose()
        {
            while (_highResponseQueue.Count > 0)
            {
                _highResponseQueue.Dequeue().Recycle();
            }

            _highResponseQueue.Clear();

            while (_midResponseQueue.Count > 0)
            {
                _midResponseQueue.Dequeue().Recycle();
            }

            _midResponseQueue.Clear();

            while (_lowResponseQueue.Count > 0)
            {
                _lowResponseQueue.Dequeue().Recycle();
            }

            _lowResponseQueue.Clear();

            _subscriberPool.Dispose();
        }

        private Queue<SubscribeData> distributeTmpQueue = new Queue<SubscribeData>();

        private void Distribute(Queue<SubscribeData> subscriberQueue, object parameters)
        {
            while (subscriberQueue.Count > 0)
            {
                var subscriber = subscriberQueue.Dequeue();
                if (subscriber.callback == null || _trashCan.Contains(subscriber.id))
                {
                    _trashCan.Remove(subscriber.id);
                    _subscriberPool.RecycleObj(subscriber);
                    break;
                }

                subscriber.callback(parameters);
                if (subscriber.InvokeCount == INVOKE_COUNT.LOOP)
                    distributeTmpQueue.Enqueue(subscriber);
            }

            subscriberQueue = distributeTmpQueue.DeepCopy();
            distributeTmpQueue.Clear();
        }
    }
}