// Created by LunarEclipse on 2024-12-25

using System;
using System.Threading.Tasks;

namespace UnityEngine.Playables
{
    public static class PlayableDirectorInternalExtensions
    {
        public static bool IsPlaying(this PlayableDirector director)
        {
            return director.state == PlayState.Playing;
        }
        
        // public static void Pause(this PlayableDirector director)
        // {
        //     director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        // }
        //
        // public static void Resume(this PlayableDirector director)
        // {
        //     director.playableGraph.GetRootPlayable(0).SetSpeed(1);
        // }
        
        public static void SetSpeed(this PlayableDirector director, float speed)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
        
        public static async Task Rewind(this PlayableDirector director, float speed = 1, double time = 0)
        {
            if (time == 0)
                time = director.time / speed;
            director.SetSpeed(-speed);
            if (!director.IsPlaying())
                director.Play();
            await Task.Delay(TimeSpan.FromSeconds(time));
            director.SetSpeed(1);
            director.Stop();
        }
    }
}