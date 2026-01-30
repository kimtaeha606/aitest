using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class MonsterMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MonsterStats monsterStats;
    

    private Rigidbody rb;
    private MonsterChasePlayer chaser;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        chaser = GetComponent<MonsterChasePlayer>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (chaser == null) return;

        Vector3 dir = chaser.ChaseDirection;
        if (dir == Vector3.zero) return;

        // 목표 속도
        Vector3 desiredVelocity = dir * monsterStats.CurrentSpeed;
        Vector3 velocityChange = desiredVelocity - rb.linearVelocity;

        velocityChange.y = 0f; // 지상 몬스터 기준

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        
    }
}
