using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaveManager: wave/time + difficulty query + monster pick + spawn driving.
/// - Owns: wave progress, which monster to spawn, when to change interval, and stat injection.
/// - Does NOT own: actual Instantiate timing/position logic (SpawnManager owns that).
/// </summary>
public sealed class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonsterDatabaseSO database;
    [SerializeField] private DifficultyDirector difficultyDirector;
    [SerializeField] private SpawnManager spawnManager;

    [Header("Wave Progress")]
    [SerializeField] private float waveDurationSeconds = 30f;
    [SerializeField] private bool autoStart = true;

    private float elapsed;
    private int waveIndex;

    // cached scaled stats (MonsterType -> scaled)
    private readonly Dictionary<MonsterType, DifficultyDirector.ScaledMonsterData> scaledByType = new();

    // current loop state
    private MonsterSO currentMonsterSO;
    private float currentInterval = -1f;

    private void OnEnable()
    {
        if (spawnManager != null)
            spawnManager.OnSpawned += HandleSpawned;

        if (autoStart)
            StartWave();
    }

    private void OnDisable()
    {
        if (spawnManager != null)
            spawnManager.OnSpawned -= HandleSpawned;

        StopWave();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (waveDurationSeconds > 0.1f)
        {
            int newWave = Mathf.FloorToInt(elapsed / waveDurationSeconds);
            if (newWave != waveIndex)
                waveIndex = newWave;
        }

        // 스폰 루프 중에도 spawnInterval은 계속 변할 수 있으므로 주기적으로 갱신
        TickSpawnTuning();
    }

    public void StartWave()
    {
        if (database == null || database.monsters == null || database.monsters.Length == 0)
        {
            Debug.LogWarning("[WaveManager] MonsterDatabaseSO is missing or empty.");
            return;
        }
        if (difficultyDirector == null)
        {
            Debug.LogWarning("[WaveManager] DifficultyDirector is null.");
            return;
        }
        if (spawnManager == null)
        {
            Debug.LogWarning("[WaveManager] SpawnManager is null.");
            return;
        }

        // 초기 몬스터 선택 (최소 구현: 랜덤)
        currentMonsterSO = PickMonsterSO(database);
        if (currentMonsterSO == null || currentMonsterSO.prefab == null)
        {
            Debug.LogWarning("[WaveManager] Picked MonsterSO or prefab is null.");
            return;
        }

        // 초기 난이도/스탯 캐시
        float difficult = difficultyDirector.MesureDifficulty(elapsed, waveIndex);
        RefreshScaledCache(difficult);

        // 초기 간격 계산 후 스폰 시작
        currentInterval = difficultyDirector.ComputeSpawnInterval(currentMonsterSO, difficult);
        spawnManager.StartSpawning(currentMonsterSO.prefab, currentInterval);
    }

    public void StopWave()
    {
        if (spawnManager != null)
            spawnManager.StopSpawning();
    }

    /// <summary>
    /// 스폰 루프를 끊지 않고 spawn interval만 갱신.
    /// (Monster 변경까지 하고 싶으면 ChangeMonsterAndRestartLoop()를 호출)
    /// </summary>
    private void TickSpawnTuning()
    {
        if (spawnManager == null || difficultyDirector == null) return;
        if (currentMonsterSO == null) return;

        float difficult = difficultyDirector.MesureDifficulty(elapsed, waveIndex);

        // 스탯 캐시는 난이도 기반이므로 갱신
        RefreshScaledCache(difficult);

        float newInterval = difficultyDirector.ComputeSpawnInterval(currentMonsterSO, difficult);

        // 너무 자주 SetSpawnInterval 하지 않게 소수점 비교
        if (Mathf.Abs(newInterval - currentInterval) > 0.001f)
        {
            currentInterval = newInterval;
            spawnManager.SetSpawnInterval(currentInterval);
        }
    }

    /// <summary>
    /// 몬스터 종류까지 바꾸고 싶을 때 사용.
    /// (SpawnManager는 "어떤 프리팹"을 스폰할지 몰라야 하므로, 여기서 loop 재시작)
    /// </summary>
    public void ChangeMonsterAndRestartLoop()
    {
        if (database == null || database.monsters == null || database.monsters.Length == 0) return;
        if (spawnManager == null) return;

        MonsterSO so = PickMonsterSO(database);
        if (so == null || so.prefab == null) return;

        currentMonsterSO = so;

        float difficult = difficultyDirector.MesureDifficulty(elapsed, waveIndex);
        RefreshScaledCache(difficult);

        currentInterval = difficultyDirector.ComputeSpawnInterval(currentMonsterSO, difficult);
        spawnManager.StartSpawning(currentMonsterSO.prefab, currentInterval);
    }

    private MonsterSO PickMonsterSO(MonsterDatabaseSO db)
    {
        int idx = Random.Range(0, db.monsters.Length);
        return db.monsters[idx];
    }

    private void RefreshScaledCache(float difficult)
    {
        // DifficultyDirector에서 스케일 계산 결과를 가져온다고 가정
        // (당신 DifficultyDirector.cs의 반환 타입/함수명이 다르면 여기만 맞추면 됨)
        DifficultyDirector.ScaledMonsterData[] arr = difficultyDirector.ComputeScaledStatsFromDatabase(difficult, database);
        if (arr == null) return;

        scaledByType.Clear();
        for (int i = 0; i < arr.Length; i++)
            scaledByType[arr[i].type] = arr[i];
    }

    private void HandleSpawned(GameObject instance, Vector3 pos)
    {
        if (instance == null) return;
        if (currentMonsterSO == null) return;

        // 프리팹에 MonsterStats가 붙어있고 Init(hp,dmg,speed)로 주입 가능하다고 가정
        MonsterStats stats = instance.GetComponent<MonsterStats>();
        if (stats == null)
        {
            Debug.LogWarning("[WaveManager] Spawned monster has no MonsterStats.");
            return;
        }

        // MonsterSO에 타입이 있다고 가정 (예: currentMonsterSO.monsterType)
        MonsterType type = currentMonsterSO.monsterType;

        if (!scaledByType.TryGetValue(type, out var scaled))
        {
            Debug.LogWarning("[WaveManager] No scaled stats for type: " + type);
            return;
        }

        stats.Init(scaled.hp, scaled.damage, scaled.speed);
    }
}
