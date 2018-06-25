using System;

namespace Events.Messages
{
    [Serializable]
    public class DoorOpenendMessage
    {
        public string Message { get; private set; }

        public enum DoorType
        {
            Front,
            Back
        }

        public DoorType DoorTypeOpenend { get; private set; }
        public DoorOpenendMessage(DoorType doorType, string message)
        {
            Message = message;
            DoorTypeOpenend = doorType;
        }
    }
}