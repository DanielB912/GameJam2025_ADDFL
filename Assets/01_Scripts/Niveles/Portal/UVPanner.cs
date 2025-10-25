using UnityEngine;

public class UVPanner : MonoBehaviour
{
    public float scrollSpeed = 0.3f;
    Renderer rend;
    Vector2 offset = Vector2.zero;

    void Start() => rend = GetComponent<Renderer>();

    void Update()
    {
        offset.y += Time.deltaTime * scrollSpeed;
        rend.material.mainTextureOffset = offset;
    }
}
