using UnityEngine;

public class NewSpawn : MonoBehaviour
{
    public TimeLoopManager manager;
    public GameObject newSpawnPosition;

    void OnTriggerEnter2D(Collider2D collision)
    {
        manager.spawnPoint = newSpawnPosition.transform;
    }
}
