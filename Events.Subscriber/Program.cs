using System;
using Events.MBus;
using Events.Messages;

namespace Events.Subscriber
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var r = new Random();
            var mbus = new NatsBus("Receiver-" /*+ r.Next(1000)*/);

            //mbus.SubscribeAsync<DoorOpenendMessage>(Callback);
            mbus.ReceiveAsync<DoorOpenendMessage>(Callback);

            Console.WriteLine("Hello World!");
        }

        private static void Callback(DoorOpenendMessage obj)
        {
            Console.WriteLine($"Got message {obj.Message} from sender");
        }
    }
}
