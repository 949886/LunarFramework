#if !UNITY_2023_1_OR_NEWER
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Playables;

namespace Extensions.Timeline
{
    public static class PlayableDirectorExtensions
    {
        public static async void SetTime(this PlayableDirector director, double time)
        {
            director.playableGraph.GetRootPlayable(0).SetTime(time);
            var state = director.state;
            if (state != PlayState.Playing)
                director.Play();        
#if UNITY_2023_1_OR_NEWER
            await Awaitable.EndOfFrameAsync();
#else
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
#endif
            if (state != PlayState.Playing)
                director.Stop();
        }
        
        public static void SetTimePercentage(this PlayableDirector director, float percentage)
        {
            director.SetTime(director.duration * percentage);
        }
        
        public static void NextFrame(this PlayableDirector director, float frameRate = 60)
        {
            director.SetTime(director.time + 1.0 / frameRate);
        }
        
        public static void NextFrames(this PlayableDirector director, int frames, float frameRate = 60)
        {
            director.SetTime(director.time + frames / frameRate);
        }
    }
}