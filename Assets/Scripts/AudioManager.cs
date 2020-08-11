using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FeatherWorks.Pooling;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField]
    private AudioClip takeCardSound = null;
    [SerializeField]
    private AudioClip takeCardEndSound = null;
    [SerializeField]
    private AudioClip playCardEndSound = null;
    [SerializeField]
    private GameObject soundPrefab = null;

    private FeatherPool soundPrefabGroup;

    private void Awake()
    {
        Instance = this;
        soundPrefabGroup = FeatherPoolManager.Instance.GetPool(soundPrefab.GetInstanceID());
    }

    private void PlayCardSound(Vector3 position, AudioClip clip)
    {
        var soundObj = soundPrefabGroup.Spawn().GetComponent<AudioController>();

        soundObj.transform.position = position;
        soundObj.Initialize(clip, soundPrefabGroup);
    }

    public void PlayCardEndSound(Vector3 position)
    {
        PlayCardSound(position, playCardEndSound);
    }

    public void PlayTakeCardSound(Vector3 position)
    {
        PlayCardSound(position, takeCardSound);
    }

    public void PlayTakeCardEndSound(Vector3 position)
    {
        PlayCardSound(position, takeCardEndSound);
    }

    public void BeginShuffle()
    {

    }
}