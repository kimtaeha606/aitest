using UnityEngine;

[DisallowMultipleComponent]
public sealed class MonsterChasePlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool flattenY = true;

    /// <summary>
    /// 현재 프레임 기준 추적 방향 (정규화됨)
    /// 타겟이 없으면 Vector3.zero
    /// </summary>
    public Vector3 ChaseDirection { get; private set; }

    private Transform target;

    private void Update()
    {
        if (!IsTargetValid(target))
        {
            TryFindTarget();
            ChaseDirection = Vector3.zero;
            return;
        }

        Vector3 dir = target.position - transform.position;
        if (flattenY) dir.y = 0f;

        ChaseDirection = dir.sqrMagnitude > 0.0001f
            ? dir.normalized
            : Vector3.zero;
    }

    private void TryFindTarget()
    {
        GameObject go;
        try
        {
            go = GameObject.FindGameObjectWithTag(playerTag);
        }
        catch
        {
            return;
        }

        if (go == null) return;
        target = go.transform;
    }

    private static bool IsTargetValid(Transform t)
    {
        return t != null && t.gameObject != null && t.gameObject.activeInHierarchy;
    }
}
