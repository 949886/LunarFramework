// 
// ObjectPoolUnit.cs
// 
// 
// Created by LunarEclipse on 2023-07-15 5:31.
// Copyright © 2023 LunarEclipse. All rights reserved.

using Luna.Core.Pool;
using UnityEngine;
using UnityEngine.Rendering;

#if USE_VISUAL_SCRIPTING

using Unity.VisualScripting;

namespace Luna.Extensions.VisualScripting
{

    [UnitTitle("Object Pool")]
    [UnitSubtitle("Get a object from pool.")]
    [UnitCategory("Luna/Optimization")]
    [TypeIcon(typeof(Volume))]
    public class ObjectPoolNode : ScriptableUnit
    {
        [DoNotSerialize]
        public ValueInput prefab;
        public ValueOutput replicaOut;

        private GameObject replica;

        protected override void Definition()
        {
            base.Definition();

            prefab = ValueInput<UnityEngine.GameObject>("prefab", null);
            replicaOut = ValueOutput<UnityEngine.GameObject>("replica", (flow) => { return replica; });
        }

        public override void Execute(Flow flow)
        {
            var original = flow.GetValue<UnityEngine.GameObject>(prefab);
            replica = ObjectPool.Get(original);
        }
    }

}

#endif
