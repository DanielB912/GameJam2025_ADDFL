using UnityEngine;
using System;

public interface IPuzzle
{
    Action OnSolved { get; set; }
    Action OnClosed { get; set; }

    void SetTargetNode(EnergyNodeInteractable node);
    void Open(); // o Show(), según tu convención
}
