// Created by LunarEclipse on 2024-12-25

using UnityEngine.Playables;

namespace Luna.Extensions.Unity
{
    public static class PlayableDirectorExtensions
    {
        public static void Pause(this PlayableDirector director)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        }
        
        public static void Resume(this PlayableDirector director)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
        
        public static void SetSpeed(this PlayableDirector director, float speed)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
        
        public static void SetTime(this PlayableDirector director, double time)
        {
            director.playableGraph.GetRootPlayable(0).SetTime(time);
        }
        
        public static void SetTimePercentage(this PlayableDirector director, float percentage)
        {
            director.SetTime(director.duration * percentage);
        }
    }
}