using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AztecEngine.Code.Modules.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using I2.Loc;
using UnityEngine;

namespace WWP.Ukrainian
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        internal const string PluginGuid = "games.trembita.wwp.ukrainian";
        internal const string PluginName = "Wild West Pioneers Ukrainian";
        internal const string PluginVersion = "1.0.0";

        internal static ManualLogSource PluginLogger;
        internal static ConfigFile PluginConfig;
        internal static ConfigEntry<bool> UseUkrainian;

        public override void Load()
        {
            PluginLogger = Log;
            PluginConfig = Config;

            UseUkrainian = Config.Bind("Localization", "UseUkrainian", false, "Remember Ukrainian as the selected text language.");

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
            Log.LogInfo($"Game version: {Application.version}");
            Log.LogInfo($"Saved Ukrainian preference: {UseUkrainian.Value}");

            AddComponent<UkrainianLocalizationController>();

            Log.LogInfo("Ukrainian localization controller added.");
        }

        internal static void SaveUkrainianPreference(bool enabled)
        {
            if (UseUkrainian.Value == enabled)
            {
                return;
            }

            UseUkrainian.Value = enabled;
            PluginConfig.Save();

            PluginLogger.LogInfo($"Saved Ukrainian preference: {enabled}");
        }
    }

    public class UkrainianLocalizationController : MonoBehaviour
    {
        private const string UkrainianLanguageName = "Ukrainian";
        private const string UkrainianLanguageCode = "uk";

        private float nextCheckTime;
        private float preferenceProtectionUntil;

        private bool i2Initialized;
        private bool startupPreferenceInitialized;
        private bool languageObservationInitialized;
        private bool menuStateLogged;

        private string lastObservedLanguage = "";

        public void Update()
        {
            if (Time.realtimeSinceStartup < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.realtimeSinceStartup + 0.25f;

            try
            {
                if (!i2Initialized)
                {
                    i2Initialized = InitializeUkrainianLocalization();

                    if (!i2Initialized)
                    {
                        return;
                    }
                }

                if (!startupPreferenceInitialized)
                {
                    InitializeSavedPreference();
                }

                ProtectSavedPreferenceDuringStartup();
                EnsureUkrainianInSettingsMenu();
                ObserveLanguageChanges();
            }
            catch (Exception ex)
            {
                Plugin.PluginLogger.LogError($"Ukrainian localization update failed: {ex}");
            }
        }

        private bool InitializeUkrainianLocalization()
        {
            var sources = LocalizationManager.Sources;

            if (sources.Count == 0)
            {
                return false;
            }

            var source = sources[0];

            Plugin.PluginLogger.LogInfo($"I2 before patch: Terms={source.mTerms.Count}, Languages={source.mLanguages.Count}");

            var ukrainianIndex = source.GetLanguageIndex(UkrainianLanguageName);

            if (ukrainianIndex < 0)
            {
                Plugin.PluginLogger.LogInfo("Adding Ukrainian language to I2...");

                source.AddLanguage(UkrainianLanguageName, UkrainianLanguageCode);
                ukrainianIndex = source.GetLanguageIndex(UkrainianLanguageName);
            }

            if (ukrainianIndex < 0)
            {
                Plugin.PluginLogger.LogError("Failed to add Ukrainian language to I2.");
                return false;
            }

            Plugin.PluginLogger.LogInfo($"Ukrainian I2 index: {ukrainianIndex}");
            Plugin.PluginLogger.LogInfo($"I2 language count: {source.mLanguages.Count}");

            if (!LoadTranslations(source, ukrainianIndex))
            {
                return false;
            }

            source.UpdateDictionary(true);

            Plugin.PluginLogger.LogInfo("Ukrainian language initialized in I2.");

            return true;
        }

        private bool LoadTranslations(LanguageSourceData source, int ukrainianIndex)
        {
            var translationDirectoryPath = GetTranslationDirectoryPath();

            Plugin.PluginLogger.LogInfo($"Loading Ukrainian translations from: {translationDirectoryPath}");

            if (!Directory.Exists(translationDirectoryPath))
            {
                Plugin.PluginLogger.LogError($"Translation directory not found: {translationDirectoryPath}");
                return false;
            }

            var translationFiles = Directory.GetFiles(translationDirectoryPath, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(translationFiles, StringComparer.Ordinal);

            if (translationFiles.Length == 0)
            {
                Plugin.PluginLogger.LogError($"No translation files found in: {translationDirectoryPath}");
                return false;
            }

            var seenTerms = new HashSet<string>(StringComparer.Ordinal);
            var loadedFiles = 0;
            var failedFiles = 0;
            var totalEntries = 0;
            var matchedTerms = 0;
            var applied = 0;
            var empty = 0;
            var missing = 0;
            var invalid = 0;
            var domainMismatch = 0;
            var duplicates = 0;

            foreach (var translationFile in translationFiles)
            {
                var fileName = Path.GetFileName(translationFile);
                var expectedDomain = Path.GetFileNameWithoutExtension(translationFile);
                Dictionary<string, string> translations;

                try
                {
                    var json = File.ReadAllText(translationFile);
                    var options = new JsonSerializerOptions
                    {
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };

                    translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
                }
                catch (Exception ex)
                {
                    failedFiles++;
                    Plugin.PluginLogger.LogError($"Failed to read translation file '{fileName}': {ex}");
                    continue;
                }

                if (translations == null)
                {
                    failedFiles++;
                    Plugin.PluginLogger.LogError($"Translation file '{fileName}' contains no translation data.");
                    continue;
                }

                loadedFiles++;
                totalEntries += translations.Count;

                var fileMatched = 0;
                var fileApplied = 0;
                var fileEmpty = 0;
                var fileMissing = 0;
                var fileInvalid = 0;
                var fileDomainMismatch = 0;
                var fileDuplicates = 0;

                foreach (var translation in translations)
                {
                    if (string.IsNullOrWhiteSpace(translation.Key))
                    {
                        invalid++;
                        fileInvalid++;
                        continue;
                    }

                    if (translation.Value == null)
                    {
                        invalid++;
                        fileInvalid++;
                        Plugin.PluginLogger.LogWarning($"Null translation value in '{fileName}': {translation.Key}");
                        continue;
                    }

                    var separatorIndex = translation.Key.IndexOf('/');

                    if (separatorIndex <= 0)
                    {
                        invalid++;
                        fileInvalid++;
                        Plugin.PluginLogger.LogWarning($"Invalid localization key in '{fileName}': {translation.Key}");
                        continue;
                    }

                    var actualDomain = translation.Key.Substring(0, separatorIndex);

                    if (!string.Equals(actualDomain, expectedDomain, StringComparison.Ordinal))
                    {
                        domainMismatch++;
                        fileDomainMismatch++;
                        Plugin.PluginLogger.LogWarning($"Domain mismatch in '{fileName}': key '{translation.Key}' belongs to domain '{actualDomain}'.");
                        continue;
                    }

                    if (!seenTerms.Add(translation.Key))
                    {
                        duplicates++;
                        fileDuplicates++;
                        Plugin.PluginLogger.LogWarning($"Duplicate translation key ignored in '{fileName}': {translation.Key}");
                        continue;
                    }

                    var term = source.GetTermData(translation.Key);

                    if (term == null)
                    {
                        missing++;
                        fileMissing++;
                        Plugin.PluginLogger.LogWarning($"Game term not found for translation key in '{fileName}': {translation.Key}");
                        continue;
                    }

                    if (term.Languages.Length <= ukrainianIndex)
                    {
                        invalid++;
                        fileInvalid++;
                        Plugin.PluginLogger.LogWarning($"Term has no Ukrainian language slot in '{fileName}': {translation.Key}");
                        continue;
                    }

                    matchedTerms++;
                    fileMatched++;

                    if (string.IsNullOrWhiteSpace(translation.Value))
                    {
                        empty++;
                        fileEmpty++;
                        continue;
                    }

                    term.Languages[ukrainianIndex] = translation.Value;
                    applied++;
                    fileApplied++;
                }

                Plugin.PluginLogger.LogInfo($"Translation file '{fileName}': entries={translations.Count}, matched={fileMatched}, applied={fileApplied}, empty={fileEmpty}, missing={fileMissing}, invalid={fileInvalid}, domainMismatch={fileDomainMismatch}, duplicates={fileDuplicates}");
            }

            var keyCoverage = source.mTerms.Count > 0 ? matchedTerms * 100.0 / source.mTerms.Count : 0.0;
            var translationCoverage = source.mTerms.Count > 0 ? applied * 100.0 / source.mTerms.Count : 0.0;

            Plugin.PluginLogger.LogInfo("----- UKRAINIAN TRANSLATION SUMMARY -----");
            Plugin.PluginLogger.LogInfo($"Translation files found: {translationFiles.Length}");
            Plugin.PluginLogger.LogInfo($"Translation files loaded: {loadedFiles}");
            Plugin.PluginLogger.LogInfo($"Translation files failed: {failedFiles}");
            Plugin.PluginLogger.LogInfo($"Translation entries loaded: {totalEntries}");
            Plugin.PluginLogger.LogInfo($"Unique localization keys: {seenTerms.Count}");
            Plugin.PluginLogger.LogInfo($"Game terms matched: {matchedTerms}/{source.mTerms.Count} ({keyCoverage:F2}%)");
            Plugin.PluginLogger.LogInfo($"Ukrainian translations applied: {applied}/{source.mTerms.Count} ({translationCoverage:F2}%)");
            Plugin.PluginLogger.LogInfo($"Empty translations skipped: {empty}");
            Plugin.PluginLogger.LogInfo($"Missing game terms: {missing}");
            Plugin.PluginLogger.LogInfo($"Invalid translations: {invalid}");
            Plugin.PluginLogger.LogInfo($"Domain mismatches: {domainMismatch}");
            Plugin.PluginLogger.LogInfo($"Duplicate keys: {duplicates}");
            Plugin.PluginLogger.LogInfo("----- END UKRAINIAN TRANSLATION SUMMARY -----");

            return loadedFiles > 0;
        }

        private static string GetTranslationDirectoryPath()
        {
            var assemblyPath = typeof(Plugin).Assembly.Location;
            var pluginDirectory = Path.GetDirectoryName(assemblyPath);

            if (string.IsNullOrEmpty(pluginDirectory))
            {
                pluginDirectory = Paths.PluginPath;
            }

            return Path.Combine(pluginDirectory, "uk-UA");
        }

        private void InitializeSavedPreference()
        {
            startupPreferenceInitialized = true;
            preferenceProtectionUntil = Time.realtimeSinceStartup + 5.0f;

            if (!Plugin.UseUkrainian.Value)
            {
                Plugin.PluginLogger.LogInfo($"No Ukrainian preference to restore. Current I2 language: '{LocalizationManager.CurrentLanguage}'");
                return;
            }

            RestoreUkrainianLanguage();
        }

        private void ProtectSavedPreferenceDuringStartup()
        {
            if (!Plugin.UseUkrainian.Value)
            {
                return;
            }

            if (Time.realtimeSinceStartup > preferenceProtectionUntil)
            {
                return;
            }

            if (string.Equals(LocalizationManager.CurrentLanguage, UkrainianLanguageName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Plugin.PluginLogger.LogInfo($"Game changed language to '{LocalizationManager.CurrentLanguage}' during startup. Restoring Ukrainian.");

            RestoreUkrainianLanguage();
        }

        private void RestoreUkrainianLanguage()
        {
            LocalizationManager.CurrentLanguage = UkrainianLanguageName;
            LocalizationManager.LocalizeAll(true);

            Plugin.PluginLogger.LogInfo($"Ukrainian restored. Current language: '{LocalizationManager.CurrentLanguage}'");
        }

        private void ObserveLanguageChanges()
        {
            if (Plugin.UseUkrainian.Value && Time.realtimeSinceStartup <= preferenceProtectionUntil)
            {
                lastObservedLanguage = LocalizationManager.CurrentLanguage;
                languageObservationInitialized = true;
                return;
            }

            var currentLanguage = LocalizationManager.CurrentLanguage;

            if (!languageObservationInitialized)
            {
                lastObservedLanguage = currentLanguage;
                languageObservationInitialized = true;

                Plugin.PluginLogger.LogInfo($"Initial observed language: '{currentLanguage}'");
                return;
            }

            if (string.Equals(lastObservedLanguage, currentLanguage, StringComparison.Ordinal))
            {
                return;
            }

            Plugin.PluginLogger.LogInfo($"I2 language changed: '{lastObservedLanguage}' -> '{currentLanguage}'");

            var wasUkrainian = string.Equals(lastObservedLanguage, UkrainianLanguageName, StringComparison.OrdinalIgnoreCase);
            var isUkrainian = string.Equals(currentLanguage, UkrainianLanguageName, StringComparison.OrdinalIgnoreCase);

            if (isUkrainian)
            {
                Plugin.SaveUkrainianPreference(true);
            }
            else if (wasUkrainian)
            {
                Plugin.SaveUkrainianPreference(false);
            }

            lastObservedLanguage = currentLanguage;
        }

        private void EnsureUkrainianInSettingsMenu()
        {
            var controllers = Resources.FindObjectsOfTypeAll<GeneralOptionsController>();

            if (controllers.Length == 0)
            {
                return;
            }

            for (var controllerIndex = 0; controllerIndex < controllers.Length; controllerIndex++)
            {
                var controller = controllers[controllerIndex];

                if (controller == null || controller.gameObject == null || !controller.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var languages = controller._listOfLanguages;
                var languageCodes = controller._listOfLanguagesCodes;
                var dropdown = controller.languageDropdown;

                if (languages == null || languageCodes == null || dropdown == null)
                {
                    continue;
                }

                if (languages.Count < 7 || languageCodes.Count < 7 || dropdown.GetOptionsCount() < 7)
                {
                    continue;
                }

                var ukrainianIndex = EnsureLanguageLists(languages, languageCodes, dropdown);

                SynchronizeDropdown(dropdown, ukrainianIndex);

                if (!menuStateLogged)
                {
                    Plugin.PluginLogger.LogInfo($"Language names: {languages.Count}");
                    Plugin.PluginLogger.LogInfo($"Language codes: {languageCodes.Count}");
                    Plugin.PluginLogger.LogInfo($"Dropdown options: {dropdown.GetOptionsCount()}");
                    Plugin.PluginLogger.LogInfo($"Dropdown selected index: {dropdown.GetCurrentOptionIndex()}");
                    Plugin.PluginLogger.LogInfo($"Dropdown selected text: '{dropdown.GetCurrentOptionText()}'");

                    menuStateLogged = true;
                }
            }
        }

        private int EnsureLanguageLists(Il2CppSystem.Collections.Generic.List<string> languages, Il2CppSystem.Collections.Generic.List<string> languageCodes, AdvancedArrowSelector dropdown)
        {
            var ukrainianIndex = FindValue(languages, UkrainianLanguageName);

            if (ukrainianIndex < 0)
            {
                languages.Add(UkrainianLanguageName);
                ukrainianIndex = languages.Count - 1;

                Plugin.PluginLogger.LogInfo($"Added Ukrainian to language names. Index={ukrainianIndex}");
            }

            if (FindValue(languageCodes, UkrainianLanguageCode) < 0)
            {
                languageCodes.Add(UkrainianLanguageCode);

                Plugin.PluginLogger.LogInfo($"Added Ukrainian language code '{UkrainianLanguageCode}'.");
            }

            if (dropdown.GetOptionsCount() < languages.Count)
            {
                dropdown.AddOptions(UkrainianLanguageName);

                Plugin.PluginLogger.LogInfo($"Added Ukrainian to language selector. Options={dropdown.GetOptionsCount()}");
            }

            if (dropdown.optionsList != null && dropdown.optionsList.Count > ukrainianIndex)
            {
                dropdown.optionsList[ukrainianIndex].nameText = UkrainianLanguageName;
            }

            return ukrainianIndex;
        }

        private void SynchronizeDropdown(AdvancedArrowSelector dropdown, int ukrainianIndex)
        {
            if (!string.Equals(LocalizationManager.CurrentLanguage, UkrainianLanguageName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (dropdown.GetCurrentOptionIndex() != ukrainianIndex)
            {
                Plugin.PluginLogger.LogInfo($"Synchronizing language selector index: {dropdown.GetCurrentOptionIndex()} -> {ukrainianIndex}");

                dropdown.SelectOption(ukrainianIndex, false);
            }

            if (dropdown.targetText != null && !string.Equals(dropdown.targetText.text, UkrainianLanguageName, StringComparison.Ordinal))
            {
                dropdown.targetText.text = UkrainianLanguageName;

                Plugin.PluginLogger.LogInfo("Language selector visible text corrected to 'Ukrainian'.");
            }
        }

        private static int FindValue(Il2CppSystem.Collections.Generic.List<string> list, string value)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
