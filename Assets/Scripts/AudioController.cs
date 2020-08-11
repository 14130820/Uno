using FeatherWorks.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource = null;

    private FeatherPool pool;

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            pool.Despawn(GetComponent<FeatherPoolInstance>());
        }
    }

    public void Initialize(AudioClip clip, FeatherPool pool)
    {
        this.pool = pool;
        audioSource.clip = clip;
        audioSource.Play();
    }
}
