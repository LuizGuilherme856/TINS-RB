using System;
using UnityEngine;

/// <summary>Ammunition for one inventory entry, independent of its GunData definition.</summary>
public sealed class GunAmmo
{
    public int Rounds { get; private set; }
    public int Reserve { get; private set; }
    public event Action Changed;

    internal void Reset(GunData gun)
    {
        Rounds = gun.GetMaxAmmoOnClip(true);
        RefillReserve(gun);
    }

    internal void RefillReserve(GunData gun)
    {
        Reserve = gun.GetMaxSpareAmmo();
        Changed?.Invoke();
    }

    internal void SetRounds(int rounds)
    {
        Rounds = Mathf.Max(0, rounds);
        Changed?.Invoke();
    }

    internal void Spend(int rounds, bool spendFromReserve)
    {
        if (spendFromReserve && rounds > Rounds)
            Reserve = Mathf.Max(0, Reserve - (rounds - Rounds));
        SetRounds(Rounds - rounds);
    }

    internal void Reload(int rounds, int reserveSpent)
    {
        Reserve = Mathf.Max(0, Reserve - reserveSpent);
        SetRounds(rounds);
    }
}
