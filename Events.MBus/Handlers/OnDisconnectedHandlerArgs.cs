using NATS.Client;

namespace Events.MBus.Handlers
{
    public class OnDisconnectedHandlerArgs
    {
        private ConnState State { get; }

        public OnDisconnectedHandlerArgs(ConnState state)
        {
            State = state;
        }
    }
}