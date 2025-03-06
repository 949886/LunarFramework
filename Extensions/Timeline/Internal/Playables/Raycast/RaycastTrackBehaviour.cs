using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class RaycastTrackBehaviour : PlayableBehaviour
{
    public int count = 5;
    public float range = 40f;   // in angle.
    public float distance = 1f;
    
    private float interval = 10;    // in angle.
    
    private int _index = 0;
    private GameObject _origin;

    public PlayableAsset Clip { private get; set; }
    public MonoBehaviour Source { private get; set; }
    
    public override void OnPlayableCreate (Playable playable)
    {
    }
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _index = 0;
        _origin = info.output.GetUserData() as GameObject;
        if (_origin != null)
        {
            // Do something with the trackBindingObject
            Debug.Log("Track Binding Object: " + _origin.name);
        }
        else
        {
            Debug.LogError("Track Binding Object not found");
        }
    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        Debug.Log("OnBehaviourPause");
        if (_origin != null)
            while (CastrayByRange(range, 1, _origin.transform));
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData == null)
        {
            Debug.Log("ProcessFrame with null playerData");
        }
        else
        {
            var currentTime = playable.GetTime();
            var duration = playable.GetDuration();
            var progress = currentTime / duration;

            // while (CastrayByInterval(interval, progress, raycaster.transform));
            while (CastrayByRange(range, progress, _origin.transform));
        }
    }

    private bool CastrayByInterval(float interval, double progress, Transform origin)
    {
        var sectorAngle = Math.Max(count - 1, 0) * interval;
        var nextAngle = interval * (_index - (count - 1) * 0.5f);
        var forwardRotation = Quaternion.Euler(0, nextAngle, 0) * origin.forward;

        Debug.Log("sectorAngle:" + sectorAngle + " nextAngle:" + nextAngle + " process:" + ((progress - 0.5f) * sectorAngle));
        if ((progress - 0.5f) * sectorAngle - nextAngle > -0.02 * sectorAngle)
        {
            _index++;
            Debug.DrawLine(
                origin.position, 
                origin.position + forwardRotation, 
                new Color(0, 0.5f, 0, 1),
                2f);

            return true;
        }
        
        return false;
    }
    
    private bool CastrayByRange(float range, double progress, Transform origin)
    {
        var currentIndex = (int)Math.Floor(progress * count);
        // Debug.Log("index: " + currentIndex + " _index" + _index + " progress:" + progress);
        if (currentIndex > _index)
        {
            var interval = range / Math.Max(count - 1, 0);
            var nextAngle = interval * (_index - (count - 1) * 0.5f);
            var forwardRotation = Quaternion.AngleAxis(nextAngle, origin.up) * (origin.forward * distance);

            Debug.Log("nextAngle: " + nextAngle + " process:" + ((progress - 0.5f) * range));
            Debug.Log($"origin.position {forwardRotation} + forwardRotation: {origin.position + forwardRotation}");
            _index++;
            Debug.DrawLine(
                origin.position, 
                origin.position + forwardRotation, 
                new Color(0, 0.5f, 0, 1),
                1f);

            return true;
        }
        
        return false;
    }
}
