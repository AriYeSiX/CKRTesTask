using CKR.Features.Clicker;
using CKR.Features.Clicker.View;
using CKR.Features.Dogs;
using CKR.Features.Dogs.View;
using CKR.Features.Navigation;
using CKR.Features.Popup;
using CKR.Features.Weather;
using CKR.Features.Weather.View;
using UnityEngine;
using Zenject;

namespace CKR.Installers
{
    /// <summary>
    /// Views, pools and presenters.
    ///
    /// Views are scene instances bound by reference, presenters are plain C# classes created by
    /// the container, so no view ever resolves a dependency itself.
    /// </summary>
    public sealed class UiInstaller : MonoInstaller<UiInstaller>
    {
        [Header("Navigation")]
        [SerializeField] private TabBarView _tabBar;

        [Header("Screens")]
        [SerializeField] private ClickerView _clickerView;
        [SerializeField] private WeatherView _weatherView;
        [SerializeField] private DogsView _dogsView;

        [Header("Popup")]
        [SerializeField] private InfoPopupView _popupView;

        [Header("Pools")]
        [SerializeField] private FlyingCurrencyView _flyingCurrencyPrefab;
        [SerializeField] private RectTransform _flyingCurrencyRoot;
        [SerializeField, Min(0)] private int _flyingCurrencyPoolSize = 12;

        [SerializeField] private ClickVfxView _clickVfxPrefab;
        [SerializeField] private RectTransform _clickVfxRoot;
        [SerializeField, Min(0)] private int _clickVfxPoolSize = 6;

        [SerializeField] private BreedRowView _breedRowPrefab;
        [SerializeField] private RectTransform _breedRowsRoot;
        [SerializeField, Min(0)] private int _breedRowPoolSize = 10;

        public override void InstallBindings()
        {
            BindViews();
            BindPools();
            BindPresenters();
        }

        private void BindViews()
        {
            Container.Bind<TabBarView>().FromInstance(_tabBar).AsSingle();
            Container.Bind<ClickerView>().FromInstance(_clickerView).AsSingle();
            Container.Bind<WeatherView>().FromInstance(_weatherView).AsSingle();
            Container.Bind<DogsView>().FromInstance(_dogsView).AsSingle();

            Container.Bind<InfoPopupView>().FromInstance(_popupView).AsSingle();
            Container.Bind<IPopupService>().To<PopupService>().AsSingle();
        }

        private void BindPools()
        {
            Container
                .BindMemoryPool<FlyingCurrencyView, FlyingCurrencyView.Pool>()
                .WithInitialSize(_flyingCurrencyPoolSize)
                .FromComponentInNewPrefab(_flyingCurrencyPrefab)
                .UnderTransform(_flyingCurrencyRoot);

            Container
                .BindMemoryPool<ClickVfxView, ClickVfxView.Pool>()
                .WithInitialSize(_clickVfxPoolSize)
                .FromComponentInNewPrefab(_clickVfxPrefab)
                .UnderTransform(_clickVfxRoot);

            Container
                .BindMemoryPool<BreedRowView, BreedRowView.Pool>()
                .WithInitialSize(_breedRowPoolSize)
                .FromComponentInNewPrefab(_breedRowPrefab)
                .UnderTransform(_breedRowsRoot);
        }

        private void BindPresenters()
        {
            Container.BindInterfacesTo<TabBarPresenter>().AsSingle();
            Container.BindInterfacesTo<ClickerPresenter>().AsSingle();
            Container.BindInterfacesTo<WeatherPresenter>().AsSingle();
            Container.BindInterfacesTo<DogsPresenter>().AsSingle();
        }
    }
}
