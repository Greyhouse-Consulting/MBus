using System;
using Events.MBus;
using Events.Messages;

namespace Events.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var mbus = new NatsBus();

            mbus.SubscribeAsync<DoorOpenendMessage>(Callback);

            Console.WriteLine("Hello World!");
        }

        private static void Callback(DoorOpenendMessage obj)
        {
            Console.WriteLine($"Got message {obj.Message} from sender");
        }
    }
}
