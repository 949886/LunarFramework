// Created by LunarEclipse on 2024-1-14 11:0.

using System;
using Luna.Arguments;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Luna
{
    public partial class Events
    {
        public static Event ApplicationQuit = new();
        public static Event ApplicationPause = new();
        public static Event ApplicationResume = new();
        public static Event ApplicationFocus = new();
        public static Event ApplicationUnFocus = new();
        
        public static Event<(Scene, LoadSceneMode)> SceneLoaded = new();
        public static Event<Scene> SceneUnloaded = new();
        public static Event<(Scene old, Scene @new)> SceneChanged = new();
      
        public static Event<AttackArgs> AttackHit = new();
        public static Event<AttackArgs> AttackMiss = new();
        public static Event<AttackArgs> AttackBlocked = new();
        public static Event<AttackArgs> AttackDodged = new();
        public static Event<AttackArgs> AttackInterrupted = new();
        
        public static Event<EventArgs> CharacterDeath = new();
        public static Event<EventArgs> CharacterRevive = new();
        public static Event<EventArgs> CharacterLevelUp = new();
        public static Event<EventArgs> CharacterJump = new();
        public static Event<EventArgs> CharacterMove = new();
        public static Event<EventArgs> CharacterStop = new();
        public static Event<EventArgs> CharacterDash = new();
        public static Event<EventArgs> CharacterTeleport = new();
        
        public static Event<EventArgs> CharacterCast = new();
        public static Event<EventArgs> CharacterCastSuccess = new();
        public static Event<EventArgs> CharacterCastFailed = new();
        public static Event<EventArgs> CharacterCastInterrupted = new();
        
        public static Event<EventArgs> CharacterSkillLearn = new();
        public static Event<EventArgs> CharacterSkillUpgrade = new();
        public static Event<EventArgs> CharacterSkillSlotChanged = new();
        public static Event<EventArgs> CharacterSkillSlotSwapped = new();
        public static Event<EventArgs> CharacterSkillSlotUnlocked = new();
        
        public static Event<EventArgs> CharacterBuffApplied = new();
        public static Event<EventArgs> CharacterBuffRemoved = new();
        public static Event<EventArgs> CharacterBuffExpired = new();
        public static Event<EventArgs> CharacterBuffRefreshed = new();
        
        public static Event<EventArgs> CharacterDebuffApplied = new();
        public static Event<EventArgs> CharacterDebuffRemoved = new();
        public static Event<EventArgs> CharacterDebuffExpired = new();
        public static Event<EventArgs> CharacterDebuffRefreshed = new();
        
        public static Event<EventArgs> EnemyDeath = new();
        public static Event<EventArgs> EnemyRevive = new();
        public static Event<EventArgs> EnemySpawn = new();
        public static Event<EventArgs> EnemyDespawn = new();
        
        public static Event<EventArgs> EnemyCast = new();
        public static Event<EventArgs> EnemyCastSuccess = new();
        public static Event<EventArgs> EnemyCastFailed = new();
        public static Event<EventArgs> EnemyCastInterrupted = new();
        
        public static Event<EventArgs> EnemySkillLearn = new();
        public static Event<EventArgs> EnemySkillUpgrade = new();
        public static Event<EventArgs> EnemySkillSlotChanged = new();
        public static Event<EventArgs> EnemySkillSlotSwapped = new();
        public static Event<EventArgs> EnemySkillSlotUnlocked = new();
        
        public static Event<EventArgs> EnemyBuffApplied = new();
        public static Event<EventArgs> EnemyBuffRemoved = new();
        public static Event<EventArgs> EnemyBuffExpired = new();
        public static Event<EventArgs> EnemyBuffRefreshed = new();
        
        public static Event<EventArgs> EnemyDebuffApplied = new();
        public static Event<EventArgs> EnemyDebuffRemoved = new();
        public static Event<EventArgs> EnemyDebuffExpired = new();
        public static Event<EventArgs> EnemyDebuffRefreshed = new();
        
        public static Event<EventArgs> ItemUsed = new();
        public static Event<EventArgs> ItemEquipped = new();
        public static Event<EventArgs> ItemUnequipped = new();
        public static Event<EventArgs> ItemAdded = new();
        public static Event<EventArgs> ItemRemoved = new();
        public static Event<EventArgs> ItemDropped = new();
        
        public static Event<EventArgs> QuestAccepted = new();
        public static Event<EventArgs> QuestCompleted = new();
        public static Event<EventArgs> QuestFailed = new();
        public static Event<EventArgs> QuestAbandoned = new();
        public static Event<EventArgs> QuestObjectiveCompleted = new();
        
        public static Event<EventArgs> PlayerLevelUp = new();
        public static Event<EventArgs> PlayerLevelDown = new();

#if UNITY
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            // Application events
            Application.quitting += () => ApplicationQuit.Invoke(null);
            Application.focusChanged += (focused) => (focused ? ApplicationFocus : ApplicationUnFocus).Invoke(null);
#if UNITY_EDITOR
            EditorApplication.pauseStateChanged += (pause) => (pause == PauseState.Paused ? ApplicationPause : ApplicationResume).Invoke(null);
#endif
            
            // Scene events
            SceneManager.sceneLoaded += (scene, mode) => SceneLoaded.Invoke(null, (scene, mode));
            SceneManager.sceneUnloaded += (scene) => SceneUnloaded.Invoke(null, scene);
            SceneManager.activeSceneChanged += (oldScene, newScene) => SceneChanged.Invoke(null, (oldScene, newScene));
            // SceneManager.sceneSaved += (scene) => SceneSaved.Invoke(null);
            
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
            
        }
#endif
    }
}