using UnityEngine;

public class VerticalDoor : MonoBehaviour
{
    private bool goingUp = false;
    private Vector3 startPosition;

    public float UpFactor = 1f;

    private float moveDuration = 3f;

    void Start()
    {
        startPosition = transform.position;
    }

    public void ChangeDirection(bool up)
    {
        goingUp = up;
    }

    void Update()
    {
        Vector3 targetPosition = goingUp
            ? startPosition + Vector3.up * UpFactor
            : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            UpFactor / moveDuration * Time.deltaTime
        );
    }
}