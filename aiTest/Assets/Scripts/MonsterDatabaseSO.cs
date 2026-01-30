using UnityEngine;

[CreateAssetMenu(
    fileName = "MonsterDatabaseSO",
    menuName = "Game/Monster/Monster Database"
)]
public class MonsterDatabaseSO : ScriptableObject
{
    [Header("All Monster Definitions")]
    public MonsterSO[] monsters;

    
}
