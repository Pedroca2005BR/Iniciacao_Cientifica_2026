using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    /*
     * How To Use AudioManager:
- AudioManager is a singleton with DontDestroyOnLoad, which means that only one, in the first scene of the game, is needed. 
- We can confgure sounds in its SoundArray on the Editor.
- When we want to interact with a sound, we use the methods in its instance, passing the name string of the sound.
- It can play globally, play at a specific point, and stop sounds.
     * 
     */
    [SerializeField] private Sound[] sounds;

    public static AudioManager instance;

    private void Awake ()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject, 2f);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound sound in sounds)
        {
            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
        }
    }



    // Added to make sure the volume changes will persist for every scene
    //private void Start()
    //{
    //    AudioListener.volume = PlayerPrefs.GetFloat(AudioSettings.GLOBAL_VOLUME_KEY, 1);

    //    for (int i = 0; i < AudioSettings.SOUND_TYPE_VOLUME_KEY.Length; i++)
    //    {
    //        float value = PlayerPrefs.GetFloat(AudioSettings.SOUND_TYPE_VOLUME_KEY[i], 1);

    //        SetSoundVolume((SoundType)i, value);
    //    }
    //}




    public void PlaySound (string name)
    {
        Sound s = FindSound(name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.audioSource.Play();
    }

    public void StopSound (string name)
    {
        Sound s = FindSound(name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.audioSource.Stop();
    }

    public void PlaySoundAt (GameObject source, string name)
    {
        Sound s = FindSound(name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        AudioSource soundSource = source.AddComponent<AudioSource>();
        soundSource.clip = s.clip;
        soundSource.volume = s.volume;
        soundSource.pitch = s.pitch;
        soundSource.spatialBlend = 0.5f;
        soundSource.Play();
        Destroy(soundSource, (soundSource.clip.length + 1f));
    }

    private Sound FindSound (string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }



    // Sets the audioSource volume of each sound to a value
    public void SetSoundVolume(SoundType type, float value)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.type == type)
            {
                sound.audioSource.volume = value;
            }
        }
    }
}
