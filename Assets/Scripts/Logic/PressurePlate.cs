using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PressurePlate : MonoBehaviour
{
    [Header("Weight")]
    [SerializeField] private int requiredWeight = 1;

    [Header("Detection")]
    [SerializeField] private LayerMask actorLayers;
    [SerializeField] private float groundedTolerance = 0.2f;
    [SerializeField] private float upwardVelocityTolerance = 0.05f;
    public Collider2D plateTrigger;


    [Header("Sprites")]
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Events")]
    [SerializeField] private UnityEvent onEnter;
    [SerializeField] private UnityEvent onStay;
    [SerializeField] private UnityEvent onLeave;

    private readonly HashSet<Collider2D> collidersInside = new HashSet<Collider2D>();

    private bool isPressed;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOnActorLayer(other))
            return;

        if (!TryGetGroundedActor(other, out _))
            return;

        collidersInside.Add(other);
        EvaluatePlate();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!collidersInside.Contains(other))
            return;

        EvaluatePlate();

        if (isPressed)
            onStay?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!collidersInside.Remove(other))
            return;

        EvaluatePlate();
    }

    private void EvaluatePlate()
    {
        int validWeight = 0;

        List<Collider2D> toRemove = null;

        foreach (Collider2D col in collidersInside)
        {
            if (col == null)
            {
                toRemove ??= new List<Collider2D>();
                toRemove.Add(col);
                continue;
            }

            if (TryGetGroundedActor(col, out bool grounded) && grounded)
            {
                validWeight++;
            }
        }

        if (toRemove != null)
        {
            foreach (Collider2D col in toRemove)
                collidersInside.Remove(col);
        }

        bool shouldBePressed = validWeight >= requiredWeight;

        if (shouldBePressed == isPressed)
            return;

        isPressed = shouldBePressed;
        UpdateVisual();

        if (isPressed)
            onEnter?.Invoke();
        else
            onLeave?.Invoke();
    }

    private bool IsOnActorLayer(Collider2D other)
    {
        return (actorLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private bool IsPlayerOrClone(Collider2D other)
    {
        return other.GetComponentInParent<PlayerMovement>() != null ||
               other.GetComponentInParent<CloneReplayMovement>() != null;
    }

    private bool TryGetGroundedActor(Collider2D other, out bool isGrounded)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null)
        {
            isGrounded = player.IsPlayerGrounded();
            return true;
        }

        CloneReplayMovement clone = other.GetComponentInParent<CloneReplayMovement>();

        if (clone != null)
        {
            isGrounded = clone.IsCloneGrounded();
            return true;
        }

        isGrounded = false;
        return false;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = isPressed ? onSprite : offSprite;
    }
}