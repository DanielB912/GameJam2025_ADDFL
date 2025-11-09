using UnityEngine;
using System.Linq;

public class InteractProximity : MonoBehaviour
{
    public float radius = 2.5f;
    public LayerMask interactMask = ~0; // Everything

    // ← NUEVO: accesible por otros scripts (solo lectura pública)
    public IInteractable Current { get; private set; }

    void Update()
    {
        Current = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, interactMask, QueryTriggerInteraction.Collide);
        if (hits.Length > 0)
        {
            Current = hits
                .Select(h => h.GetComponent<IInteractable>() ?? h.GetComponentInParent<IInteractable>())
                .Where(i => i != null)
                .OrderBy(i => Vector3.SqrMagnitude(((Component)i).transform.position - transform.position))
                .FirstOrDefault();
        }

        if (Current != null && Input.GetKeyDown(KeyCode.E))
            Current.Interact();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
