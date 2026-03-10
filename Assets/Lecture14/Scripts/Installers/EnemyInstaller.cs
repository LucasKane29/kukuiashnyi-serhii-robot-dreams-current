using Zenject;

public class EnemyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<Zombie>().FromComponentsInHierarchy().AsTransient();
    }
}