using UnityEngine;

public class DEMO_MoveWall : MonoBehaviour
{
    private bool activateMovement = false;


    public void MoveThisWall()
    {
        activateMovement = true;
    }

    void Update()
    {
        if (activateMovement)
        {
            transform.position = (Vector2)transform.position + Vector2.up * Time.deltaTime;
        }
    }
}
