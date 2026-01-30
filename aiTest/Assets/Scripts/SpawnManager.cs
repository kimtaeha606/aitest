using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpawnManager: Instantiate + timing only.
/// - Does NOT do difficulty/wave/selection logic.
/// - Spawns only at positions provided via spawnPositions.
/// </summary>
public sealed class SpawnManager : MonoBehaviour
{
    public enum PositionPolicy
    {
        Cycle,
        Random,
        ExternalIndex // optional: external sets next index
    }

    [Header("Spawn Positions (must be provided from outside)")]
    [SerializeField] private List<Transform> spawnPoints = new();

    [Header("Position Selection Policy")]
    [SerializeField] private PositionPolicy policy = PositionPolicy.Cycle;

    // Optional: event callback
    public event Action<GameObject, Vector3> OnSpawned;

    private Coroutine _spawnLoop;
    private float _spawnInterval = 1f;
    private GameObject _currentPrefab;

    // For Cycle policy
    private int _cycleIndex = 0;

    // For ExternalIndex policy
    private int _externalNextIndex = 0;

    /// <summary>
    /// Start a loop that spawns the given prefab repeatedly with the given interval.
    /// </summary>
    public void StartSpawning(GameObject prefab, float spawnInterval)
    {
        _currentPrefab = prefab;
        SetSpawnInterval(spawnInterval);

        // Validate before starting loop
        if (!CanSpawn(_currentPrefab))
        {
            StopSpawning(); // ensure stopped
            return;
        }

        if (_spawnLoop != null)
            StopCoroutine(_spawnLoop);

        _spawnLoop = StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Stops the current spawn loop immediately.
    /// </summary>
    public void StopSpawning()
    {
        if (_spawnLoop != null)
        {
            StopCoroutine(_spawnLoop);
            _spawnLoop = null;
        }
    }

    /// <summary>
    /// Updates the spawn interval. Takes effect from the next spawn tick.
    /// </summary>
    public void SetSpawnInterval(float spawnInterval)
    {
        // Clamp to avoid negative/zero causing tight loop; still respect "external sets" intent.
        // If you want "0 means spawn every frame" behavior, remove the clamp.
        _spawnInterval = Mathf.Max(0.01f, spawnInterval);
    }

    /// <summary>
    /// Spawns the given prefab immediately once (position chosen from spawnPositions).
    /// </summary>
    public GameObject SpawnOnce(GameObject prefab)
    {
        if (!CanSpawn(prefab))
            return null;

        Vector3 pos = GetNextSpawnPosition();
        var instance = Instantiate(prefab, pos, Quaternion.identity);
        OnSpawned?.Invoke(instance, pos);
        return instance;
    }

    /// <summary>
    /// Returns the next spawn position chosen strictly from spawnPositions.
    /// Policy is fixed internally (Cycle/Random/ExternalIndex).
    /// </summary>
    public Vector3 GetNextSpawnPosition()
    {
        // NOTE: Caller should have ensured CanSpawn() already, but keep it safe.
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[SpawnManager] Cannot pick spawn position: spawnPositions is empty.");
            return Vector3.zero;
        }

        int idx = policy switch
        {
            PositionPolicy.Cycle => GetCycleIndex(),
            PositionPolicy.Random => UnityEngine.Random.Range(0, spawnPoints.Count),
            PositionPolicy.ExternalIndex => GetExternalIndexClamped(),
            _ => GetCycleIndex()
        };

        Transform t = spawnPoints[idx];
        if (t == null)
        {
            Debug.LogError($"[SpawnManager] spawnPositions[{idx}] is null Transform. Check list wiring.");
            return Vector3.zero;
        }

        return t.position;
    }

    /// <summary>
    /// Optional: only used when policy == ExternalIndex.
    /// Sets which index will be used on the next GetNextSpawnPosition().
    /// </summary>
    public void SetNextSpawnIndex(int index)
    {
        _externalNextIndex = index;
    }

    /// <summary>
    /// Optional: external provides/overwrites spawn positions list (Transform version).
    /// </summary>
    public void SetSpawnPoints(List<Transform> points)
    {
        spawnPoints = points ?? new List<Transform>();
        _cycleIndex = 0; // reset cycle for determinism
    }

    private IEnumerator SpawnLoop()
    {
        // Spawn immediately or after first interval?
        // If you want first spawn after interval, move WaitForSeconds to the top.
        while (true)
        {
            // If prefab/positions become invalid at runtime, stop safely.
            if (!CanSpawn(_currentPrefab))
            {
                StopSpawning();
                yield break;
            }

            SpawnOnce(_currentPrefab);

            // Interval may be changed at runtime; read _spawnInterval each loop.
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private bool CanSpawn(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[SpawnManager] Cannot spawn: prefab is null.");
            return false;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[SpawnManager] Cannot spawn: spawnPositions is empty.");
            return false;
        }

        return true;
    }

    private int GetCycleIndex()
    {
        int idx = _cycleIndex;

        // Advance for next time
        _cycleIndex++;
        if (_cycleIndex >= spawnPoints.Count)
            _cycleIndex = 0;

        return idx;
    }

    private int GetExternalIndexClamped()
    {
        if (spawnPoints.Count <= 0) return 0;
        return Mathf.Clamp(_externalNextIndex, 0, spawnPoints.Count - 1);
    }
}
