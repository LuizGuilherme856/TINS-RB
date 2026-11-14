using System.Collections;
using UnityEngine;

/// <summary>Stops a demon at the tower and applies damage on its attack animation event.</summary>
public sealed class EnemyTowerAttack : MonoBehaviour
{
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private TowerController targetTower;
    [SerializeField, Min(0.1f)] private float attackRange = 1.4f;
    [SerializeField, Min(0f)] private float attackCooldown = 1f;
    [SerializeField, Min(0.1f)] private float attackAnimationDuration = 1f;
    [SerializeField] private string attackTrigger = "TowerAttack";

    private EnemyAI enemyAI;
    private Rigidbody2D body;
    private float nextAttackTime;
    private Coroutine attackRoutine;
    private bool impactApplied;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (attackAnimator == null) attackAnimator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        body = GetComponent<Rigidbody2D>();
        ResolveTower();
    }

    private void Update()
    {
        if (IsAttacking || Time.time < nextAttackTime || attackAnimator == null) return;
        if (targetTower == null) ResolveTower();
        if (targetTower == null || targetTower.currentLife <= 0) return;

        Collider2D towerCollider = targetTower.GetComponent<Collider2D>();
        Vector2 closestPoint = towerCollider != null
            ? towerCollider.ClosestPoint(transform.position)
            : (Vector2)targetTower.transform.position;
        if (Vector2.Distance(transform.position, closestPoint) > attackRange) return;

        BeginAttack();
    }

    private void BeginAttack()
    {
        IsAttacking = true;
        impactApplied = false;
        if (enemyAI != null) enemyAI.PauseForTowerAttack(true);
        if (body != null) body.linearVelocity = Vector2.zero;
        attackAnimator.ResetTrigger(attackTrigger);
        attackAnimator.SetTrigger(attackTrigger);
        attackRoutine = StartCoroutine(EndAttackAfterAnimation());
    }

    /// <summary>Animation Event on "attack animation demoninho" at the impact frame.</summary>
    public void AnimationImpact()
    {
        if (!IsAttacking || impactApplied) return;
        impactApplied = true;
        if (targetTower != null) targetTower.TryTakeDamageFromEnemy();
    }

    private IEnumerator EndAttackAfterAnimation()
    {
        yield return new WaitForSeconds(attackAnimationDuration);
        IsAttacking = false;
        if (enemyAI != null) enemyAI.PauseForTowerAttack(false);
        nextAttackTime = Time.time + attackCooldown;
        attackRoutine = null;
    }

    private void ResolveTower()
    {
        GameObject towerObject = GameObject.FindGameObjectWithTag("Tower");
        if (towerObject != null) targetTower = towerObject.GetComponent<TowerController>();
    }

    private void OnDisable()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = null;
        IsAttacking = false;
        if (enemyAI != null) enemyAI.PauseForTowerAttack(false);
    }
}
