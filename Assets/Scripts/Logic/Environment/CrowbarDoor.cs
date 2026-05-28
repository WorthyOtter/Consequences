using Unity.VisualScripting;
using UnityEngine;

public class CrowbarDoor : MonoBehaviour
{
    public GameObject side1;
    public GameObject side2;
    public Sprite side1S;
    public Sprite side2S;
    public AudioClip sfx;
    public BoxCollider2D col;

    public void BreakDoor()
    {
        side1.GetComponent<SpriteRenderer>().sprite = side1S;
        side2.GetComponent<SpriteRenderer>().sprite = side2S;

        Destroy(col);
        AudioSource source = GetComponent<AudioSource>();
        if (sfx != null && source != null)
        {
            source.PlayOneShot(sfx);
        }
    }
}
