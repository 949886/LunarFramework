//
//  ThreadMode.cs
//  EventBus
//
//  Created by LunarEclipse on 1/18/2019 10:55:49 PM.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Core.Event
{
    public enum ThreadMode
    {
        DEFAULT,    // Subscription will be called in the same thread.
        MAIN,       // Subscription will be called in the main thread.
        BACKGROUND, // Subscription will be called in the background thread.
        
        //ASYNC       // Subscription will be called in the different thread except main thread.
    }
}
