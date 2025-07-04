using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IdleAmbientSound : MonoBehaviour
{
    public AudioClip idleAmbientClip; // The looping idle clip
    public bool isIdle = true;        // Set externally

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = idleAmbientClip;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D sound (non-positional)
    }

    void Update()
    {
        if (isIdle)
        {
            if (!audioSource.isPlaying && idleAmbientClip != null)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
