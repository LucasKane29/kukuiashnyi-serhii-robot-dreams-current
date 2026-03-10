using Zenject;

public class HealthbarManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.DeclareSignal<HealthChangedSignal>();
        Container.BindInterfacesAndSelfTo<HealhbarManager>().FromComponentsInHierarchy().AsSingle();
    }
}