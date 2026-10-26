using System;
using System.Collections.Generic;
using CKR.Configs;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace CKR.Installers
{
    /// <summary>
    /// Binds every ScriptableObject config. Assign it to the "Scriptable Object Installers" list
    /// of the SceneContext.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigsInstaller", menuName = "CKR/Installers/Configs", order = 0)]
    public sealed class ConfigsInstaller : ScriptableObjectInstaller<ConfigsInstaller>
    {
        [SerializeField] private NetworkConfig _network;
        [SerializeField] private LoggingConfig _logging;
        [SerializeField] private EconomyConfig _economy;
        [SerializeField] private ClickerFeedbackConfig _clickerFeedback;
        [SerializeField] private WeatherConfig _weather;
        [SerializeField] private DogApiConfig _dogApi;

        public override void InstallBindings()
        {
            AssertFullyAssigned();

            Container.BindInterfacesAndSelfTo<NetworkConfig>().FromInstance(_network).AsSingle();
            Container.BindInterfacesAndSelfTo<LoggingConfig>().FromInstance(_logging).AsSingle();

            Container.Bind<EconomyConfig>().FromInstance(_economy).AsSingle();
            Container.Bind<ClickerFeedbackConfig>().FromInstance(_clickerFeedback).AsSingle();
            Container.Bind<WeatherConfig>().FromInstance(_weather).AsSingle();
            Container.Bind<DogApiConfig>().FromInstance(_dogApi).AsSingle();
        }

        private void AssertFullyAssigned()
        {
            var missing = new List<string>();

            Collect(_network, nameof(_network), missing);
            Collect(_logging, nameof(_logging), missing);
            Collect(_economy, nameof(_economy), missing);
            Collect(_clickerFeedback, nameof(_clickerFeedback), missing);
            Collect(_weather, nameof(_weather), missing);
            Collect(_dogApi, nameof(_dogApi), missing);

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"'{name}' has unassigned config fields: {string.Join(", ", missing)}.");
        }

        private void Collect(Object value, string fieldName, List<string> missing)
        {
            if (!value)
                missing.Add(fieldName);
        }
    }
}
