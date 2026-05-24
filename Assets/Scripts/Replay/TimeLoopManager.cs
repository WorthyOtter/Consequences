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


    [SerializeField] private Transform resetTemplateContainer;

    private readonly List<LoopSpawnEntry> loopSpawns = new List<LoopSpawnEntry>();
    private readonly List<GameObject> spawnedLoopObjects = new List<GameObject>();
    private void Start()
    {
        SaveResettables();
        StartNewAttempt();
    }

    private void SaveResettables()
    {
        loopSpawns.Clear();

        AddToResetList[] resetMarkers =
            FindObjectsByType<AddToResetList>();

        if (resetTemplateContainer == null)
        {
            GameObject container = new GameObject("Loop Reset Templates");
            resetTemplateContainer = container.transform;
            resetTemplateContainer.gameObject.SetActive(false);
        }

        foreach (AddToResetList marker in resetMarkers)
        {
            GameObject original = marker.gameObject;

            GameObject template = Instantiate(
                original,
                original.transform.position,
                original.transform.rotation,
                resetTemplateContainer
            );

            template.name = original.name + " Template";
            template.SetActive(false);

            loopSpawns.Add(new LoopSpawnEntry
            {
                template = template,
                position = original.transform.position,
                rotation = original.transform.rotation,
                parent = original.transform.parent
            });

            Destroy(original);
        }
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
        ResetLoopObjects();

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

    private void ResetLoopObjects()
    {
        for (int i = spawnedLoopObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedLoopObjects[i] != null)
                Destroy(spawnedLoopObjects[i]);
        }

        spawnedLoopObjects.Clear();

        foreach (LoopSpawnEntry entry in loopSpawns)
        {
            if (entry.template == null)
                continue;

            GameObject spawned = Instantiate(
                entry.template,
                entry.position,
                entry.rotation,
                entry.parent
            );

            spawned.name = entry.template.name.Replace(" Template", "");
            spawned.SetActive(true);

            spawnedLoopObjects.Add(spawned);
        }
    }
}