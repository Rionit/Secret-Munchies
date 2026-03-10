using UnityEngine;

public class AudioSourceLinker : MonoBehaviour
{
    [SerializeField] private string soundName;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found in scene.");
            return;
        }

        AudioSource source = GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogWarning($"No AudioSource found on {gameObject.name}");
            return;
        }

        LinkSource(source);
    }

    private void LinkSource(AudioSource source)
    {
        Sound[] array = AudioManager.Instance.GetSounds();

        Sound s = System.Array.Find(array, sound => sound.name == soundName);

        if (s == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found in AudioManager.");
            return;
        }

        // Destroy previously auto-created AudioSource on AudioManager (component only)
        if (s.source != null && s.source.gameObject == AudioManager.Instance.gameObject)
        {
            Destroy(s.source);
        }
        
        s.source = source;

        // Apply sound settings to this AudioSource
        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        source.playOnAwake = s.playOnAwake;
        source.spatialBlend = s.is3D ? 1f : 0f;
        source.outputAudioMixerGroup = AudioManager.Instance.mixer.FindMatchingGroups("Master")[0];
        
        if (s.playOnAwake) { source.Play(); }
    }
}