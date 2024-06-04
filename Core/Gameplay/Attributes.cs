// Created by LunarEclipse on 2024-1-14 9:16.

namespace Luna.Core.Gameplay
{
    public partial class Attributes
    {
        public readonly Health health;
        public readonly Mana mana;
        public readonly Movement movement;
        public readonly Defense defense;
        public readonly Attack attack;
        public readonly Cooldown cooldown;
        public readonly Resource resource;

        public Attributes(Health health, Mana mana, Movement movement, Defense defense, Attack attack, Cooldown cooldown, Resource resource)
        {
            this.health = health;
            this.mana = mana;
            this.movement = movement;
            this.defense = defense;
            this.attack = attack;
            this.cooldown = cooldown;
            this.resource = resource;
        }
        
        public class Health
        {
            public float currentHealth;
            public float maxHealth;
            public float healthRegen;

            public Health(float maxHealth, float healthRegen)
            {
                this.maxHealth = maxHealth;
                this.healthRegen = healthRegen;
            }
            
            public void TakeDamage(float damage)
            {
                currentHealth -= damage;
            }
            
            public void Heal(float amount)
            {
                currentHealth += amount;
            }
            
            public static Health operator +(Health health, float amount)
            {
                health.currentHealth += amount;
                return health;
            }
            
            public static Health operator -(Health health, float amount)
            {
                health.currentHealth -= amount;
                return health;
            }
            
            public static implicit operator float(Health health)
            {
                return health.currentHealth;
            }
        }

        public class Mana
        {
            public readonly float maxMana;
            public readonly float manaRegen;

            public Mana(float maxMana, float manaRegen)
            {
                this.maxMana = maxMana;
                this.manaRegen = manaRegen;
            }
        }

        public class Movement
        {
            public readonly float moveSpeed;

            public Movement(float moveSpeed)
            {
                this.moveSpeed = moveSpeed;
            }
        }

        public class Defense
        {
            public readonly float armor;
            public readonly float magicResist;

            public Defense(float armor, float magicResist)
            {
                this.armor = armor;
                this.magicResist = magicResist;
            }
        }

        public class Attack
        {
            public readonly float attackDamage;
            public readonly float abilityPower;
            public readonly float attackSpeed;
            public readonly float critChance;
            public readonly float critDamage;

            public Attack(float attackDamage, float abilityPower, float attackSpeed, float critChance, float critDamage)
            {
                this.attackDamage = attackDamage;
                this.abilityPower = abilityPower;
                this.attackSpeed = attackSpeed;
                this.critChance = critChance;
                this.critDamage = critDamage;
            }
        }

        public class Cooldown
        {
            public readonly float cooldownReduction;

            public Cooldown(float cooldownReduction)
            {
                this.cooldownReduction = cooldownReduction;
            }
        }

        public class Resource
        {
            public readonly float resource;
            public readonly float resourceRegen;

            public Resource(float resource, float resourceRegen)
            {
                this.resource = resource;
                this.resourceRegen = resourceRegen;
            }
        }
        
        public class ElementalPower
        {
            public readonly float fire;
            public readonly float water;
            public readonly float earth;
            public readonly float wind;
            public readonly float electric;
            public readonly float ice;
            public readonly float light;
            public readonly float darkness;

            public ElementalPower(float fire, float water, float earth, float wind, float electric, float ice, float light, float darkness)
            {
                this.fire = fire;
                this.water = water;
                this.earth = earth;
                this.wind = wind;
                this.electric = electric;
                this.ice = ice;
                this.light = light;
                this.darkness = darkness;
            }
        }
        
        public class ElementalResistance
        {
            public readonly float fire;
            public readonly float water;
            public readonly float earth;
            public readonly float wind;
            public readonly float electric;
            public readonly float ice;
            public readonly float light;
            public readonly float darkness;

            public ElementalResistance(float fire, float water, float earth, float wind, float electric, float ice, float light, float darkness)
            {
                this.fire = fire;
                this.water = water;
                this.earth = earth;
                this.wind = wind;
                this.electric = electric;
                this.ice = ice;
                this.light = light;
                this.darkness = darkness;
            }
        }
    }
}