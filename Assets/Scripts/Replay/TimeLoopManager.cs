using System.Collections.Generic;
using UnityEngine;

public class TimeLoopManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject replayClonePrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Loop Settings")]
    [SerializeField] private int maxCopies = 3;

    private readonly List<AttemptData> savedAttempts = new List<AttemptData>();

    private GameObject currentPlayer;
    private PlayerAttemptRecorder currentRecorder;

    private void Start()
    {
        StartNewAttempt();
    }

    public void PlayerDied()
    {
        SaveCurrentAttempt();

        if (currentPlayer != null)
            Destroy(currentPlayer);

        StartNewAttempt();
    }

    private void StartNewAttempt()
    {
        ClearOldClones();
        SpawnReplayClones();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        currentPlayer = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        currentRecorder = currentPlayer.GetComponent<PlayerAttemptRecorder>();

        if (currentRecorder != null)
            currentRecorder.StartRecording();
    }

    private void SaveCurrentAttempt()
    {
        if (currentRecorder == null)
            return;

        AttemptData attempt = currentRecorder.StopRecording();

        if (attempt == null || attempt.frames == null || attempt.frames.Count < 2)
        {
            Debug.Log("Attempt ignored. Not enough frames.");
            return;
        }

        if (attempt.Duration < 0.2f)
        {
            Debug.Log("Attempt ignored. Too short.");
            return;
        }

        savedAttempts.Add(attempt);

        while (savedAttempts.Count > maxCopies)
        {
            savedAttempts.RemoveAt(0);
        }
    }

    private void SpawnReplayClones()
    {
        foreach (AttemptData attempt in savedAttempts)
        {
            GameObject cloneObject = Instantiate(
                replayClonePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            ReplayClone replayClone = cloneObject.GetComponent<ReplayClone>();

            if (replayClone != null)
                replayClone.BeginReplay(attempt);
        }
    }

    private void ClearOldClones()
    {
        ReplayClone[] clones = FindObjectsByType<ReplayClone>();

        foreach (ReplayClone clone in clones)
        {
            Destroy(clone.gameObject);
        }
    }
}