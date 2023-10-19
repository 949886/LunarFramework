//
//  EventBus.cs
//  EventBus
//
//  Created by LunarEclipse on 2018-11-29.
//  Copyright © 2018 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Luna.Core.Dispatch;
using Luna.Extensions;

#if UNITY
using UnityEngine;
#endif

namespace Luna.Core.Event
{
    public class EventBus
    {
        public static EventBus Default = new EventBus();

        // Parameter's type of subscriber method as key, method delegates as value.
        private Dictionary<Type, List<Subscription>> eventHandlers = new Dictionary<Type, List<Subscription>>();

        public EventBus() { }

        ~EventBus() { }

        public void Register(object listener)
        {
            bool hasAttribute = false;

            // Check [Subscribe] attributes of listener.
            foreach (MethodInfo method in listener.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                foreach (Attribute attribute in method.GetCustomAttributes(true))
                {
#if UNITY                    
                    // Debug.Log("Register: " + method);
#else
                    Console.WriteLine("Register: " + method);
#endif
                    Subscribe subscribeInfo = attribute as Subscribe;
                    if (subscribeInfo == null)
                        continue;
                    hasAttribute = true;

                    var parameters = method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        var action = method.CreateDelegate(listener);

                        if (!eventHandlers.ContainsKey(parameters[0].ParameterType))
                            eventHandlers[parameters[0].ParameterType] = new List<Subscription>();
                        var subscription = new Subscription(listener, parameters[0].ParameterType, subscribeInfo, action);
                        eventHandlers[parameters[0].ParameterType].Add(subscription);

                    }
                    else throw new Exception("Subscribe method must have only one parameter!");
                }

            if (!hasAttribute)
                throw new Exception("Subscribe method not found!");
        }

        public void Unregister(object listener)
        {
            foreach (MethodInfo method in listener.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                foreach (Attribute attribute in method.GetCustomAttributes(true))
                {
#if UNITY
                    Debug.Log("Unregister: " + method);
#else
                    Console.WriteLine("Unregister: " + method);
#endif
                    if (!(attribute is Subscribe))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        var action = method.CreateDelegate(listener);
                        var type = parameters[0].ParameterType;

                        if (eventHandlers.TryGetValue(type, out var handler))
                            handler.RemoveAll(item => item.Handler == action);
                    }
                    else throw new Exception("Subscribe method must have only one parameter!");
                }
        }

        public void UnregisterAll()
        {
            eventHandlers.Clear();
        }

        public void Post(object newEvent)
        {
            var subscriptions = eventHandlers.Get(newEvent.GetType());
            if (subscriptions == null)
                return;

            foreach (Subscription sub in subscriptions)
            {
                //try { sub.Handler.DynamicInvoke(newEvent); } /* Old implementation without thread mode */
                try { Invoke(sub, newEvent); } 
                catch (Exception e)
                {
#if UNITY
                    Debug.Log(e);
#else
                    Console.WriteLine(e);
#endif
                    throw;
                }
            }
        }

        private void Invoke(Subscription subscription, object newEvent)
        {
            switch (subscription.Subscribe.threadMode)
            {
                case ThreadMode.DEFAULT:
                    subscription.Handler.DynamicInvoke(newEvent);
                    break;
                case ThreadMode.MAIN:
                    DispatchQueue.Main.Async(() => {
                        subscription.Handler.DynamicInvoke(newEvent);
                    });
                    break;
                case ThreadMode.BACKGROUND:
                    DispatchQueue.Global.Async(() => {
                        subscription.Handler.DynamicInvoke(newEvent);
                    });
                    break;
            }
        }


    }

}
