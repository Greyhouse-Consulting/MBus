using System;
using System.Threading;
using Events.MBus;
using Events.Messages;

namespace Events
{
    class Program
    {
        static void Main(string[] args)
        {
            //var bus = new InmemoryMBus();
            //bus.Subscribe<DoorOpenendMessage>(Callback);
            //var v = new Vehicle(bus);

            //v.FrontDoorEvent += VOnFrontDoorEvent;

            //v.Open(new FrontDoor());

            var mbus = new NatsBus();

            mbus.OnDisconnected += (sender, handlerArgs) => { Console.WriteLine("Bloody hell! Disconnected!"); };

            //mbus.SubscribeAsync< DoorOpenendMessage>(Callback);


            for (int i = 0; i < 10; i++)
            {
                mbus.Publish(new DoorOpenendMessage(DoorOpenendMessage.DoorType.Front, $"FHellon! {i}"));

                Thread.Sleep(1000);
            }

        }

        private static void Callback(DoorOpenendMessage obj)
        {
            Console.WriteLine($"Got message {obj.Message}");
        }


        private static void VOnFrontDoorEvent(object sender, FrontDoorOpenEventArgs args)
        {
            Console.WriteLine("Frontdoor Opened!");
        }
    }


    public class Vehicle
    {
        private readonly InmemoryMBus _bus;

        public Vehicle(InmemoryMBus bus)
        {
            _bus = bus;
        }

        public void Open(Door door)
        {
            if (door is FrontDoor)
            {
                OnFrontDoorEvent(new FrontDoorOpenEventArgs());
            }
        }

        public event FrontDoorOpenEvent FrontDoorEvent;

        protected virtual void OnFrontDoorEvent(FrontDoorOpenEventArgs args)
        {
            FrontDoorEvent?.Invoke(this, args);

            _bus.Publish(new DoorOpenendMessage(DoorOpenendMessage.DoorType.Front, "Fellon!"));
        }
    }


    public delegate void FrontDoorOpenEvent(object sender, FrontDoorOpenEventArgs args);

    public class FrontDoorOpenEventArgs : EventArgs
    {
    }

    public class Door
    {
    }

    class FrontDoor : Door
    {
    }
}
