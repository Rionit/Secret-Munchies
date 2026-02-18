using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    public static AudioManager Instance { get; private set; }

    public string currentRoomTone;
    
    private Queue<Sound> soundQueue = new Queue<Sound>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (Sound sound in sounds)
        {
            sound.source = sound.source == null 
                ? gameObject.AddComponent<AudioSource>() 
                : sound.source;
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
            sound.source.spatialBlend = sound.is3D ? 1f : 0f;
            if (sound.playOnAwake) Play(sound.name);
        }
    }

    public void PlayOneShot(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;

        s.source.PlayOneShot(s.clip);
    }

    public void Play(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;

        s.source.Play();
    }
    
    public void Stop(string name)
    {
        Sound s = GetSound(name);
        if (s == null) return;

        s.source.Stop();
    }
    
    public void FadeIn(string name, float duration = 1f)
    {
        Sound s = GetSound(name);
        if (s == null) return;
        if (s.fade != null)
            StopCoroutine(s.fade);

        s.fade = StartCoroutine(FadeInCoroutine(s, duration));
    }

    public void FadeOut(string name, float duration = 1f)
    {
        Sound s = GetSound(name);
        if (s == null) return;
        if (s.fade != null)
            StopCoroutine(s.fade);
        
        s.fade = StartCoroutine(FadeOutCoroutine(s, duration));
    }
    
    public void Transition(string from, string to, float duration = 1f)
    {
        Sound fromSound = GetSound(from);
        Sound toSound = GetSound(to);

        if (toSound == null) return;
        if (fromSound == null)
            StartCoroutine(FadeInCoroutine(toSound, duration));
        else
            StartCoroutine(TransitionCoroutine(fromSound, toSound, duration));
    }

    private IEnumerator FadeInCoroutine(Sound sound, float duration)
    {
        //Debug.LogError("Fading in to " + sound.name);
        
        float targetVolume = sound.volume;
        float startVolume = sound.source.volume;
        
        if (!sound.source.isPlaying)
            sound.source.Play();
        
        float time = 0f;
        while (time < duration)
        {
            time += Time.fixedDeltaTime;
            sound.source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        sound.source.volume = targetVolume;
    }

    private IEnumerator FadeOutCoroutine(Sound sound, float duration)
    {
        //Debug.LogError("Fading out from " + sound.name);
        
        float targetVolume = 0f;
        float startVolume = sound.source.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.fixedDeltaTime;
            sound.source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        sound.source.volume = 0f;
        sound.source.Stop();
    }

    private IEnumerator TransitionCoroutine(Sound from, Sound to, float duration)
    {
        //Debug.LogError("Transition from " + from.name + " to " + to.name);
        
        float halfDuration = duration / 2f;

        yield return FadeOutCoroutine(from, halfDuration);
        yield return FadeInCoroutine(to, halfDuration);
    }

    private Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
        }
        return s;
    }
}
