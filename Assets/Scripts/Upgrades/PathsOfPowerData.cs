using UnityEngine;


//[CreateAssetMenu(fileName = "paths data", menuName = "paths data")]
public class PathsOfPowerData : ScriptableObject
{

    [Header("#---- EQUILIBRIUM ----#")]
    public Sprite equilibriumIcon;
    [SerializeField] private string equilibriumNameEnglish;
    [SerializeField] private string equilibriumNameChinese;
    [SerializeField] private string equilibriumNamePortuguese;
    [SerializeField] private string equilibriumNameSpanish;

    [Header("#---- POWER ----#")]
    public Sprite powerIcon;
    [SerializeField] private string powerNameEnglish;
    [SerializeField] private string powerNameChinese;
    [SerializeField] private string powerNamePortuguese;
    [SerializeField] private string powerNameSpanish;

    [Header("#---- LUCK ----#")]
    public Sprite luckIcon;
    [SerializeField] private string luckNameEnglish;
    [SerializeField] private string luckNameChinese;
    [SerializeField] private string luckNamePortuguese;
    [SerializeField] private string luckNameSpanish;

    [Header("#---- TIME ----#")]
    public Sprite timeIcon;
    [SerializeField] private string timeNameEnglish;
    [SerializeField] private string timeNameChinese;
    [SerializeField] private string timeNamePortuguese;
    [SerializeField] private string timeNameSpanish;

    [Header("#---- FOCUS ----#")]
    public Sprite focusIcon;
    [SerializeField] private string focusNameEnglish;
    [SerializeField] private string focusNameChinese;
    [SerializeField] private string focusNamePortuguese;
    [SerializeField] private string focusNameSpanish;

    public Sprite GetIcon(PathsOfPowerNames path)
    {
        switch (path)
        {
            case PathsOfPowerNames.equilibrium:
                return equilibriumIcon;
            case PathsOfPowerNames.strength:
                return powerIcon;
            case PathsOfPowerNames.luck:
                return luckIcon;
            case PathsOfPowerNames.time:
                return timeIcon;
            case PathsOfPowerNames.focus:
                return focusIcon;
            default:
                return null;
        }
    }


    #region Get Name

    public string GetName(PathsOfPowerNames path, Language language)
    {
        switch (path)
        {
            case PathsOfPowerNames.equilibrium:
                return GetEquilibriumName(language);
            case PathsOfPowerNames.strength:
                return GetPowerName(language);
            case PathsOfPowerNames.luck:
                return GetLuckName(language);
            case PathsOfPowerNames.time:
                return GetTimeName(language);
            case PathsOfPowerNames.focus:
                return GetFocusName(language);
            default:
                return string.Empty;
        }
    }

    private string GetEquilibriumName(Language language)
    {
        switch (language)
        {
            case Language.english: return equilibriumNameEnglish;
            case Language.chinese: return equilibriumNameChinese;
            case Language.portuguese: return equilibriumNamePortuguese;
            case Language.spanish: return equilibriumNameSpanish;
            default: return string.Empty;
        }
    }

    private string GetPowerName(Language language)
    {
        switch (language)
        {
            case Language.english: return powerNameEnglish;
            case Language.chinese: return powerNameChinese;
            case Language.portuguese: return powerNamePortuguese;
            case Language.spanish: return powerNameSpanish;
            default: return string.Empty;
        }
    }

    private string GetLuckName(Language language)
    {
        switch (language)
        {
            case Language.english: return luckNameEnglish;
            case Language.chinese: return luckNameChinese;
            case Language.portuguese: return luckNamePortuguese;
            case Language.spanish: return luckNameSpanish;
            default: return string.Empty;
        }
    }

    private string GetTimeName(Language language)
    {
        switch (language)
        {
            case Language.english: return timeNameEnglish;
            case Language.chinese: return timeNameChinese;
            case Language.portuguese: return timeNamePortuguese;
            case Language.spanish: return timeNameSpanish;
            default: return string.Empty;
        }
    }

    private string GetFocusName(Language language)
    {
        switch (language)
        {
            case Language.english: return focusNameEnglish;
            case Language.chinese: return focusNameChinese;
            case Language.portuguese: return focusNamePortuguese;
            case Language.spanish: return focusNameSpanish;
            default: return string.Empty;
        }
    }

    #endregion
}

public enum PathsOfPowerNames { equilibrium, strength, luck, time, focus}
