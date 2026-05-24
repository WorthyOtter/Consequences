using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
    public UnityEvent eventToTrigger;

    public bool deleteTrigger = false;
    [SerializeField] private LayerMask selectedLayers;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(((1 << collision.gameObject.layer) & selectedLayers) != 0))
        {
            return;
        }

        eventToTrigger.Invoke();

        if (deleteTrigger) Destroy(this);
    }
}
