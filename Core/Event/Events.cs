// Created by LunarEclipse on 2024-1-14 11:0.

using System;
using Luna.Core.Event.Arguments;

namespace Luna.Core.Event
{
    public class Events
    {
        public static Event<EventArgs> ApplicationQuit = new Event<EventArgs>();
        public static Event<EventArgs> ApplicationPause = new Event<EventArgs>();
        public static Event<EventArgs> ApplicationResume = new Event<EventArgs>();
        public static Event<EventArgs> ApplicationFocus = new Event<EventArgs>();
        public static Event<EventArgs> ApplicationUnFocus = new Event<EventArgs>();
        
        public static Event<EventArgs> SceneLoaded = new Event<EventArgs>();
        public static Event<EventArgs> SceneUnloaded = new Event<EventArgs>();
        public static Event<EventArgs> SceneChanged = new Event<EventArgs>();
        public static Event<EventArgs> SceneSaved = new Event<EventArgs>();
      
        public static Event<AttackArgs> AttackHit = new Event<AttackArgs>();
        public static Event<AttackArgs> AttackMiss = new Event<AttackArgs>();
        public static Event<AttackArgs> AttackBlocked = new Event<AttackArgs>();
        public static Event<AttackArgs> AttackDodged = new Event<AttackArgs>();
        public static Event<AttackArgs> AttackInterrupted = new Event<AttackArgs>();
        
        public static Event<EventArgs> CharacterDeath = new Event<EventArgs>();
        public static Event<EventArgs> CharacterRevive = new Event<EventArgs>();
        public static Event<EventArgs> CharacterLevelUp = new Event<EventArgs>();
        public static Event<EventArgs> CharacterJump = new Event<EventArgs>();
        public static Event<EventArgs> CharacterMove = new Event<EventArgs>();
        public static Event<EventArgs> CharacterStop = new Event<EventArgs>();
        public static Event<EventArgs> CharacterDash = new Event<EventArgs>();
        public static Event<EventArgs> CharacterTeleport = new Event<EventArgs>();
        
        public static Event<EventArgs> CharacterCast = new Event<EventArgs>();
        public static Event<EventArgs> CharacterCastSuccess = new Event<EventArgs>();
        public static Event<EventArgs> CharacterCastFailed = new Event<EventArgs>();
        public static Event<EventArgs> CharacterCastInterrupted = new Event<EventArgs>();
        
        public static Event<EventArgs> CharacterSkillLearn = new Event<EventArgs>();
        public static Event<EventArgs> CharacterSkillUpgrade = new Event<EventArgs>();
        public static Event<EventArgs> CharacterSkillSlotChanged = new Event<EventArgs>();
        public static Event<EventArgs> CharacterSkillSlotSwapped = new Event<EventArgs>();
        public static Event<EventArgs> CharacterSkillSlotUnlocked = new Event<EventArgs>();
        
        public static Event<EventArgs> CharacterBuffApplied = new Event<EventArgs>();
        public static Event<EventArgs> CharacterBuffRemoved = new Event<EventArgs>();
        public static Event<EventArgs> CharacterBuffExpired = new Event<EventArgs>();
        public static Event<EventArgs> CharacterBuffRefreshed = new Event<EventArgs>();
        
        public static Event<EventArgs> CharacterDebuffApplied = new Event<EventArgs>();
        public static Event<EventArgs> CharacterDebuffRemoved = new Event<EventArgs>();
        public static Event<EventArgs> CharacterDebuffExpired = new Event<EventArgs>();
        public static Event<EventArgs> CharacterDebuffRefreshed = new Event<EventArgs>();
        
        public static Event<EventArgs> EnemyDeath = new Event<EventArgs>();
        public static Event<EventArgs> EnemyRevive = new Event<EventArgs>();
        public static Event<EventArgs> EnemySpawn = new Event<EventArgs>();
        public static Event<EventArgs> EnemyDespawn = new Event<EventArgs>();
        
        public static Event<EventArgs> EnemyCast = new Event<EventArgs>();
        public static Event<EventArgs> EnemyCastSuccess = new Event<EventArgs>();
        public static Event<EventArgs> EnemyCastFailed = new Event<EventArgs>();
        public static Event<EventArgs> EnemyCastInterrupted = new Event<EventArgs>();
        
        public static Event<EventArgs> EnemySkillLearn = new Event<EventArgs>();
        public static Event<EventArgs> EnemySkillUpgrade = new Event<EventArgs>();
        public static Event<EventArgs> EnemySkillSlotChanged = new Event<EventArgs>();
        public static Event<EventArgs> EnemySkillSlotSwapped = new Event<EventArgs>();
        public static Event<EventArgs> EnemySkillSlotUnlocked = new Event<EventArgs>();
        
        public static Event<EventArgs> EnemyBuffApplied = new Event<EventArgs>();
        public static Event<EventArgs> EnemyBuffRemoved = new Event<EventArgs>();
        public static Event<EventArgs> EnemyBuffExpired = new Event<EventArgs>();
        public static Event<EventArgs> EnemyBuffRefreshed = new Event<EventArgs>();
        
        public static Event<EventArgs> EnemyDebuffApplied = new Event<EventArgs>();
        public static Event<EventArgs> EnemyDebuffRemoved = new Event<EventArgs>();
        public static Event<EventArgs> EnemyDebuffExpired = new Event<EventArgs>();
        public static Event<EventArgs> EnemyDebuffRefreshed = new Event<EventArgs>();
        
        public static Event<EventArgs> ItemUsed = new Event<EventArgs>();
        public static Event<EventArgs> ItemEquipped = new Event<EventArgs>();
        public static Event<EventArgs> ItemUnequipped = new Event<EventArgs>();
        public static Event<EventArgs> ItemAdded = new Event<EventArgs>();
        public static Event<EventArgs> ItemRemoved = new Event<EventArgs>();
        public static Event<EventArgs> ItemDropped = new Event<EventArgs>();
        
        public static Event<EventArgs> QuestAccepted = new Event<EventArgs>();
        public static Event<EventArgs> QuestCompleted = new Event<EventArgs>();
        public static Event<EventArgs> QuestFailed = new Event<EventArgs>();
        public static Event<EventArgs> QuestAbandoned = new Event<EventArgs>();
        public static Event<EventArgs> QuestObjectiveCompleted = new Event<EventArgs>();
        
        public static Event<EventArgs> PlayerLevelUp = new Event<EventArgs>();
        public static Event<EventArgs> PlayerLevelDown = new Event<EventArgs>();
    }
}