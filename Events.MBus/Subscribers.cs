using System;
using System.Collections.Generic;

namespace Events.MBus
{
    public class Subscribers<T> : List<Action<T>>
    {

    }
}