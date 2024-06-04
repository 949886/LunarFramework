// Created by LunarEclipse on 2024-1-14 9:3.

namespace Luna.Core.Gameplay
{
    public abstract class Ability
    {
        public readonly string name;
        public readonly string description;
        public readonly float cooldown;
        
        public Ability(string name, string description, float cooldown)
        {
            this.name = name;
            this.description = description;
            this.cooldown = cooldown;
        }
    }
}