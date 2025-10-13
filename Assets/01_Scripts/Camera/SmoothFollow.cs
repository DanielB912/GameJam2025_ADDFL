using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -6f); // en MUNDO
    public float followSpeed = 8f;
    public float lookSpeed = 12f;

    void LateUpdate()
    {
        if (!target) return;

        // Offset fijo en mundo (NO usar TransformDirection).
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // Mira al target suavemente.
        Quaternion lookRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, lookSpeed * Time.deltaTime);
    }
}
