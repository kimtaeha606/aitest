using UnityEngine;

/// <summary>
/// MonsterStats
/// - 몬스터 인스턴스가 사용할 "현재 스탯(HP / Damage / Speed)"을 보관하는 컴포넌트
/// - 계산/난이도 처리 없음 (저장만)
/// - 스폰 시 외부(WaveManager/Spawner)에서 Init으로 주입
/// - 피격/사망 로직은 Health(예: MonsterHealth)가 담당
/// </summary>
public sealed class MonsterStats : MonoBehaviour
{
    [Header("Runtime Current Stats (read-only outside)")]
    [SerializeField] private int currentHp;
    [SerializeField] private int currentDamage;
    [SerializeField] private float currentSpeed;

    /// <summary>현재 HP (Health가 감소/증가시킬 값)</summary>
    public int CurrentHp => currentHp;

    /// <summary>현재 Damage (Attack이 참조할 값)</summary>
    public int CurrentDamage => currentDamage;

    /// <summary>현재 Speed (Movement가 참조할 값)</summary>
    public float CurrentSpeed => currentSpeed;

    /// <summary>
    /// 외부(Spawner/WaveManager)에서 스폰 직후 호출하여 현재 스탯을 주입한다.
    /// MonsterStats는 "값 저장"만 하며, 난이도 계산/스케일링은 호출자 측에서 끝내고 들어와야 한다.
    /// </summary>
    public void Init(int hp, int dmg, float speed)
    {
        currentHp = hp;
        currentDamage = dmg;
        currentSpeed = speed;
    }

    /// <summary>
    /// (선택) Health가 현재 HP를 갱신할 수 있도록 제공.
    /// 피격/회복 계산 자체는 Health 책임.
    /// </summary>
    public void SetCurrentHp(int hp)
    {
        currentHp = hp;
    }
}
