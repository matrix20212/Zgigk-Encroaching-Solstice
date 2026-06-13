using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private int maxHp = 40;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private int attackDamage = 8;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float targetRefreshInterval = 0.2f;

    private int currentHp;
    private float nextAttackTime;
    private float nextTargetRefreshTime;
    private BuildingInstance target;

    public bool IsAlive => currentHp > 0;

    private void Awake()
    {
        currentHp = Mathf.Max(1, maxHp);
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        RefreshTargetIfNeeded();

        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            MoveToTarget();
        }
        else
        {
            TryAttack();
        }
    }

    private void RefreshTargetIfNeeded()
    {
        if (Time.time < nextTargetRefreshTime && target != null && target.IsAlive)
            return;

        nextTargetRefreshTime = Time.time + Mathf.Max(0.05f, targetRefreshInterval);
        target = BuildingRegistry.GetNearest(transform.position);
    }

    private void MoveToTarget()
    {
        if (target == null)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || target == null || !target.IsAlive)
            return;

        nextAttackTime = Time.time + Mathf.Max(0.1f, attackCooldown);
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