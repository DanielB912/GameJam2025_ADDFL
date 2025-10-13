using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EnergyNodeInteractable : MonoBehaviour, IInteractable
{
    public string prompt = "Activar nodo de energía";
    public Light linkedLight;
    public Color offColor = Color.gray;
    public Color onColor = new Color(0.2f, 0.8f, 1f);

    private bool isOn = false;
    private Renderer rend;
    private MaterialPropertyBlock mpb;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        UpdateVisual();
    }

    public string GetPrompt()
    {
        return isOn ? "Desactivar nodo" : prompt;
    }

    public void Interact()
    {
        isOn = !isOn;
        if (linkedLight != null) linkedLight.enabled = isOn;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (rend == null) return;
        rend.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", isOn ? onColor : offColor);
        rend.SetPropertyBlock(mpb);
    }
}
