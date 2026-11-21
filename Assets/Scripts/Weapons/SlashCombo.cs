using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SlashStep
{
    [Min(0f)] public float windup, activeTime, recovery;
    [Min(0.1f)] public float reach;
    [Range(1f, 180f)] public float arc;
    [Min(0f)] public float lungeSpeed, damage, push, hitStop;

    public SlashStep(float windup, float activeTime, float recovery, float reach, float arc,
        float lungeSpeed, float damage, float push, float hitStop)
    {
        this.windup = windup;
        this.activeTime = activeTime;
        this.recovery = recovery;
        this.reach = reach;
        this.arc = arc;
        this.lungeSpeed = lungeSpeed;
        this.damage = damage;
        this.push = push;
        this.hitStop = hitStop;
    }
}

// One instance per melee manager. Only the opening press consumes the action charge.
public sealed class SlashCombo
{
    private readonly MeleeAttackHandler handler;
    private readonly PlayerInputHandler input;
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private readonly HashSet<EnemyHealth> struck = new HashSet<EnemyHealth>();
    private readonly List<RaycastHit2D> walls = new List<RaycastHit2D>();
    private readonly ContactFilter2D wallFilter;
    private MeleeData weapon;
    private SlashStep step;
    private int index;
    private float started, aimAngle;
    private Vector2 direction;
    private bool buffered, sampled, confirmed;
    private float impactBufferUntil;
    private LineRenderer trail;

    public SlashCombo(MeleeAttackHandler handler, PlayerInputHandler input)
    {
        this.handler = handler;
        this.input = input;
        wallFilter = new ContactFilter2D {
            useTriggers = false, useLayerMask = true,
            layerMask = LayerMask.GetMask("Obstacle", "ClosedDoor", "cliff Obstacle", "Table")
        };
    }

    public bool Active => step != null;
    private float ReadyAt => started + step.windup + step.activeTime + step.recovery;

    public void Start(MeleeData equipped)
    {
        weapon = equipped;
        Begin(0);
    }

    private void Begin(int next)
    {
        index = next;
        step = next == 0 ? weapon.openingSlash : next == 1 ? weapon.returnSlash : weapon.finishingSlash;
        started = Time.time;
        aimAngle = input.GetAimAngle();
        direction = Quaternion.Euler(0f, 0f, aimAngle) * Vector2.right;
        buffered = sampled = confirmed = false;
        impactBufferUntil = -1f;
        struck.Clear();

        Rigidbody2D body = handler.PlayerBody;
        float speed = step.lungeSpeed;
        float distance = speed * Mathf.Max(0.05f, step.windup + step.activeTime);
        if (distance > 0f && body.Cast(direction, wallFilter, walls, distance) > 0)
        {
            float clearance = distance;
            foreach (var wall in walls) clearance = Mathf.Min(clearance, wall.distance);
            speed *= Mathf.Clamp01((clearance - 0.03f) / distance);
        }
        body.AddForce(direction * (speed * body.mass), ForceMode2D.Impulse);
        handler.PlaySlashSwing(next == 2);
    }

    public void Press()
    {
        if (index == 2 || Time.time > ReadyAt + weapon.comboLinkWindow) return;
        if (Time.time >= ReadyAt - weapon.comboInputBuffer || Time.unscaledTime <= impactBufferUntil)
            buffered = true;
    }

    public void Tick()
    {
        if (Time.deltaTime <= 0f) return;
        float elapsed = Time.time - started;
        if (elapsed < step.windup) return;
        float swingTime = elapsed - step.windup;
        if (!sampled || swingTime < step.activeTime)
        {
            sampled = true;
            DrawArc(Mathf.Clamp01(swingTime / Mathf.Max(0.001f, step.activeTime)));
            CheckHits();
        }
        else if (trail != null) trail.enabled = false;

        if (Time.time < ReadyAt) return;
        if (index == 2 || Time.time > ReadyAt + weapon.comboLinkWindow) Cancel();
        else if (buffered) Begin(index + 1);
    }

    private void CheckHits()
    {
        Vector2 origin = handler.PlayerBody.position;
        var filter = new ContactFilter2D { useTriggers = true };
        Physics2D.OverlapCircle(origin, step.reach, filter, overlaps);
        foreach (Collider2D hit in overlaps)
        {
            if (hit == null) continue;
            var receiver = hit.GetComponentInParent<HandleAttackOnEnemy>();
            if (receiver == null || receiver.SlashTarget == null || struck.Contains(receiver.SlashTarget)) continue;
            Vector2 point = hit.ClosestPoint(origin);
            Vector2 offset = point - origin;
            if (Vector2.Angle(direction, offset) > step.arc * 0.5f) continue;
            if (offset.sqrMagnitude > 0.0001f && Physics2D.Linecast(origin, point, wallFilter, walls) > 0) continue;
            var target = receiver.SlashTarget;
            if (!receiver.TrySlashHit(weapon, step, direction, handler.DamageMultiplier, index == 2)) continue;
            struck.Add(target);
            if (confirmed) continue;
            confirmed = true;
            impactBufferUntil = Time.unscaledTime + step.hitStop;
            handler.PlaySlashImpact(step.hitStop, aimAngle, weapon.camShakeMagnitude * (index == 2 ? 1f : 0.35f));
        }
    }

    private void DrawArc(float progress)
    {
        if (trail == null)
        {
            var visual = new GameObject("Melee slash");
            visual.transform.SetParent(handler.transform, false);
            trail = visual.AddComponent<LineRenderer>();
            trail.sharedMaterial = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
            trail.sortingLayerName = "AbovePlayer";
            trail.useWorldSpace = true;
            trail.positionCount = 18;
            trail.numCapVertices = 2;
        }
        trail.enabled = true;
        trail.startWidth = 0.015f;
        trail.endWidth = index == 2 ? 0.18f : 0.11f;
        trail.startColor = new Color(1f, 0.75f, 0.55f, 0f);
        trail.endColor = new Color(1f, 0.94f, 0.8f, 1f);
        float sign = index == 1 ? -1f : 1f;
        Vector3 origin = handler.PlayerBody.transform.position;
        for (int i = 0; i < trail.positionCount; i++)
        {
            float t = i / (float)(trail.positionCount - 1);
            float head = Mathf.Lerp(0.35f, 1f, progress);
            float sweep = Mathf.Lerp(Mathf.Max(0f, head - 0.65f), head, t);
            float radians = (aimAngle + sign * (sweep - 0.5f) * step.arc) * Mathf.Deg2Rad;
            trail.SetPosition(i, origin + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians)) * step.reach);
        }
    }

    public void Cancel()
    {
        step = null;
        buffered = false;
        if (trail != null) trail.enabled = false;
    }
}
