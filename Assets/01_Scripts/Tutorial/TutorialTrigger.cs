using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    public string message;
    public float displayTime = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ui = FindObjectOfType<TutorialUIManager>();
            if (ui != null)
                ui.ShowMessage(message, displayTime);

            // Destruye el trigger para que no se repita
            Destroy(gameObject);
        }
    }
}
