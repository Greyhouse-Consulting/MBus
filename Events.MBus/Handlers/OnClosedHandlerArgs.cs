using NATS.Client;

namespace Events.MBus.Handlers
{
    public class OnClosedHandlerArgs
    {
        private ConnState ConnState { get; }

        public OnClosedHandlerArgs(ConnState connState)
        {
            ConnState = connState;
        }
    }
}