//
//  Subscription.cs
//  EventBus
//
//  Created by LunarEclipse on 1/19/2019 12:13:56 AM.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using Luna.Core.Dispatch;

namespace Luna.Core.Event
{
    /// <summary>
    /// Class using to store info of subscriber.
    /// </summary>
    public class Subscription
    {
        public object Subscriber => subscriber.Target;
        public Type SubscribeType => subscribeType;
        public Subscribe Subscribe => subscribe;
        public Action<IMessage> Handler => handler;

        private WeakReference subscriber;
        private Type subscribeType;
        private Subscribe subscribe;
        private Action<IMessage> handler;

        public Subscription(object subscriber, Type subscribeType, Subscribe subscribe, Action<IMessage> handler)
        {
            this.subscriber = new WeakReference(subscriber);
            this.subscribeType = subscribeType;
            this.subscribe = subscribe;
            this.handler = handler;
        }

        public void Invoke(IMessage newEvent)
        {
            var subscription = this;

            switch (subscription.Subscribe.threadMode)
            {
                case ThreadMode.DEFAULT:
                    subscription.Handler.Invoke(newEvent);
                    break;
                case ThreadMode.MAIN:
                    DispatchQueue.Main.Async(() => {
                        subscription.Handler.Invoke(newEvent);
                    });
                    break;
                case ThreadMode.BACKGROUND:
                    DispatchQueue.Global.Async(() => {
                        subscription.Handler.Invoke(newEvent);
                    });
                    break;
            }
        }
    }
}