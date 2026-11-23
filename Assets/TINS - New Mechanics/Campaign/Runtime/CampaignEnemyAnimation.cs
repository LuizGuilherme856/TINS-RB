using UnityEngine;

namespace TINS.Campaign
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth), typeof(EnemyTowerAttack))]
    public sealed class CampaignEnemyAnimation : MonoBehaviour
    {
        public const string MovingParameter = "Moving";
        public const string HitTrigger = "Hit";
        public const string AttackTrigger = "TowerAttack";

        private static readonly int Moving = Animator.StringToHash(MovingParameter);
        private static readonly int Hit = Animator.StringToHash(HitTrigger);

        private Animator animator;
        private SpriteRenderer sprites;
        private Rigidbody2D body;
        private EnemyHealth health;
        private EnemyTowerAttack towerAttack;
        private MoveTowardsPlayer chaser;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            sprites = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<EnemyHealth>();
            towerAttack = GetComponent<EnemyTowerAttack>();
            chaser = GetComponent<MoveTowardsPlayer>();
        }

        private void OnEnable()
        {
            sprites.flipX = false;
            health.OnTakeDamage += PlayHit;
        }

        private void OnDisable() => health.OnTakeDamage -= PlayHit;

        private void LateUpdate()
        {
            if (!animator.isInitialized) return;

            Vector2 velocity = body.linearVelocity;
            bool attacking = towerAttack.IsAttacking;
            bool moving = !attacking && !health.IsCampaignFlashStunned && velocity.sqrMagnitude > 0.0225f;
            animator.SetBool(Moving, moving);

            float horizontal = velocity.x;
            if (attacking && chaser != null && chaser.CurrentTarget != null)
                horizontal = chaser.CurrentTarget.position.x - transform.position.x;

            // The imported frames face left. Flip the renderer without mirroring the colliders.
            if ((moving || attacking) && Mathf.Abs(horizontal) > 0.05f)
                sprites.flipX = horizontal > 0f;
        }

        private void PlayHit()
        {
            if (health.currentHealth > 0f && animator.isInitialized)
                animator.SetTrigger(Hit);
        }
    }
}
