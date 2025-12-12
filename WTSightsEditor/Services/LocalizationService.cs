using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;

namespace WTSightsEditor.Services
{
    /// <summary>
    /// Service for managing application localization
    /// </summary>
    public class LocalizationService
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private ResourceDictionary? _currentResources;
        private readonly Dictionary<string, ResourceDictionary> _loadedLanguages;
        private string _currentLanguage = string.Empty;

        public event Action? LanguageChanged;

        private LocalizationService()
        {
            _loadedLanguages = new Dictionary<string, ResourceDictionary>();
        }

        /// <summary>
        /// Initializes the localization service (call this after app is loaded)
        /// </summary>
        public void Initialize()
        {
            LoadLanguages();
        }

        /// <summary>
        /// Loads available language resources
        /// </summary>
        private void LoadLanguages()
        {
            try
            {
                // Clear existing dictionaries
                _loadedLanguages.Clear();

                // Load English (default)
                var englishUri = new Uri("Resources/Languages/en.xaml", UriKind.Relative);
                var englishDict = Application.LoadComponent(englishUri) as ResourceDictionary;
                if (englishDict != null)
                {
                    _loadedLanguages["en"] = englishDict;
                }

                // Load Russian
                var russianUri = new Uri("Resources/Languages/ru.xaml", UriKind.Relative);
                var russianDict = Application.LoadComponent(russianUri) as ResourceDictionary;
                if (russianDict != null)
                {
                    _loadedLanguages["ru"] = russianDict;
                }

                // Load language preference
                var preferredLanguage = LoadLanguagePreference();
                if (!string.IsNullOrEmpty(preferredLanguage) && _loadedLanguages.ContainsKey(preferredLanguage))
                {
                    SetLanguage(preferredLanguage);
                }
                else
                {
                    SetLanguage("en");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback to hardcoded English
                SetLanguage("en");
            }
        }

        /// <summary>
        /// Sets the current application language
        /// </summary>
        /// <param name="languageCode">Language code (en, ru)</param>
        public void SetLanguage(string languageCode)
        {
            if (!_loadedLanguages.ContainsKey(languageCode))
            {
                // Fallback to English if requested language not found
                languageCode = "en";
            }

            if (_currentLanguage == languageCode && _currentResources != null)
                return;

            _currentLanguage = languageCode;
            _currentResources = _loadedLanguages[languageCode];

            // Update application resources
            var appResources = Application.Current.Resources;

            // Remove existing language resources
            var dictionariesToRemove = new List<ResourceDictionary>();
            foreach (ResourceDictionary dict in appResources.MergedDictionaries)
            {
                if (dict.Source?.ToString().Contains("Languages/") == true)
                {
                    dictionariesToRemove.Add(dict);
                }
            }

            foreach (var dict in dictionariesToRemove)
            {
                appResources.MergedDictionaries.Remove(dict);
            }

            // Add new language resources
            appResources.MergedDictionaries.Add(_currentResources);

            // Set culture for formatting
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(languageCode == "ru" ? "ru-RU" : "en-US");
            }
            catch
            {
                culture = CultureInfo.InvariantCulture;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
            catch
            {
                // Ignore if already overridden
            }

            LanguageChanged?.Invoke();
        }

        /// <summary>
        /// Gets the localized string for a key
        /// </summary>
        public string GetString(string key)
        {
            if (_currentResources != null && _currentResources.Contains(key))
            {
                return _currentResources[key] as string ?? $"<{key}>";
            }
            return $"<{key}>";
        }

        /// <summary>
        /// Gets available languages
        /// </summary>
        public List<string> GetAvailableLanguages()
        {
            return new List<string>(_loadedLanguages.Keys);
        }

        /// <summary>
        /// Gets the current language code
        /// </summary>
        public string GetCurrentLanguage()
        {
            return _currentLanguage;
        }

        /// <summary>
        /// Saves language preference to settings
        /// </summary>
        public void SaveLanguagePreference()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appDataPath, "WTSightsEditor");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                var settingsPath = Path.Combine(appFolder, "settings.json");
                var settings = new
                {
                    Language = _currentLanguage,
                    LastUpdated = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                // Silent fail for settings saving
                System.Diagnostics.Debug.WriteLine($"Error saving language preference: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads language preference from settings
        /// </summary>
        public string? LoadLanguagePreference()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var settingsPath = Path.Combine(appDataPath, "WTSightsEditor", "settings.json");

                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<dynamic>(json);
                    return settings?.Language ?? "en";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading language preference: {ex.Message}");
            }

            return "en"; // Default to English
        }
    }
}