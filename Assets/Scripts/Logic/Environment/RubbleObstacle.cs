using System.Collections;
using UnityEngine;

public class RubbleObstacle : MonoBehaviour
{
    [SerializeField] private GameObject object1;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private LayerMask playerLayer;

    public void ExplodeRubble()
    {
        StartCoroutine(ExplodeRubbleRoutine());
    }

    private IEnumerator ExplodeRubbleRoutine()
    {
        object1.SetActive(true);

        yield return new WaitForSeconds(3f);

        RaycastHit2D hit = Physics2D.CircleCast(
            object1.transform.position,
            3f,
            Vector2.zero,
            0f,
            playerLayer
        );

        if (hit.collider != null)
        {
            PlayerDeath playerDeath = hit.collider.GetComponent<PlayerDeath>();

            if (playerDeath != null)
            {
                playerDeath.Die();
            }
        }

        if (particles != null)
        {
            particles.Play();
        }

        foreach (Transform child in transform)
        {
            if (child.name == "Particle System") continue;
            child.gameObject.SetActive(false);
        }
    }
}