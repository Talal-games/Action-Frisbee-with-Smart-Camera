using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FrisbeeFmodAudioManager : MonoBehaviour
{
    public static FrisbeeFmodAudioManager Instance { get; private set; }

    private readonly List<EventInstance> eventInstances = new List<EventInstance>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Found more than one FrisbeeFmodAudioManager in the scene.", this);
            return;
        }

        Instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        if (sound.IsNull) return;

        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    public EventInstance CreateEventInstance(EventReference eventReference, Transform attachTarget, Rigidbody attachRigidbody)
    {
        if (eventReference.IsNull) return default;

        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        if (!eventInstance.isValid()) return default;

        if (attachTarget != null)
        {
            RuntimeManager.AttachInstanceToGameObject(eventInstance, attachTarget, attachRigidbody);
        }

        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public void SetEventInstanceParameter(EventInstance eventInstance, string parameterName, float parameterValue)
    {
        if (!eventInstance.isValid()) return;

        eventInstance.setParameterByName(parameterName, parameterValue);
    }

    public void StopAndRelease(EventInstance eventInstance)
    {
        if (!eventInstance.isValid()) return;

        eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        eventInstance.release();
        eventInstances.Remove(eventInstance);
    }

    public bool IsPlaying(EventInstance eventInstance)
    {
        if (!eventInstance.isValid()) return false;

        eventInstance.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
    }

    private void OnDestroy()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            if (!eventInstance.isValid()) continue;

            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        eventInstances.Clear();
    }
}
