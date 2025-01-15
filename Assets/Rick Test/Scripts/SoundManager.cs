using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioSource soundObject;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundClip(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audiosource = Instantiate(soundObject, spawnTransform.position, Quaternion.identity);
        audiosource.clip = clip;
        audiosource.volume = volume;
        audiosource.Play();
        float clipLength = audiosource.clip.length;
        Destroy(audiosource.gameObject, clipLength);
    }

    public void PlayPitchedSoundClip(AudioClip clip, Transform spawnTransform, float volume, float minPitch, float maxPitch)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        AudioSource audiosource = Instantiate(soundObject, spawnTransform.position, Quaternion.identity);
        audiosource.clip = clip;
        audiosource.volume = volume;
        audiosource.pitch = pitch;
        audiosource.Play();
        float clipLength = audiosource.clip.length;
        Destroy(audiosource.gameObject, clipLength);
    }
}
