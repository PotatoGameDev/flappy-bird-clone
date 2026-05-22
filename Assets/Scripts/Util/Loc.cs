using System.Threading.Tasks;
using System;
using UnityEngine.Localization.Settings;

public static class Loc
{
    public static event Action OnLanguageChanged;

    private const string PrefsKey = "user-locale";

    static Loc()
    {
        LocalizationSettings.SelectedLocaleChanged += _ => OnLanguageChanged?.Invoke();
    }

    public static string Get(string key, string table = "UI_Strings")
    {
        return LocalizationSettings.StringDatabase
            .GetLocalizedString(table, key);
    }

    public static string Get(string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase
            .GetLocalizedString("UI_Strings", key, args);
    }

    public static async Task<string> GetAsync(string key, string table = "UI_Strings")
    {
        var op = LocalizationSettings.StringDatabase
            .GetLocalizedStringAsync(table, key);
        await op.Task;
        return op.Result;
    }

    public static string GetRandom(string keyPrefix, int max, string table, params object[] args)
    {
        int index = UnityEngine.Random.Range(0, max);

        string key = $"{keyPrefix}_{index}";

        return args.Length > 0
            ? LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args)
            : LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }

    public static void SetLanguage(string localeCode)
    {
        if (!LocalizationSettings.InitializationOperation.IsDone)
        {
            LocalizationSettings.InitializationOperation.Completed += _ => SetLanguage(localeCode);
            return;
        }
        var locale = LocalizationSettings.AvailableLocales.Locales
            .Find(l => l.Identifier.Code == localeCode);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;

            UnityEngine.PlayerPrefs.SetString(PrefsKey, localeCode);
            UnityEngine.PlayerPrefs.Save();
        }
    }

    public static void LoadSavedLanguage()
    {
        if (!UnityEngine.PlayerPrefs.HasKey(PrefsKey))
        {
            return;
        }
        string val = UnityEngine.PlayerPrefs.GetString(PrefsKey);
        SetLanguage(val);
    }
}
