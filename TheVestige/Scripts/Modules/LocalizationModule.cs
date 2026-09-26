using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts.Modules;

public class LocalizationModule : MonoBehaviour
{
    private Dictionary<string, string> _englishTranslations = new();
    private Dictionary<string, string> _currentTranslations = new();
    
    public void Awake()
    {
        _englishTranslations = LoadTranslation(TextTranslation.Language.ENGLISH);
        _currentTranslations = LoadTranslation(TextTranslation.Get().GetLanguage());
        
        TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
    }   
    
    public void RegisterNomaiText(AssetBundle bundle)
    {
        var translation = TextTranslation.Get();
        if (translation == null || translation.m_table == null)
            return;

        foreach (var textAsset in bundle.LoadAllAssets<TextAsset>())
        {
            string contents = textAsset.text;

            if (string.IsNullOrWhiteSpace(contents))
                continue;

            var document = new XmlDocument();

            try
            {
                document.LoadXml(contents);
            }
            catch (XmlException)
            {
                continue;
            }

            if (document.DocumentElement?.Name != "NomaiObject")
                continue;
            
            foreach (XmlNode textBlock in document.SelectNodes("NomaiObject/TextBlock"))
            {
                string id = textBlock.SelectSingleNode("ID").InnerText;
                string translationKey = $"{textAsset.name}:{id}";

                string text = textBlock.SelectSingleNode("Text").InnerText;

                if (!string.IsNullOrEmpty(text))
                {
                    string value = GetTranslation(translationKey, text);
                    translation.m_table.Insert(text, value);
                }
            }
        }
    }

    private Dictionary<string, string> LoadTranslation(TextTranslation.Language language)
    {
        string fileName = language switch
        {
            TextTranslation.Language.ENGLISH => "english.json",
            _ => null
        };

        if (fileName == null)
        {
            Log.Error($"Unknown or untranslated language: {language.ToString()} | Defaulting to English", false);
            fileName = "english.json";
        }

        string path = Path.Combine(
            Instance.ModHelper.Manifest.ModFolderPath,
            "translations",
            fileName
        );

        if (!File.Exists(path))
        {
            Log.Error($"Translation file for language {language.ToString()} not found.", false);
            return null;
        }

        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            File.ReadAllText(path)
        ) ?? new();
        
        return json;
    }

    private string GetTranslation(string key, string fallback)
    {
        if (_currentTranslations.TryGetValue(key, out var translation))
            return translation;

        if (_englishTranslations.TryGetValue(key, out translation))
            return translation;
        
        return fallback;
    }

    private void OnLanguageChanged()
    {
        _currentTranslations.Clear();

        _currentTranslations = LoadTranslation(TextTranslation.Get().GetLanguage());

        foreach (var bundle in bundleModule.loadedBundles.Values)
            RegisterNomaiText(bundle);
    }
}