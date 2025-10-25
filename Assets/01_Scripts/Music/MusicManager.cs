using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;   // exact scene name in Build Settings
        public AudioClip clip;     // music for that scene
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Tracks per Scene")]
    public List<SceneTrack> tracks = new List<SceneTrack>();

    [Header("Playback")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    public float crossfadeSeconds = 2f;
    public bool playOnSceneLoad = true;

    // internal
    private static MusicManager instance;
    private AudioSource a, b;         // two sources for crossfade
    private AudioSource current;      // which is active

    void Awake()
    {
        if (instance && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // prepare audio sources
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        a.loop = b.loop = true;
        a.playOnAwake = b.playOnAwake = false;
        a.spatialBlend = b.spatialBlend = 0f;

        current = a;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!playOnSceneLoad) return;
        PlayForScene(scene.name);
    }

    // Public API: call manually if you want
    // dentro de MusicManager.cs
    public void PlayForScene(string sceneName)
    {
        var track = tracks.Find(t => t.sceneName == sceneName);
        if (track == null || track.clip == null) return;

        // ⛔ no reiniciar si ya está este clip
        if (current != null && current.clip == track.clip && current.isPlaying)
        {
            current.volume = track.volume * masterVolume; // ajusta volumen si cambió
            return;
        }

        CrossfadeTo(track.clip, track.volume * masterVolume);
    }


    public void Stop(float fadeSeconds = 0.5f)
    {
        if (current && current.isPlaying) StartCoroutine(FadeOut(current, fadeSeconds));
    }

    void CrossfadeTo(AudioClip clip, float targetVol)
    {
        if (clip == null) return;

        AudioSource next = (current == a) ? b : a;
        next.clip = clip;
        next.volume = 0f;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(FadeSwap(current, next, targetVol, Mathf.Max(0f, crossfadeSeconds)));

        current = next;
    }

    System.Collections.IEnumerator FadeSwap(AudioSource from, AudioSource to, float toVol, float sec)
    {
        float t = 0f;
        float fromStart = from ? from.volume : 0f;
        while (t < sec)
        {
            t += Time.deltaTime;
            float k = (sec > 0f) ? t / sec : 1f;
            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (to) to.volume = Mathf.Lerp(0f, toVol, k);
            yield return null;
        }
        if (from) { from.volume = 0f; from.Stop(); }
        if (to) { to.volume = toVol; }
    }

    System.Collections.IEnumerator FadeOut(AudioSource src, float sec)
    {
        if (!src) yield break;
        float t = 0f; float v0 = src.volume;
        while (t < sec)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(v0, 0f, t / sec);
            yield return null;
        }
        src.Stop(); src.volume = 0f;
    }


}
