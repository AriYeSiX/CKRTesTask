using CKR.Features.Clicker.Model;
using CKR.Features.Clicker.Services;
using Zenject;

namespace CKR.Installers
{
    /// <summary>
    /// Model layer of the clicker. Lives independently from the UI: the auto collect and the energy
    /// regeneration keep running no matter which tab is currently open.
    /// </summary>
    public sealed class GameplayInstaller : MonoInstaller<GameplayInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Wallet>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnergyModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClickerService>().AsSingle();

            Container.BindInterfacesTo<AutoCollectService>().AsSingle();
            Container.BindInterfacesTo<EnergyRegenService>().AsSingle();
        }
    }
}
