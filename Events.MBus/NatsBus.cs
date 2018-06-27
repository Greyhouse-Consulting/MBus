using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NATS.Client;

namespace Events.MBus
{
    public interface IMBus
    {
        void SubscribeAsync<T>(Action<T> callback);
        void Publish<T>(T t);

        void ReceiveAsync<T>(Action<T> callback);

        event OnDisconnectedHandler OnDisconnected;
        event OnClosedHandler OnClosed;
    }

    public delegate void OnClosedHandler(object sender, OnClosedHandlerArgs args);

    public class OnClosedHandlerArgs
    {
        private ConnState ConnState { get; }

        public OnClosedHandlerArgs(ConnState connState)
        {
            ConnState = connState;
        }
    }

    public delegate void OnDisconnectedHandler(object sender, OnDisconnectedHandlerArgs args);

    public class OnDisconnectedHandlerArgs
    {
        private ConnState State { get; }

        public OnDisconnectedHandlerArgs(ConnState state)
        {
            State = state;
        }
    }


    public class NatsBus : IMBus, IDisposable
    {
        private IConnection _connection;

        private readonly IDictionary<Type, object> _typeSubscribers;

        public NatsBus()
        {
            _typeSubscribers = new Dictionary<Type, object>();
            Setup();
        }

        public void SubscribeAsync<T>(Action<T> callback)
        {
            if (!_typeSubscribers.ContainsKey(typeof(T)))
                _typeSubscribers.Add(typeof(T), new Subscribers<T>());

            ((Subscribers<T>)_typeSubscribers[typeof(T)]).Add(callback);

            _connection.SubscribeAsync(nameof(T), (sender, args) =>
            {
                var t = (T)new BinaryFormatter().Deserialize(new MemoryStream(args.Message.Data));

                foreach (var subscriberCallback in (Subscribers<T>)_typeSubscribers[typeof(T)])
                    subscriberCallback(t);
            });
        }

        public void Publish<T>(T t)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, t);
                _connection.Publish(nameof(T), memoryStream.GetBuffer());
            }
        }

        public void ReceiveAsync<T>(Action<T> callback)
        {
            if (!_typeSubscribers.ContainsKey(typeof(T)))
                _typeSubscribers.Add(typeof(T), new Subscribers<T>());

            ((Subscribers<T>)_typeSubscribers[typeof(T)]).Add(callback);

            _connection.SubscribeAsync(nameof(T), nameof(T), (sender, args) =>
            {
                var t = (T)new BinaryFormatter().Deserialize(new MemoryStream(args.Message.Data));

                foreach (var subscriberCallback in (Subscribers<T>)_typeSubscribers[typeof(T)])
                    subscriberCallback(t);
            });
        }

        public event OnDisconnectedHandler OnDisconnected;
        public event OnClosedHandler OnClosed;


        public void Setup()
        {
            // Create a new connection factory to create
            // a connection.
            var cf = new ConnectionFactory();

            // Creates a live connection to the default
            // NATS Server running locally
            var c = cf.CreateConnection();

            c.Opts.DisconnectedEventHandler += (sender, args) =>
            {
                OnDisconnected?.Invoke(this, new OnDisconnectedHandlerArgs(args.Conn.State));
            };

            c.Opts.ClosedEventHandler += (sender, args) =>
            {
                OnClosed?.Invoke(this, new OnClosedHandlerArgs(args.Conn.State)); 
            };


            _connection = c;
            // Setup an event handler to process incoming messages.
            // An anonymous delegate function is used for brevity.
            //EventHandler<MsgHandlerEventArgs> h = (sender, args) =>
            //{
            //    // print the message
            //    Console.WriteLine(args.Message);

            //    // Here are some of the accessible properties from
            //    // the message:
            //    // args.Message.Data;
            //    // args.Message.Reply;
            //    // args.Message.Subject;
            //    // args.Message.ArrivalSubcription.Subject;
            //    // args.Message.ArrivalSubcription.QueuedMessageCount;
            //    // args.Message.ArrivalSubcription.Queue;

            //    // Unsubscribing from within the delegate function is supported.
            //    args.Message.ArrivalSubcription.Unsubscribe();
            //};

            //// The simple way to create an asynchronous subscriber
            //// is to simply pass the event in.  Messages will start
            //// arriving immediately.
            //IAsyncSubscription s = c.SubscribeAsync("foo", h);

            //// Alternatively, create an asynchronous subscriber on subject foo,
            //// assign a message handler, then start the subscriber.   When
            //// multicasting delegates, this allows all message handlers
            //// to be setup before messages start arriving.
            //IAsyncSubscription sAsync = c.SubscribeAsync("foo");
            //sAsync.MessageHandler += h;
            //sAsync.Start();

            //// Simple synchronous subscriber
            //ISyncSubscription sSync = c.SubscribeSync("foo");

            //// Using a synchronous subscriber, gets the first message available,
            //// waiting up to 1000 milliseconds (1 second)
            //Msg m = sSync.NextMessage(1000);

            //c.Publish("foo", Encoding.UTF8.GetBytes("hello world"));

            //// Unsubscribing
            //sAsync.Unsubscribe();

            //// Publish requests to the given reply subject:
            //c.Publish("foo", "bar", Encoding.UTF8.GetBytes("help!"));

            //// Sends a request (internally creates an inbox) and Auto-Unsubscribe the
            //// internal subscriber, which means that the subscriber is unsubscribed
            //// when receiving the first response from potentially many repliers.
            //// This call will wait for the reply for up to 1000 milliseconds (1 second).
            //m = c.Request("foo", Encoding.UTF8.GetBytes("help"), 1000);

            //// Closing a connection
            //c.Close();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection?.Dispose();
        }
    }

}
