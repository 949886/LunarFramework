//
//  ThreadExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2019-01-19 07:20:51.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System.Threading;

namespace Luna.Extensions
{
    public static class ThreadExtension
    {
        private static int MAIN_THREAD_ID = Thread.CurrentThread.ManagedThreadId;

        //[DllExport]
        public static bool IsMain(this Thread thread)
        {
            return thread.ManagedThreadId == MAIN_THREAD_ID;
        }
    }
}