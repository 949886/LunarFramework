// Created by LunarEclipse on 2024-1-16 9:6.

namespace Luna.Core.Gameplay.Interface
{
    public interface IDamagable
    {
        public Attributes Attributes { get; }

        void TakeDamage(float damage)
        {
            Attributes.health.TakeDamage(damage);
        }
        
        void TakeDamageFrom(Attributes attacker, DamageType damageType)
        {
            
        }
        
        public enum DamageType
        {
            Physical,
            Magical,
            True,
            Fire,
            Water,
            Ice,
            Electric,
            Wind,
            Earth,
            Dendro,
        }
    }
}