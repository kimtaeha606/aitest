// WeaponController.cs
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;

    [Header("Hitscan")]
    [SerializeField] private float range = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask hitMask = ~0; // 기본: 전부

    public void OnAttack()
    {
        if (muzzle == null) return;

        Vector3 origin = muzzle.position;
        Vector3 dir    = muzzle.up;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // 1) 맞은 오브젝트에서 IDamageable 찾기
            // (몬스터의 Health가 IDamageable 구현했다고 가정)
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // 디버그용 (원하면 유지)
            Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
        }
        else
        {
            Debug.DrawRay(origin, dir * range, Color.yellow, 0.1f);
        }
    }

    // 입력 시스템 콜백을 쓰는 경우만 유지
    public void OnAttack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        OnAttack();
    }
}
