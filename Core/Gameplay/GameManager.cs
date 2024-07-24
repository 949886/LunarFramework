// Created by LunarEclipse on 2024-1-16 7:15.

using System.Collections.Generic;
using Luna.Arguments;
using UnityEngine;

namespace Luna.Core.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        private Dictionary<object, Entity> entities = new();

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            InitEntities();
            RegisterEvents();
            var a = entities[1];
            gameObject.GetHashCode();
        }

        private void InitEntities()
        {
            
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }
        
        private void Update()
        {
            foreach (var entity in entities.Values)
            {
                entity.Update();
            }
        }
        
        private void RegisterEvents()
        {
            Events.AttackHit += OnAttackHit;
        }

        private void UnregisterEvents()
        {
            Events.AttackHit -= OnAttackHit;
        }

        #region Event Handlers

        private void OnAttackHit(object sender, AttackArgs e)
        {
            if (e.source == AttackArgs.Source.Character)
            {
                if (entities.ContainsKey(e.target))
                {
                    var entity = entities[e.target];
                    entity.attributes.health.TakeDamage(10);
                }
            }
        }

        #endregion
        
    }
}