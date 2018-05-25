using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transparity.C2C.Client.Example
{
    public enum SubscriptionActionEnum
    {
        // not implemented
        Reserved = 0,

        // implemented
        NewSubscription,

        // not implemented
        ReplaceSubscription,

        // implemented
        CancelSubscription,

        // implemented
        CancelAllPriorSubscriptions
    }

    public enum SubscriptionTypeEnum
    {
        Reserved = 0,
        OneTime,
        Periodic,
        OnChange,
    }
}
