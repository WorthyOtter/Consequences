using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    public bool destroyAfterTrigger = false;
    void OnTriggerEnter2D(Collider2D collision)
    {

        PlayerDeath playerDeath = collision.GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.Die();
            if (destroyAfterTrigger) Destroy(gameObject);
        }

        CloneReplayMovement cloneMovement = collision.GetComponent<CloneReplayMovement>();
        if (cloneMovement != null)
        {
            cloneMovement.CanMove(false);
            if (destroyAfterTrigger) Destroy(gameObject);
        }

        EnemyAI enemy = collision.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.Die();
            if (destroyAfterTrigger) Destroy(gameObject);
        }
    }
}
