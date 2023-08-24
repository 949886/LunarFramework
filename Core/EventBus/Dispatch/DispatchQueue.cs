//
//  DispatchQueue.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-13 16:47:15.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY
using UnityEngine;
#endif

namespace Luna.Core.Dispatch
{
    public class DispatchQueue
#if UNITY
        : MonoBehaviour
#endif
    {

#region Singleton

        private static DispatchQueue global;
        private static DispatchQueue main;


        /// <summary>
        /// The dispatch queue associated with the background threads of the current process.
        /// </summary>
        public static DispatchQueue Global
        {
            get
            {
                if (global == null)
                    global = new DispatchQueue();
                return global;
            }
        }

        /// <summary>
        /// The dispatch queue associated with the main thread of the current process.
        /// </summary>
        public static DispatchQueue Main
        {
            get
            {
                if (main == null)
                {
                    main = new DispatchQueue();
                    main.threadMode = ThreadMode.Main;
                }
                return main;
            }
        }

#if UNITY
        /// <summary>
        /// Initialize main queue before scene load.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (main == null || main.gameObject == null)
            {
                main = new GameObject("DispatchQueue").AddComponent<DispatchQueue>();
                main.threadMode = ThreadMode.Main;
                DontDestroyOnLoad(main.gameObject);
            }
        }
#endif

#endregion

        private readonly Queue<Action> executionQueue = new Queue<Action>();

        private ThreadMode threadMode = ThreadMode.Background;
        private DispatchQueueType type = DispatchQueueType.Consecutive; //TODO: Never used.


        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void Async(Action action)
        {
            if (threadMode == ThreadMode.Background)
                Task.Run(() => action());
            else
            {
                lock (executionQueue)
                {
                    //executionQueue.Enqueue(() => {
                    //    StartCoroutine(ActionWrapper(action));
                    //});
                    executionQueue.Enqueue(action);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        public void AsyncAfter(double delay, Action action)
        {
            if (threadMode == ThreadMode.Background)
                Task.Run(async delegate
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(delay));
                    action();
                });
            else
            {
                lock (executionQueue)
                {
                    //executionQueue.Enqueue(() =>
                    //{
                    //    StartCoroutine(DelayedActionWrapper((float)delay, action));
                    //});
                    executionQueue.Enqueue(action);
                }
            }
        }

        //IEnumerator ActionWrapper(Action action)
        //{
        //    action();
        //    yield return null;
        //}

        //IEnumerator DelayedActionWrapper(float delay, Action action)
        //{
        //    yield return new WaitForSeconds(delay / 1000);
        //    action();
        //    yield return null;
        //}

        /// <summary>
        /// In unity, this method will run as override method within unity script lifecycle.
        /// Otherwise, you should invoke this method explicitly in event loop of main thread.
        /// </summary>
        public void Update()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }

#region Enumeration

        private enum ThreadMode
        {
            Main,       // Associated with the main thread.
            Background  // associated with background threads.
        }

        private enum DispatchQueueType
        {
            Consecutive, // Serial queue, by default.
            Concurrent   // Execute concurrently.
        }

#endregion

    }
}