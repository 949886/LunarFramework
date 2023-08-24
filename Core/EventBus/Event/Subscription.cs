//
//  Subscription.cs
//  EventBus
//
//  Created by LunarEclipse on 1/19/2019 12:13:56 AM.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Luna.Extensions;

namespace Luna.Core.Event
{
    /// <summary>
    /// Class using to store info of subscriber.
    /// </summary>
    public struct Subscription
    {
        public object Subscriber => subscriber.Target;
        public Type SubscribeType => subscribeType;
        public Subscribe Subscribe => subscribe;
        public Delegate Handler => handler;

        private WeakReference subscriber;
        private Type subscribeType;
        private Subscribe subscribe;
        private Delegate handler;
        
        public Subscription(object subscriber, Type subscribeType, Subscribe subscribe, Delegate handler)
        {
            this.subscriber = new WeakReference(subscriber);
            this.subscribeType = subscribeType;
            this.subscribe = subscribe;
            this.handler = handler;
        }
    }
}
