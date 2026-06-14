using System.Collections;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField] private int maxHp = 40;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private int attackDamage = 8;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float targetRefreshInterval = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string movingParameter = "Moving";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathTrigger = "Death";
    [SerializeField] private float attackHitDelay = 0.35f;
    [SerializeField] private float destroyAfterDeathDelay = 2.2f;

    [Header("DŸwiêki")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioSource audioSource;

    private int currentHp;
    private float nextAttackTime;
    private float nextTargetRefreshTime;
    private bool isDead;
    private bool isAttacking;
    private BuildingInstance target;

    public bool IsAlive => currentHp > 0 && !isDead;

    private void Awake()
    {
        currentHp = Mathf.Max(1, maxHp);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsAlive)
            return;

        RefreshTargetIfNeeded();

        if (target == null)
        {
            SetMoving(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            MoveToTarget();
        }
        else
        {
            SetMoving(false);
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
        if (target == null || isAttacking)
            return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            SetMoving(false);
            return;
        }

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);

        SetMoving(true);
    }

    private void TryAttack()
    {
        if (isAttacking || Time.time < nextAttackTime || target == null || !target.IsAlive)
            return;

        nextAttackTime = Time.time + Mathf.Max(0.1f, attackCooldown);

        if (animator != null)
            animator.SetTrigger(attackTrigger);

        StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(BuildingInstance attackedTarget)
    {
        isAttacking = true;
        SetMoving(false);

        yield return new WaitForSeconds(attackHitDelay);

        if (IsAlive && attackedTarget != null && attackedTarget.IsAlive)
        {
            float distance = Vector3.Distance(transform.position, attackedTarget.transform.position);

            if (distance <= attackRange + 0.35f)
            {
                // dŸwiêk uderzenia
                if (attackSound != null && audioSource != null)
                    audioSource.PlayOneShot(attackSound);

                attackedTarget.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(0.15f);

        isAttacking = false;
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
        if (isDead)
            return;

        isDead = true;
        currentHp = 0;

        StopAllCoroutines();
        SetMoving(false);

        if (animator != null)
            animator.SetTrigger(deathTrigger);

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Destroy(gameObject, destroyAfterDeathDelay);
    }

    private void SetMoving(bool moving)
    {
        if (animator != null)
            animator.SetBool(movingParameter, moving);
    }
}