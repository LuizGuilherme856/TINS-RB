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
        public const string NorthFacingParameter = "NorthFacing";

        private static readonly int Moving = Animator.StringToHash(MovingParameter);
        private static readonly int Hit = Animator.StringToHash(HitTrigger);
        private static readonly int NorthFacing = Animator.StringToHash(NorthFacingParameter);
        private const float FacingDeadZone = 0.05f;

        [SerializeField] private bool fourDirections;

        private Animator animator;
        private SpriteRenderer sprites;
        private Rigidbody2D body;
        private EnemyHealth health;
        private EnemyTowerAttack towerAttack;
        private MoveTowardsPlayer chaser;
        private bool facingNorth;

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
            facingNorth = false;
            if (fourDirections && animator.isInitialized)
                animator.SetFloat(NorthFacing, 0f);
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

            if (fourDirections)
            {
                if (moving) SetFacing(velocity);
                return;
            }

            float horizontal = velocity.x;
            if (attacking && chaser != null && chaser.CurrentTarget != null)
                horizontal = chaser.CurrentTarget.position.x - transform.position.x;

            // The imported frames face left. Flip the renderer without mirroring the colliders.
            if ((moving || attacking) && Mathf.Abs(horizontal) > FacingDeadZone)
                sprites.flipX = horizontal > 0f;
        }

        internal void FaceAttackTarget(Vector2 direction)
        {
            if (fourDirections && animator.isInitialized)
                SetFacing(direction);
        }

        internal void CopyCorpseFacing(GameObject corpse)
        {
            if (fourDirections && corpse.TryGetComponent(out Animator corpseAnimator))
                corpseAnimator.SetFloat(NorthFacing, facingNorth ? 1f : 0f);
        }

        private void SetFacing(Vector2 direction)
        {
            // Keep the last side on an axis when moving almost exactly along the other one.
            if (Mathf.Abs(direction.x) > FacingDeadZone) sprites.flipX = direction.x > 0f;
            if (Mathf.Abs(direction.y) > FacingDeadZone) facingNorth = direction.y > 0f;
            animator.SetFloat(NorthFacing, facingNorth ? 1f : 0f);
        }

        private void PlayHit()
        {
            if (health.currentHealth > 0f && animator.isInitialized)
                animator.SetTrigger(Hit);
        }
    }
}
