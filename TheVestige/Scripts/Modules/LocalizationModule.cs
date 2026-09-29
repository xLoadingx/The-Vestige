using System;
using System.Collections;
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

    private TextTranslation translation;
    
    private readonly string TextAssetsPath = Path.Combine(
        Instance.ModHelper.Manifest.ModFolderPath,
        "planets",
        "Text Assets"
    );
    
    public void Awake()
    {
        _englishTranslations = LoadTranslation(TextTranslation.Language.ENGLISH);
        _currentTranslations = LoadTranslation(TextTranslation.Get().GetLanguage());
        
        translation = TextTranslation.Get();
        
        RegisterLocalizationFrom(TextAssetsPath);
        
        translation.OnLanguageChanged += () => StartCoroutine(OnLanguageChanged());
    }

    public void OnStarSystemLoaded(string starSystem)
    {
        StartCoroutine(OnLanguageChanged());
    }
    
    public void RegisterNomaiText(XmlDocument document, string sourceName)
    {
        if (translation?.m_table == null)
            return;

        int count = 0;
        
        foreach (XmlNode textBlock in document.SelectNodes("NomaiObject/TextBlock"))
        {
            string id = textBlock.SelectSingleNode("ID").InnerText;
            string translationKey = $"{sourceName}:{id}";

            string text = textBlock.SelectSingleNode("Text").InnerText;

            if (!string.IsNullOrEmpty(text))
            {
                string value = GetTranslation(translationKey, text);
                translation.m_table.Insert(text, value);
                count++;
            }
        }
        
        Log.Info($"Registered {count} Nomai Text translations from {sourceName}");
    }

    public void RegisterShipLog(XmlDocument document, string sourceName)
    {
        if (translation?.m_table == null)
            return;

        int count = 0;
        
        foreach (XmlNode entry in document.SelectNodes("AstroObjectEntry/Entry"))
            count += RegisterShipLogEntry(entry);
        
        Log.Info($"Registered {count} Ship Log translations from {sourceName}");
    }

    public int RegisterShipLogEntry(XmlNode entry)
    {
        int count = 0;
        
        string entryID = entry.SelectSingleNode("ID")?.InnerText;
        string entryName = entry.SelectSingleNode("Name")?.InnerText;

        if (string.IsNullOrEmpty(entryID) || string.IsNullOrEmpty(entryName))
            return 0;

        string translatedName = GetTranslation($"{entryID}:Name", entryName);

        // InsertShipLog refuses to replace existing entries
        // NH already places my translations in there, but I need to
        // replace that.
        translation.m_table.theShipLogTable[entryName] = translatedName;
        count++;

        foreach (XmlNode fact in entry.SelectNodes("ExploreFact"))
        {
            string factID = fact.SelectSingleNode("ID")?.InnerText;
            string factText = fact.SelectSingleNode("Text")?.InnerText;
            string altFactText = fact.SelectSingleNode("AltText/Text")?.InnerText;

            if (string.IsNullOrEmpty(factID))
                continue;
            
            Patches._shipLogFactIDs.Add(factID);

            if (!string.IsNullOrEmpty(factText))
            {
                translation.m_table.theShipLogTable[entryName + factText] = GetTranslation($"{factID}:Text", factText);
                count++;
            }

            if (!string.IsNullOrEmpty(altFactText))
            {
                translation.m_table.theShipLogTable[entryName + altFactText] = GetTranslation($"{factID}:AltText", altFactText);
                count++;
            }
        }

        foreach (XmlNode rumor in entry.SelectNodes("RumorFact"))
        {
            string rumorID = rumor.SelectSingleNode("ID")?.InnerText;
            string rumorText = rumor.SelectSingleNode("Text")?.InnerText;
            string rumorName = rumor.SelectSingleNode("RumorName")?.InnerText;
            string altRumorText = rumor.SelectSingleNode("AltText/Text")?.InnerText;

            if (string.IsNullOrEmpty(rumorID))
                continue;
            
            Patches._shipLogFactIDs.Add(rumorID);

            if (!string.IsNullOrEmpty(rumorText))
            {
                translation.m_table.theShipLogTable[entryName + rumorText] = GetTranslation($"{rumorID}:Text", rumorText);
                count++;
            }

            if (!string.IsNullOrEmpty(altRumorText))
            {
                translation.m_table.theShipLogTable[entryName + altRumorText] = GetTranslation($"{rumorID}:AltText", altRumorText);
                count++;
            }

            if (!string.IsNullOrEmpty(rumorName))
            {
                translation.m_table.theShipLogTable[rumorName] = GetTranslation($"{rumorID}:RumorName", rumorName);
                count++;
            }
        }
        
        foreach (XmlNode childEntry in entry.SelectNodes("Entry"))
            count += RegisterShipLogEntry(childEntry);

        return count;
    }

    public int RegisterDialogue(XmlDocument document, string sourceName)
    {
        int count = 0;

        foreach (XmlNode entry in document.SelectNodes("DialogueTree"))
        {
            string nameField = entry.SelectSingleNode("NameField")?.InnerText;

            if (string.IsNullOrEmpty(nameField))
                continue;

            foreach (XmlNode node in entry.SelectNodes("DialogueNode"))
            {
                string dialogueName = node.SelectSingleNode("Name")?.InnerText;

                // NPC Dialogue
                int pageCount = 0;
                foreach (XmlNode page in node.SelectNodes("Dialogue/Page"))
                {
                    string text = page.InnerText;
                    string key = $"{sourceName}:{dialogueName}:Page:{pageCount}";

                    translation.m_table.theTable[dialogueName + text] = GetTranslation(key, text);

                    pageCount++;
                }

                // Player dialogue options
                int optionCount = 0;
                foreach (XmlNode option in node.SelectNodes("DialogueOptionsList/DialogueOption"))
                {
                    string text = option.SelectSingleNode("Text")?.InnerText;
                    string key = $"{sourceName}:{dialogueName}:Option:{optionCount}";
                    
                    translation.m_table.theTable[nameField + dialogueName + text] = GetTranslation(key, text);

                    optionCount++;
                }
            }
        }

        return count;
    }

    public void RegisterLocalizationFrom(string path)
    {
        foreach (var file in Directory.GetFiles(path, "*.xml"))
        {
            var doc = new XmlDocument();

            try
            {
                doc.Load(file);
            }
            catch (XmlException)
            {
                continue;
            }
            
            RegisterLocalization(doc, Path.GetFileNameWithoutExtension(file));
        }
    }

    public void RegisterLocalizationFrom(AssetBundle bundle)
    {
        foreach (var textAsset in bundle.LoadAllAssets<TextAsset>())
        {
            var doc = new XmlDocument();

            try
            {
                doc.LoadXml(textAsset.text);
            }
            catch (XmlException)
            {
                continue;
            }
            
            RegisterLocalization(doc, textAsset.name);
        }
    }

    public void RegisterLocalization(XmlDocument document, string sourceName)
    {
        switch (document.DocumentElement?.Name)
        {
            case "DialogueTree":
                RegisterDialogue(document, sourceName);
                break;
            
            case "AstroObjectEntry":
                RegisterShipLog(document, sourceName);
                break;
            
            case "NomaiObject":
                RegisterNomaiText(document, sourceName);
                break;
        }
    }

    private Dictionary<string, string> LoadTranslation(TextTranslation.Language language)
    {
        string fileName = language switch
        {
            TextTranslation.Language.ENGLISH => "english.json",
            TextTranslation.Language.SPANISH_LA => "spanish.json",
            _ => null
        };

        if (fileName == null)
        {
            Log.Error($"Unknown or untranslated language: {language.ToString()} | Defaulting to English", false);
            fileName = "english.json";
        }

        // Frickin UI popup on the first session of the mod
        // requires me to use an ENITRE DIFFERENT folder
        // for its localization qwp
        string path = Path.Combine(
            Instance.ModHelper.Manifest.ModFolderPath,
            "translations (actual ones)",
            fileName
        );

        if (!File.Exists(path))
        {
            Log.Error($"Translation file for language {language.ToString()} not found.", false);
            return new();
        }

        var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            File.ReadAllText(path)
        ) ?? new();
        
        return json;
    }

    public string GetTranslation(string key, string fallback)
    {
        if (_currentTranslations.TryGetValue(key, out var translated))
            return translated;

        if (_englishTranslations.TryGetValue(key, out translated))
            return translated;
        
        return fallback;
    }

    private IEnumerator OnLanguageChanged()
    {
        // For NH language change
        yield return null;
        
        Log.Info("Loaded new Language");
        
        _currentTranslations.Clear();

        _currentTranslations = LoadTranslation(TextTranslation.Get().GetLanguage());

        foreach (var bundle in bundleModule.loadedBundles.Values)
            RegisterLocalizationFrom(bundle: bundle);
        
        RegisterLocalizationFrom(TextAssetsPath);
    }
}