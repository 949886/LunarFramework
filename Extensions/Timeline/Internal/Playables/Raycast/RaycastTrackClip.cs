using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

[Serializable]
public class RaycastTrackClip : PlayableAsset, ITimelineClipAsset
{
    public MonoBehaviour raycaster;
    public RaycastTrackBehaviour behaviour = new();
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    private void Awake()
    {
        behaviour.Clip = this;
        behaviour.Source = raycaster;
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RaycastTrackBehaviour>.Create (graph, behaviour);
        RaycastTrackBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
    
}