// 
// ObjectPool.cs
// 
// 
// Created by LunarEclipse on 2023-07-15 5:02.
// Copyright © 2023 LunarEclipse. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Luna.Core.Pool
{
    public class ObjectPool
    {
        private static readonly Dictionary<int, ObjectPool> objectPoolMap = new();
        private static readonly Dictionary<int, WeakReference> referenceMap = new();

        public readonly List<object> pooledObjects = new();
        public int capacity = 128;

        private static readonly Random random = new();

        public static T Get<T>(T original) where T : UnityEngine.Object
        {
            // Use the hash code of the original object as the key.
            var hash = original.GetHashCode();

            // If the object is not pooled, create a new pool.
            if (!objectPoolMap.ContainsKey(hash))
            {
                var objectPool = new ObjectPool();
                objectPoolMap.Add(hash, objectPool);
                referenceMap.Add(hash, new WeakReference(original));
            }

            var pool = objectPoolMap[hash];

            // If the pool is not full, create a new object and add it to the pool.
            @ExpandCapacity:
            if (pool.pooledObjects.Count < pool.capacity)
            {
                T t = UnityEngine.Object.Instantiate(original);
                pool.pooledObjects.Add(t);
                return t;
            }
            else
            {
                // Try to find an inactive object in the pool.
                for (int i = 0; i < 32; i++)
                {
                    var pooledObject = pool.pooledObjects[random.Next() % pool.capacity] as T;
                    if ((pooledObject as GameObject)?.activeSelf != true)
                        return pooledObject;
                }

                // If not found for 32 times, expand the capacity of the pool.
                pool.capacity += 64;
                goto @ExpandCapacity;
            }
        }
    }
}