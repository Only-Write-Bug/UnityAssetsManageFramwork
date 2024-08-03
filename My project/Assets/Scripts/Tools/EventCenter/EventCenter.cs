using System;
using System.Collections.Generic;

namespace Tools.EventCenter
{
    public class EventCenter
    {
        protected static long _centerId = 0;
        protected long _id;
        public long id => _id;

        public EventCenter()
        {
            _id = _centerId++;
        }

        ~EventCenter()
        {
            Dispose();
        }

        private Dictionary<string, Event> _events = new Dictionary<string, Event>();

        public void RegisterEvent(string name)
        {
            _events.TryAdd(name, new Event(name));
        }

        public void LogoutEvent(string name)
        {
            if (_events.TryGetValue(name, out var e))
            {
                e.Dispose();
                _events.Remove(name);
            }
        }

        public long Subscribe(string eventName, Action<object> callback,
            RESPONSIVENESS responseLevel = RESPONSIVENESS.MID, INVOKE_COUNT invokeCount = INVOKE_COUNT.ONE)
        {
            if (!_events.TryGetValue(eventName, out var e))
            {
                return -1;
            }

            return e.Subscribe(callback, responseLevel, invokeCount);
        }

        public void Unsubscribe(string eventName, long id)
        {
            if (!_events.TryGetValue(eventName, out var e))
            {
                return;
            }

            e.UnSubscribe(id);
        }

        public void Publish(string eventName, object parameters)
        {
            if (!_events.TryGetValue(eventName, out var e))
            {
                return;
            }

            e.Publish(parameters);
        }

        public void Dispose()
        {
            foreach (var e in _events.Values)
            {
                e.Dispose();
            }
            _events.Clear();
        }
    }

    public class GlobalEventCenter : EventCenter
    {
        private GlobalEventCenter _init = null;
        public GlobalEventCenter init => _init ??= new GlobalEventCenter();

        private GlobalEventCenter()
        {
        }
    }
}