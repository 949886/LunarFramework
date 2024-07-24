// Created by LunarEclipse on 2024-1-14 11:7.

namespace Luna.Arguments
{
    public class AttackArgs
    {
        public Type type;
        public Source source;
        public int attacker;
        public int target;
        
        public enum Type
        {
            Melee,
            Ranged,
            AOE,
        }
        
        public enum Source
        {
            Character,
            Enemy,
            Environment,
        }
    }
}