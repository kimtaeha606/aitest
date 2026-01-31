using UnityEngine;
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(hp);
        if (hp <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
