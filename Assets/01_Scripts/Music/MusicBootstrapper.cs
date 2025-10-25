// MusicBootstrapper.cs
using UnityEngine;

public static class MusicBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureMusicManager()
    {
        if (Object.FindObjectOfType<MusicManager>() != null) return;

        var prefab = Resources.Load<MusicManager>("MusicManager"); // Assets/Resources/MusicManager.prefab
        if (prefab != null) Object.Instantiate(prefab);
        else Debug.LogWarning("[Music] No MusicManager prefab found in Resources/MusicManager.");
    }
}
