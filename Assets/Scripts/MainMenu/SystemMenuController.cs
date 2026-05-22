using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class SystemMenuController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;
    [SerializeField] private TMP_Dropdown languageSelector;


    void Start()
    {
        UpdateMenu();

        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            PopulateLanguagesDropdown();
        }
        else
        {
            LocalizationSettings.InitializationOperation.Completed += _ => PopulateLanguagesDropdown();
        }
    }

    public override void UpdateMenu()
    {
        startButton.gameObject.SetActive(false);
    }

    // Locale
    public void OnLanguageChange(int index)
    {
        Loc.SetLanguage(LocalizationSettings.AvailableLocales.Locales[index].Identifier.Code);
    }

    private void PopulateLanguagesDropdown()
    {
        List<UnityEngine.Localization.Locale> locales = LocalizationSettings.AvailableLocales.Locales;

        List<TMP_Dropdown.OptionData> options = new();

        foreach (UnityEngine.Localization.Locale locale in locales)
        {
            options.Add(new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName));
        }

        languageSelector.options = options;

        int currentIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
        languageSelector.SetValueWithoutNotify(currentIndex);
    }
}
