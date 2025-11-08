using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTouchProxy : MonoBehaviour
{
    public BossMini boss;

    void OnTriggerStay(Collider other)
    {
        if (boss != null) boss.OnTouchTrigger(other);
    }
}
