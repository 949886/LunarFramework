// Created by LunarEclipse on 2025-02-13

using System;
using System.Threading.Tasks;

namespace Luna.Extensions
{
    public static class TaskExtensions
    {
        public static async void Then(this Task task, Action<Task> action)
        {
            await task;
            action(task);
        }
        
        public static async Task<U> Then<T,U>(this Task<T> task, Func<T,U> func)
        {
            var result = await task;
            return func(result);
        }
    }
}