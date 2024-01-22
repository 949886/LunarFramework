using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(RaycastTrackClip))]
[TrackBindingType(typeof(GameObject))]
public class RaycastTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<RaycastTrackMixerBehaviour>.Create (graph, inputCount);
    }
}
