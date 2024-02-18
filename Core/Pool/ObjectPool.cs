// 
// ObjectPool.cs
// 
// 
// Created by LunarEclipse on 2023-07-15 5:02.
// Copyright © 2023 LunarEclipse. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Random = System.Random;

namespace Luna.Core.Pool
{
    public class ObjectPool
    {
        public static readonly ConditionalWeakTable<object, ObjectPool> objectPools = new();

        public readonly List<object> pooledObjects = new();
        public int capacity = 64;

        private static readonly Random _random = new();

        public static T Get<T>(T original) 
#if UNITY
        where T : UnityEngine.Object
#else
        where T : class, ICloneable
#endif
        {
            // If the object is not pooled, create a new pool.
            if (!objectPools.TryGetValue(original, out var pool))
            {
                pool = new ObjectPool();
                objectPools.Add(original, pool);
            }

            // If the pool is not full, create a new object and add it to the pool.
            @ExpandCapacity:
            if (pool.pooledObjects.Count < pool.capacity)
            {
#if UNITY
                T t = UnityEngine.Object.Instantiate(original);
#else
                T t = original.Clone() as T;
#endif
                pool.pooledObjects.Add(t);
                return t;
            }
            else
            {
                // Try to find an inactive object in the pool.
                for (int i = 0; i < 32; i++)
                {
                    var pooledObject = pool.pooledObjects[_random.Next() % pool.capacity] as T;
                    if ((pooledObject as UnityEngine.GameObject)?.activeSelf != true)
                        return pooledObject;
                }

                // If not found for 32 times, expand the capacity of the pool.
                pool.capacity += 64;
                goto @ExpandCapacity;
            }
        }
        
        public static T Get<T>() where T : class, IActivatable
        {
            // If the object is not pooled, create a new pool.
            if (!objectPools.TryGetValue(typeof(T), out var pool))
            {
                pool = new ObjectPool();
                objectPools.Add(typeof(T), pool);
            }

            // If the pool is not full, create a new object and add it to the pool.
            @ExpandCapacity:
            if (pool.pooledObjects.Count < pool.capacity)
            {
                T t = Activator.CreateInstance<T>();
                t.Active = true;
                pool.pooledObjects.Add(t);
                return t;
            }
            else
            {
                // Try to find an inactive object in the pool.
                for (int i = 0; i < 32; i++)
                    if (pool.pooledObjects[_random.Next() % pool.capacity] is T { Active: false } pooledObject)
                        return pooledObject;

                // If not found for 32 times, expand the capacity of the pool.
                pool.capacity += 64;
                goto @ExpandCapacity;
            }
        }
        
        public static ObjectPool GetPool<T>(T original)
        {
            // If the object is not pooled, create a new pool.
            if (!objectPools.TryGetValue(original, out var pool))
            {
                pool = new ObjectPool();
                objectPools.Add(original, pool);
            }

            return pool;
        }
        
        public static ObjectPool GetPool<T>() where T : class
        {
            // If the object is not pooled, create a new pool.
            if (!objectPools.TryGetValue(typeof(T), out var pool))
            {
                pool = new ObjectPool();
                objectPools.Add(typeof(T), pool);
            }

            return pool;
        }
        
        public void Release()
        {
#if UNITY
            foreach (var pooledObject in pooledObjects)
            {
                if (pooledObject is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            }
#endif
            pooledObjects.Clear();
        }
    }
}