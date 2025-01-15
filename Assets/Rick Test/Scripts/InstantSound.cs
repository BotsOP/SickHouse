using UnityEngine;

public class InstantSound : MonoBehaviour
{
    public bool dontPlayOnStart;
    float time;
    [SerializeField] private AudioClip engines;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!dontPlayOnStart)
        {
            SoundManager.instance.PlaySoundClip(engines, transform, 1f);
        }
    }
    public void TriggerSound()
    {
        SoundManager.instance.PlaySoundClip(engines, transform, 1f);
    }
}
