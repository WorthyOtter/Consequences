using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndSequence : MonoBehaviour
{
    public Image FadeToBlack;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        triggered = true;
        StartCoroutine(FadeOutAndLoadScene());
    }

    IEnumerator FadeOutAndLoadScene()
    {
        float duration = 3f;
        float elapsed = 0f;

        Color color = FadeToBlack.color;
        color.a = 0f;
        FadeToBlack.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Clamp01(elapsed / duration);

            color.a = alpha;
            FadeToBlack.color = color;

            yield return null;
        }

        SceneManager.LoadScene(0);
    }
}