using UnityEngine;

[CreateAssetMenu(
    fileName = "MonsterSO",
    menuName = "Game/Monster/MonsterSO"
)]
public class MonsterSO : ScriptableObject
{
    [Header("Identity")]
    public MonsterType monsterType;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Base Stats (Editor Only Data)")]
    public int HP;
    public int Damage;
    public float Speed;
}
