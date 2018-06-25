using System;
using System.Collections.Generic;

namespace Events.MBus
{
    public class InmemoryMBus
    {
        public InmemoryMBus()
        {
            _typeSubscribers = new Dictionary<Type, object>();
        }

        private readonly IDictionary<Type, object> _typeSubscribers;

        public void Subscribe<T>(Action<T> callback)
        {
            if (!_typeSubscribers.ContainsKey(typeof(T)))
            {
                _typeSubscribers.Add(typeof(T), new Subscribers<T>());
            }

            ((Subscribers<T>)_typeSubscribers[typeof(T)]).Add(callback);
        }

        public void Publish<T>(T t)
        {
            if (_typeSubscribers.ContainsKey(typeof(T)))
            {
                var subscribers = (Subscribers<T>)_typeSubscribers[typeof(T)];

                foreach (var subscriber in subscribers)
                {
                    subscriber(t);
                }
            }
        }
    }
}