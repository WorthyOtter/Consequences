using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.25f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private Transform interactOrigin;

    private readonly Collider2D[] hits = new Collider2D[16];

    private void Awake()
    {
        if (interactOrigin == null)
            interactOrigin = transform;
    }

    public void TryInteract()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            interactOrigin.position,
            interactRadius,
            hits,
            interactableLayers
        );

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector2.Distance(
                interactOrigin.position,
                hit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != null)
        {
            closestInteractable.Interact(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = interactOrigin != null ? interactOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, interactRadius);
    }
}
