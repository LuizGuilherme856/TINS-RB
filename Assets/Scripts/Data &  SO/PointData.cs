using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointData : ScriptableObject
{
    public int pointsPerShotHit = 10;
    public int pointsPerMeleeHit = 20;
    public int pointsPerShotKill = 80;
    public int pointsPerMeleeKill = 120;
    public int pointsPerExplosionKill = 40;

    public int GetKillPoints(KillType type)
    {
        if (type == KillType.shot) return pointsPerShotKill;
        else if (type == KillType.melee) return pointsPerMeleeKill;
        else if (type == KillType.explosion) return pointsPerExplosionKill;
        else throw new System.Exception("KillType não suportado");
    }

    public int GetHitPoints(HitType type)
    {
        if (type == HitType.shot) return pointsPerShotHit;
        else if (type == HitType.melee) return pointsPerMeleeHit;
        else throw new System.Exception("Hit type nao suportado");
    }
}

public enum KillType { shot, melee, explosion}
public enum HitType { shot, melee }