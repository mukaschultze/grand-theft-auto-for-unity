using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Shared {
    public partial class Settings {

        private const string SETTINGS_FILENAME = "settings.json";

        private static Settings m_settings = new Settings();
        private static DateTime m_readTime = DateTime.MinValue;

        public static string SettingsFilePath {
            get {
                return Path.Combine(Application.streamingAssetsPath, SETTINGS_FILENAME);
            }
        }

        public static Settings Instance {
            get {
                return LoadSettingsFile();
            }
        }

        private static Settings LoadSettingsFile() {
            using(Timing.IORead())
            if(File.Exists(SettingsFilePath) && m_readTime >= File.GetLastWriteTime(SettingsFilePath))
                return m_settings;

            Log.Message("Reloading settings file");

            if(File.Exists(SettingsFilePath)) {
                using(Timing.IORead()) {
                    var json = File.ReadAllText(SettingsFilePath);
                    JsonUtility.FromJsonOverwrite(json, m_settings);
                }
            } else
                m_settings.SaveSettingsFile();

            m_readTime = DateTime.Now;

            return m_settings;

        }

        public static void ResetAllSettings() {
            var settings = new Settings();
            settings.SaveSettingsFile();
            m_settings = settings;
        }

        public void SaveSettingsFile() {
            using(Timing.IOWrite()) {
                var json = JsonUtility.ToJson(this, true);

                if(!Directory.Exists(Path.GetDirectoryName(SettingsFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));

                if(File.Exists(SettingsFilePath))
                    File.Delete(SettingsFilePath);

                File.WriteAllText(SettingsFilePath, json);
                m_readTime = File.GetLastWriteTime(SettingsFilePath);
                Log.Message("Settings file saved");
            }
        }

    }
}