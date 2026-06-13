using UnityEngine;

public class TowerProjectile : MonoBehaviour
{
    private EnemyUnit target;
    private int damage;
    private float speed;
    private float hitDistance;

    public void Init(EnemyUnit newTarget, int newDamage, float newSpeed, float newHitDistance)
    {
        target = newTarget;
        damage = newDamage;
        speed = newSpeed;
        hitDistance = newHitDistance;
    }

    private void Update()
    {
        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * 0.6f;
        Vector3 direction = targetPosition - transform.position;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if ((targetPosition - transform.position).sqrMagnitude <= hitDistance * hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}