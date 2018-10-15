using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Events.MBus.Handlers;
using STAN.Client;

namespace Events.MBus
{
    public delegate void OnClosedHandler(object sender, OnClosedHandlerArgs args);

    public delegate void OnDisconnectedHandler(object sender, OnDisconnectedHandlerArgs args);

    public class NatsBus : IMBus, IDisposable
    {
        private IStanConnection _connection;

        private readonly IDictionary<Type, object> _typeSubscribers;
        protected internal static BinaryFormatter BinaryFormatter;


        static NatsBus()
        {
            BinaryFormatter = new BinaryFormatter();
        }

        public NatsBus(string appname)
        {
            _typeSubscribers = new Dictionary<Type, object>();

            Setup(appname);
        }

        public void SubscribeAsync<T>(Action<T> callback)
        {
            if (!_typeSubscribers.ContainsKey(typeof(T)))
                _typeSubscribers.Add(typeof(T), new Subscribers<T>());

            ((Subscribers<T>)_typeSubscribers[typeof(T)]).Add(callback);
        }

        public void Publish<T>(T t)
        {
            Publish(nameof(T), t);
        }

        public void Publish<T>(string queueName, T t)
        {
            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentException("invalid queue name", nameof(queueName));

            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, t);
                _connection.Publish(queueName, memoryStream.GetBuffer());
            }
        }

        public void ReceiveAsync<T>(string queueName, Action<T> callback)
        {
            if(string.IsNullOrEmpty(queueName))
                throw new ArgumentException("Invalid queue name", nameof(queueName));

            var opts = StanSubscriptionOptions.GetDefaultOptions();

            opts.DurableName = queueName;

            if (!_typeSubscribers.ContainsKey(typeof(T)))
                _typeSubscribers.Add(typeof(T), new Subscribers<T>());

            ((Subscribers<T>)_typeSubscribers[typeof(T)]).Add(callback);

            _connection.Subscribe(queueName, opts, (sender, args) =>
            {
                var t = (T)BinaryFormatter.Deserialize(new MemoryStream(args.Message.Data));

                foreach (var subscriberCallback in (Subscribers<T>)_typeSubscribers[typeof(T)])
                    subscriberCallback(t);
            });

        }

        public void ReceiveAsync<T>(Action<T> callback)
        {
            ReceiveAsync(nameof(T), callback);
        }

        public event OnDisconnectedHandler OnDisconnected;
        public event OnClosedHandler OnClosed;


        public void Setup(string appname)
        {
            var cf = new StanConnectionFactory();

            _connection = cf.CreateConnection("test-cluster", appname);

            _connection.NATSConnection.Opts.DisconnectedEventHandler += (sender, args) =>
            {
                OnDisconnected?.Invoke(this, new OnDisconnectedHandlerArgs(args.Conn.State));
            };

            _connection.NATSConnection.Opts.ClosedEventHandler += (sender, args) =>
            {
                OnClosed?.Invoke(this, new OnClosedHandlerArgs(args.Conn.State)); 
            };
        }

        public void Dispose()
        {
            _connection.Close();
            _connection?.Dispose();
        }

        public void Send<T>(T t)
        {
            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter.Serialize(memoryStream, t);
                _connection.Publish(nameof(T), memoryStream.GetBuffer());
            }
        }
    }
}
