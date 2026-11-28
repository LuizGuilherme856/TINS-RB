using System.Collections;
using TINS.Campaign;
using UnityEngine;

/// <summary>Resolves a tower strike after its wind-up or at the attack animation's impact event.</summary>
public sealed class EnemyTowerAttack : MonoBehaviour
{
    [SerializeField] private Animator attackAnimator;
    [SerializeField] private TowerController targetTower;
    [SerializeField, Min(0.1f)] private float attackRange = 1.1f;
    [SerializeField, Min(0f)] private float attackCooldown = 1f;
    [SerializeField, Min(0.1f)] private float attackAnimationDuration = .7f;
    [SerializeField, Min(0.05f)] private float simulatedWindup = .3f;
    [SerializeField, Tooltip("The attack clip must call AnimationImpact on the frame that hits the tower.")]
    private bool useAnimationEvent;
    [SerializeField] private string attackTrigger = "TowerAttack";

    private EnemyAI enemyAI;
    private MoveTowardsPlayer simpleChaser;
    private EnemyHealth enemyHealth;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private float nextAttackTime;
    private Coroutine attackRoutine;
    private bool impactApplied;
    private bool showingWindup;
    private Color originalTint;
    private Color windupTint;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        if (attackAnimator == null) attackAnimator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        simpleChaser = GetComponent<MoveTowardsPlayer>();
        enemyHealth = GetComponent<EnemyHealth>();
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResolveTower();
    }

    private void Update()
    {
        if (IsAttacking || Time.time < nextAttackTime || CannotAttack()) return;
        if (useAnimationEvent && (attackAnimator == null || !attackAnimator.isActiveAndEnabled ||
                                  !attackAnimator.isInitialized)) return;
        if (targetTower == null) ResolveTower();
        if (TowerWithinRange()) BeginAttack();
    }

    private bool CannotAttack()
        => CampaignMissionRuntime.Active?.IsResolved == true ||
           (enemyHealth != null && (enemyHealth.currentHealth <= 0f || enemyHealth.IsCampaignFlashStunned || enemyHealth.IsCampaignRepelled));

    private bool TowerWithinRange()
    {
        if (targetTower == null || targetTower.currentLife <= 0) return false;
        Collider2D towerCollider = targetTower.GetComponent<Collider2D>();
        Vector2 closestPoint = towerCollider != null
            ? towerCollider.ClosestPoint(transform.position)
            : (Vector2)targetTower.transform.position;
        return Vector2.Distance(transform.position, closestPoint) <= attackRange;
    }

    private void BeginAttack()
    {
        IsAttacking = true;
        impactApplied = false;
        SetMovementPaused(true);
        if (body != null && body.bodyType != RigidbodyType2D.Static) body.linearVelocity = Vector2.zero;

        if (useAnimationEvent)
        {
            if (TryGetComponent(out CampaignEnemyAnimation direction))
                direction.FaceAttackTarget(targetTower.transform.position - transform.position);
            attackAnimator.ResetTrigger(attackTrigger);
            attackAnimator.SetTrigger(attackTrigger);
        }
        else if (spriteRenderer != null)
        {
            originalTint = spriteRenderer.color;
            windupTint = originalTint * new Color(1f, .58f, .58f, 1f);
            spriteRenderer.color = windupTint;
            showingWindup = true;
        }
        attackRoutine = StartCoroutine(AttackSequence());
    }

    /// <summary>Add this Animation Event to the contact frame of a tower attack clip.</summary>
    public void AnimationImpact()
    {
        if (useAnimationEvent) ApplyImpact();
    }

    private void ApplyImpact()
    {
        if (!IsAttacking || impactApplied || CannotAttack() || !TowerWithinRange()) return;
        impactApplied = true;
        ClearWindupTint();
        targetTower.TryTakeDamageFromEnemy();
    }

    private IEnumerator AttackSequence()
    {
        float duration = useAnimationEvent ? attackAnimationDuration : Mathf.Max(attackAnimationDuration, simulatedWindup);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return null;
            if (CannotAttack() || !TowerWithinRange()) break;

            elapsed += Time.deltaTime;
            if (!useAnimationEvent && !impactApplied && elapsed >= simulatedWindup) ApplyImpact();
        }
        ResetAttack();
        nextAttackTime = Time.time + attackCooldown;
    }

    private void SetMovementPaused(bool paused)
    {
        if (enemyAI != null) enemyAI.PauseForTowerAttack(paused);
        if (simpleChaser != null) simpleChaser.PauseForTowerAttack(paused);
    }

    private void ClearWindupTint()
    {
        if (!showingWindup) return;
        if (spriteRenderer != null && spriteRenderer.color == windupTint)
            spriteRenderer.color = originalTint;
        showingWindup = false;
    }

    private void ResolveTower()
    {
        var core = CampaignMissionRuntime.Active?.DefenseCore;
        if (core != null && core.TryGetComponent(out targetTower)) return;
        GameObject towerObject = GameObject.FindGameObjectWithTag("Tower");
        if (towerObject != null) targetTower = towerObject.GetComponent<TowerController>();
    }

    private void ResetAttack()
    {
        ClearWindupTint();
        SetMovementPaused(false);
        IsAttacking = false;
        attackRoutine = null;
    }

    private void OnDisable()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        ResetAttack();
        nextAttackTime = 0f;
    }
}
