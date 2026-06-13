using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private int maxHp = 40;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private int attackDamage = 8;
    [SerializeField] private float attackCooldown = 1.2f;

    private int currentHp;
    private float nextAttackTime;
    private BuildingInstance target;

    public bool IsAlive => currentHp > 0;

    private void Awake()
    {
        currentHp = maxHp;
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        if (target == null || !target.IsAlive)
            target = BuildingRegistry.GetNearest(transform.position);

        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || target == null)
            return;

        nextAttackTime = Time.time + attackCooldown;
        target.TakeDamage(attackDamage);
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive)
            return;

        currentHp -= Mathf.Max(0, damage);

        if (currentHp <= 0)
            Die();
    }

    private void Die()
    {
        currentHp = 0;
        Destroy(gameObject);
    }
}
