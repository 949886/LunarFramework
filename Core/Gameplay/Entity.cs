// Created by LunarEclipse on 2024-1-16 7:56.

using Luna.Core.Pool;

namespace Luna.Core.Gameplay
{
    public abstract class Entity: IActivatable
    {
        public Attributes attributes;
        
        public bool Active { get; set; }
        
        public void Update()
        {
            
        }

        public void FixedUpdate()
        {

        }
    }
}