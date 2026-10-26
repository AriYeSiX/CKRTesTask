using CKR.Core.Json;
using CKR.Core.Logging;
using CKR.Core.Net;
using CKR.Core.Requests;
using CKR.Features.Navigation;
using CKR.Infrastructure.Audio;
using CKR.Infrastructure.Ui;
using UnityEngine;
using Zenject;

namespace CKR.Installers
{
    /// <summary>
    /// Application wide services: transport, the request queue, navigation, audio and the sprite
    /// cache. All of them are container scoped and resolved through constructor injection.
    /// </summary>
    public sealed class CoreInstaller : MonoInstaller<CoreInstaller>
    {
        [SerializeField] private AudioService _audioService;

        public override void InstallBindings()
        {
            Container.Bind<ILogService>().To<UnityLogService>().AsSingle();
            Container.Bind<IJsonSerializer>().To<UnityJsonSerializer>().AsSingle();
            Container.Bind<IWebClient>().To<UnityWebClient>().AsSingle();

            // IInitializable starts the pump, IDisposable drains the queue on shutdown.
            Container.BindInterfacesAndSelfTo<SequentialRequestQueue>().AsSingle();

            Container.BindInterfacesAndSelfTo<TabNavigator>().AsSingle();

            // Owns the textures created from downloaded images and destroys them on shutdown.
            Container.BindInterfacesAndSelfTo<SpriteCache>().AsSingle();

            if (_audioService != null)
                Container.Bind<IAudioService>().To<AudioService>().FromInstance(_audioService).AsSingle();
            else
                Container.Bind<IAudioService>().To<AudioService>().FromNewComponentOnNewGameObject()
                    .WithGameObjectName("[AudioService]").AsSingle().NonLazy();
        }
    }
}
