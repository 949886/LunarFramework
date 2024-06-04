// Created by LunarEclipse on 2024-1-14 10:34.

using System;

namespace Luna.Core.Event
{
    public delegate bool EventFilter<in T>(object sender, T args);
    
    public class Event<T>
    {
        public event EventHandler<T> @event;
        public EventFilter<T> filter;
        
        public static Event<T> operator +(Event<T> @event, EventHandler<T> handler)
        {
            @event.@event += handler;
            return @event;
        }
        
        public static Event<T> operator -(Event<T> @event, EventHandler<T> handler)
        {
            @event.@event -= handler;
            return @event;
        }
        
        public void Invoke(object sender, T args)
        {
            if (filter != null && !filter(sender, args)) return;
            @event?.Invoke(sender, args);
        }
    }
}