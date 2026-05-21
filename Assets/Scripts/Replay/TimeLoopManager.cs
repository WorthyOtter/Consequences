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

    private readonly List<InputAttemptData> savedAttempts = new List<InputAttemptData>();

    private GameObject currentPlayer;
    private InputAttemptRecorder currentRecorder;

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

        currentRecorder = currentPlayer.GetComponent<InputAttemptRecorder>();

        if (currentRecorder != null)
            currentRecorder.StartRecording();
        // Update Cinemachine follow target
        DynamicCamera dynamicCamera = FindAnyObjectByType<DynamicCamera>();

        if (dynamicCamera != null)
        {
            dynamicCamera.camera.Follow = currentPlayer.transform;
        }
    }

    private void SaveCurrentAttempt()
    {
        if (currentRecorder == null)
            return;

        InputAttemptData attempt = currentRecorder.StopRecording();

        if (attempt == null || attempt.frames == null || attempt.frames.Count < 2)
        {
            Debug.Log("Attempt ignored. Not enough frames.");
            return;
        }

        if (attempt.DurationTicks < 0.2f)
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
        foreach (InputAttemptData attempt in savedAttempts)
        {
            GameObject cloneObject = Instantiate(
                replayClonePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            ReplayInputDriver replayClone = cloneObject.GetComponent<ReplayInputDriver>();

            if (replayClone != null)
                replayClone.BeginReplay(attempt);
        }
    }

    private void ClearOldClones()
    {
        ReplayInputDriver[] clones = FindObjectsByType<ReplayInputDriver>();

        foreach (ReplayInputDriver clone in clones)
        {
            Destroy(clone.gameObject);
        }
    }
}