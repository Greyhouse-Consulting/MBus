using System;

namespace Events.MBus
{
    public interface IMBus
    {
        void SubscribeAsync<T>(Action<T> callback);
        
        void Publish<T>(T t);

        void ReceiveAsync<T>(Action<T> callback);

        event OnDisconnectedHandler OnDisconnected;

        event OnClosedHandler OnClosed;
        
        void Send<T>(T t);
    }
}