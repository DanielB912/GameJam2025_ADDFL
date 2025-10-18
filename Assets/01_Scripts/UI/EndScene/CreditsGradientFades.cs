using UnityEngine;
using UnityEngine.UI;

public class CreditsGradientFades : MonoBehaviour
{
    [Header("Refs")]
    public Image topFade;
    public Image bottomFade;

    [Header("Size")]
    public int texWidth = 8;
    public int texHeight = 256;

    [Header("Colors")]
    public Color fadeColor = Color.black; // color base (negro semitransparente)
    [Range(0, 1)] public float maxAlpha = 1f;    // opacidad máxima del fade

    void Start()
    {
        if (topFade) topFade.sprite = GenerateVerticalFadeSprite(topToOpaque: true);
        if (bottomFade) bottomFade.sprite = GenerateVerticalFadeSprite(topToOpaque: false);

        if (topFade) topFade.raycastTarget = false;
        if (bottomFade) bottomFade.raycastTarget = false;

        // Ajuste para que no se vea “lavado” si el Image tiene Color ≠ blanco
        if (topFade) topFade.color = Color.white;
        if (bottomFade) bottomFade.color = Color.white;
    }

    Sprite GenerateVerticalFadeSprite(bool topToOpaque)
    {
        var tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < texHeight; y++)
        {
            float t = (float)y / (texHeight - 1);
            float a = topToOpaque ? t * maxAlpha : (1f - t) * maxAlpha;
            var c = new Color(fadeColor.r, fadeColor.g, fadeColor.b, a);
            for (int x = 0; x < texWidth; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f), 100f);
    }
}
