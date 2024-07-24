// Created by LunarEclipse on 2024-1-14 10:34.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Luna
{
    public class Event
    {
        public delegate bool EventFilter(object sender, object receiver);
        public delegate void EventHandler(object sender);
        
        public event EventHandler handler;
        public EventFilter filter;
        
        public static Event operator +(Event @event, EventHandler handler)
        {
            @event.handler += handler;
            return @event;
        }
        
        public static Event operator -(Event @event, EventHandler handler)
        {
            @event.handler -= handler;
            return @event;
        }

        public void Invoke(object sender = null)
        {
            if (filter == null) // No filter (36ms for 1,000,000 empty iterations)
                handler?.Invoke(sender);
            else if (handler != null) // With filter (300+ms for 1,000,000 empty iterations)
                foreach (EventHandler d in handler.GetInvocationList())
                    if (filter(sender, d.Target))
                        d.Invoke(sender);
        }
        
        /// Asynchronously invokes the event.
        /// Note: 5ns extra cost for single Task.
        public Task InvokeAsync(object sender = null)
        {
            return Task.Run(() => Invoke(sender));
        }
    }
    
    public class Event<T>
    {
        public delegate bool EventFilter<in T1>(object sender, object receiver, T1 args);
        
        public event EventHandler<T> handler;
        public EventFilter<T> filter;
        
        public static Event<T> operator +(Event<T> @event, EventHandler<T> handler)
        {
            @event.handler += handler;
            return @event;
        }
        
        public static Event<T> operator -(Event<T> @event, EventHandler<T> handler)
        {
            @event.handler -= handler;
            return @event;
        }
        
        public void Invoke(object sender = null, T args = default)
        {
            if (filter == null) // No filter (36ms for 1,000,000 empty iterations)
                handler?.Invoke(sender, args);
            else if (handler != null) // With filter (300+ms for 1,000,000 empty iterations)
                foreach (EventHandler<T> d in handler.GetInvocationList())
                    if (filter(sender, d.Target, args))
                        d.Invoke(sender, args);
        }
        
        /// Asynchronously invokes the event.
        /// Note: 5ns extra cost for single Task.
        public Task InvokeAsync(object sender = null, T args = default)
        {
            return Task.Run(() => Invoke(sender, args));
        }
    }
}