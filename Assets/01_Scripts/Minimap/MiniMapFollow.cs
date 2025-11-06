using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public float height = 50f;

    void LateUpdate()
    {
        if (!player) return;

        Vector3 newPos = player.position;
        newPos.y = height;
        transform.position = newPos;

        // Mantén la rotación superior fija
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
