using UnityEngine;

public abstract class WeaponData : GenericData
{
    public Sprite sprite;
    public int price = 100;
    public int ammoPrice = 50;

    public float criticalChance;
    public float criticalMultiplier;
    [SerializeField] public float damage;
    public DamageType damageType;
    public float knockBackForce = 1f;

    [Range(0, 1)] public float statusChance = 1;
    [Range(5, 30)] public float statusDuration = 5;
    public float statusDamagePerSec = 0;
    public int maxCollisions;


    [Range(0.2f, 10)] public float projectileSize = 1;
    public Sprite buyIcon;
    [SerializeField] protected StatsCalculator statsCalculator;
    public float instaKillChance = 0;

    [Tooltip("multiplica o dano com base na distancia. Na verdade uso o tempo para calcular isso. um valor de 0,5 significa que apos 1 seg o dano cai pela metade")]
    public float damageFalloffMultiplier = 0.01f;
    public float minDamageFalloff = 1;

    public GlobalStats globalStats;

    public abstract float GetDamage(bool getDefaultDamage = false);

    public virtual int GetPrice()
    {
        return price;
    }

    public string GetDamageTypeName(Language language)
    {
        return language switch
        {
            Language.english => damageType switch
            {
                DamageType.Puncture => "Puncture",
                DamageType.Slash => "Slash",
                DamageType.Impact => "Impact",
                DamageType.Explosive => "Explosive",
                DamageType.Fire => "Fire",
                DamageType.Ice => "Ice",
                DamageType.Poison => "Poison",
                DamageType.Water => "Water",
                DamageType.Electricity => "Electricity",
                DamageType.Ether => "Ether",
                DamageType.None => "None",
                _ => "Unknown"
            },
            Language.portuguese => damageType switch
            {
                DamageType.Puncture => "Perfuração",
                DamageType.Slash => "Corte",
                DamageType.Impact => "Impacto",
                DamageType.Explosive => "Explosivo",
                DamageType.Fire => "Fogo",
                DamageType.Ice => "Gelo",
                DamageType.Poison => "Veneno",
                DamageType.Water => "Água",
                DamageType.Electricity => "Eletricidade",
                DamageType.Ether => "Éter",
                DamageType.None => "Nenhum",
                _ => "Desconhecido"
            },
            Language.spanish => damageType switch
            {
                DamageType.Puncture => "Perforación",
                DamageType.Slash => "Corte",
                DamageType.Impact => "Impacto",
                DamageType.Explosive => "Explosivo",
                DamageType.Fire => "Fuego",
                DamageType.Ice => "Hielo",
                DamageType.Poison => "Veneno",
                DamageType.Water => "Agua",
                DamageType.Electricity => "Electricidad",
                DamageType.Ether => "Éter",
                DamageType.None => "Ninguno",
                _ => "Desconocido"
            },
            Language.chinese => damageType switch
            {
                DamageType.Puncture => "刺穿",
                DamageType.Slash => "斩击",
                DamageType.Impact => "冲击",
                DamageType.Explosive => "爆炸",
                DamageType.Fire => "火焰",
                DamageType.Ice => "冰冻",
                DamageType.Poison => "毒性",
                DamageType.Water => "水",
                DamageType.Electricity => "闪电",
                DamageType.Ether => "以太",
                DamageType.None => "无",
                _ => "未知"
            },
            _ => "Unknown"
        };
    }

    public string GetStatusEffectName(Language language, StatusEffect statusEffect)
    {
        return language switch
        {
            Language.english => statusEffect switch
            {
                StatusEffect.Punctured => "Punctured",
                StatusEffect.Bleeding => "Bleeding",
                StatusEffect.Concussion => "Concussion",
                StatusEffect.Exploded => "Exploded",
                StatusEffect.Burning => "Burning",
                StatusEffect.Frozen => "Frozen",
                StatusEffect.Poisoned => "Poisoned",
                StatusEffect.Soaked => "Soaked",
                StatusEffect.Electrocuted => "Electrocuted",
                StatusEffect.None => "None",
                _ => "Unknown"
            },
            Language.portuguese => statusEffect switch
            {
                StatusEffect.Punctured => "Perfurado",
                StatusEffect.Bleeding => "Sangrando",
                StatusEffect.Concussion => "Concussão",
                StatusEffect.Exploded => "Explodido",
                StatusEffect.Burning => "Queimando",
                StatusEffect.Frozen => "Congelado",
                StatusEffect.Poisoned => "Envenenado",
                StatusEffect.Soaked => "Encharcado",
                StatusEffect.Electrocuted => "Eletrocutado",
                StatusEffect.None => "Nenhum",
                _ => "Desconhecido"
            },
            Language.spanish => statusEffect switch
            {
                StatusEffect.Punctured => "Perforado",
                StatusEffect.Bleeding => "Sangrando",
                StatusEffect.Concussion => "Conmoción",
                StatusEffect.Exploded => "Explotado",
                StatusEffect.Burning => "Ardiendo",
                StatusEffect.Frozen => "Congelado",
                StatusEffect.Poisoned => "Envenenado",
                StatusEffect.Soaked => "Empapado",
                StatusEffect.Electrocuted => "Electrocutado",
                StatusEffect.None => "Ninguno",
                _ => "Desconocido"
            },
            Language.chinese => statusEffect switch
            {
                StatusEffect.Punctured => "刺穿",
                StatusEffect.Bleeding => "流血",
                StatusEffect.Concussion => "脑震荡",
                StatusEffect.Exploded => "爆炸",
                StatusEffect.Burning => "燃烧",
                StatusEffect.Frozen => "冻结",
                StatusEffect.Poisoned => "中毒",
                StatusEffect.Soaked => "浸湿",
                StatusEffect.Electrocuted => "触电",
                StatusEffect.None => "无",
                _ => "未知"
            },
            _ => "Unknown"
        };
    }


}
