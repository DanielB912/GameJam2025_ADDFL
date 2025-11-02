using UnityEngine;
using System.Collections.Generic;

public class LightGroupController : MonoBehaviour
{
    [Header("Luces controladas por este grupo")]
    public List<Light> lights = new();

    private int activeSignals = 0;

    // Llamado por un nodo cuando se activa o desactiva
    public void SetSignal(bool on)
    {
        if (on) activeSignals++;
        else activeSignals = Mathf.Max(0, activeSignals - 1);

        bool finalState = activeSignals > 0;
        foreach (var l in lights)
        {
            if (l) l.enabled = finalState;
        }
    }
}
