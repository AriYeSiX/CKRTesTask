using System;
using System.IO;
using CKR.Configs;
using CKR.Installers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CKR.EditorTools
{
    /// <summary>
    /// Creates every config asset with its default values and wires them into a ConfigsInstaller.
    /// Saves the manual "create 6 assets and drag them into place" step when opening the project.
    /// </summary>
    public static class CkrConfigsCreator
    {
        private const string ConfigsFolder = "Assets/Configs";

        [MenuItem("CKR/Create Default Configs", false, 0)]
        public static void CreateDefaultConfigs()
        {
            EnsureConfigs();
        }

        /// <summary>
        /// Creates the config assets if they do not exist yet and returns the installer that
        /// binds them. Safe to call repeatedly — existing assets keep their tuned values.
        /// </summary>
        public static ConfigsInstaller EnsureConfigs()
        {
            EnsureFolder(ConfigsFolder);

            var network = CreateOrLoad<NetworkConfig>("NetworkConfig");
            var logging = CreateOrLoad<LoggingConfig>("LoggingConfig");
            var economy = CreateOrLoad<EconomyConfig>("EconomyConfig");
            var feedback = CreateOrLoad<ClickerFeedbackConfig>("ClickerFeedbackConfig");
            var weather = CreateOrLoad<WeatherConfig>("WeatherConfig");
            var dogApi = CreateOrLoad<DogApiConfig>("DogApiConfig");
            var installer = CreateOrLoad<ConfigsInstaller>("ConfigsInstaller");

            var serialized = new SerializedObject(installer);
            Assign(serialized, "_network", network);
            Assign(serialized, "_logging", logging);
            Assign(serialized, "_economy", economy);
            Assign(serialized, "_clickerFeedback", feedback);
            Assign(serialized, "_weather", weather);
            Assign(serialized, "_dogApi", dogApi);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(installer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return installer;
        }

        private static T CreateOrLoad<T>(string assetName) where T : ScriptableObject
        {
            var path = $"{ConfigsFolder}/{assetName}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Assign(SerializedObject serialized, string fieldName, Object value)
        {
            var property = serialized.FindProperty(fieldName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on {serialized.targetObject.name}.");
            }

            property.objectReferenceValue = value;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            var leaf = Path.GetFileName(folder);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
                return;

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
