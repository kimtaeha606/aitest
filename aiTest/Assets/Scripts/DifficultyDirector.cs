using UnityEngine;

public class DifficultyDirector : MonoBehaviour
{
    [Header("Runtime Difficulty")]
    [SerializeField] public float Difficult;

    [Header("Difficulty Measure Tuning")]
    [SerializeField] private float timeWeightMinutes = 1.0f;
    [SerializeField] private float waveWeight = 0.75f;
    [SerializeField] private float globalExponent = 0.9f;
    [SerializeField] private float softCapStrength = 0.15f;

    // ------------------------------------------------------------------
    // 1️⃣ WaveManager 런타임에서 난이도 측정
    // ------------------------------------------------------------------
    public float MesureDifficulty(float elapsedTimeSeconds, int waveIndex)
    {
        float timeMinutes = Mathf.Max(0f, elapsedTimeSeconds) / 60f;
        float waves = Mathf.Max(0, waveIndex);

        float p = (timeMinutes * timeWeightMinutes) + (waves * waveWeight);
        p = Mathf.Max(0f, p);

        if (!Mathf.Approximately(globalExponent, 1f))
            p = Mathf.Pow(p, globalExponent);

        if (softCapStrength > 0f)
            p = p / (1f + softCapStrength * p);

        Difficult = p;
        return p;
    }

    // ------------------------------------------------------------------
    // 2️⃣ 난이도 받아서 MonsterDatabaseSO 기준으로 스탯 계산
    //    - 에셋 수정 ❌
    //    - 계산 결과를 반환만 함
    // ------------------------------------------------------------------
    public ScaledMonsterData[] ComputeScaledStatsFromDatabase(
        float difficult,
        MonsterDatabaseSO db)
    {
        if (db == null || db.monsters == null)
            return null;

        var result = new ScaledMonsterData[db.monsters.Length];

        for (int i = 0; i < db.monsters.Length; i++)
        {
            MonsterSO so = db.monsters[i];
            result[i] = ScaleOne(so, difficult);
        }

        return result;
    }

    // ------------------------------------------------------------------
    // 3️⃣ 난이도 받아서 스폰 간격 계산
    //    - 몬스터별 SpawnInterval + MulspawnInterval 사용
    // ------------------------------------------------------------------
    public float ComputeSpawnInterval(MonsterSO so, float difficult)
    {
        float p = Mathf.Max(0f, difficult);

        float intervalMul = 1f + so.MulspawnInterval * p;
        float interval = so.SpawnInterval / intervalMul;

        return Mathf.Max(0.1f, interval); // 폭주 방지용 하한
    }

    // ------------------------------------------------------------------
    // 내부 헬퍼: 단일 몬스터 스탯 계산
    // ------------------------------------------------------------------
    private ScaledMonsterData ScaleOne(MonsterSO so, float difficult)
    {
        float p = Mathf.Max(0f, difficult);

        int hp = Mathf.RoundToInt(so.HP * (1f + so.Mulhp * p));
        int dmg = Mathf.RoundToInt(so.Damage * (1f + so.Muldamage * p));
        float speed = so.Speed * (1f + so.Mulspeed * p);

        return new ScaledMonsterData(
            so.monsterType,
            hp,
            dmg,
            speed
        );
    }

    [System.Serializable]
    public struct ScaledMonsterData
    {
        public MonsterType type;
        public int hp;
        public int damage;
        public float speed;

        public ScaledMonsterData(MonsterType type, int hp, int damage, float speed)
        {
            this.type = type;
            this.hp = hp;
            this.damage = damage;
            this.speed = speed;
        }
    }

}
