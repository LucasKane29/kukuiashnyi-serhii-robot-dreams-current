using UnityEngine;
using Zenject;

public class UIManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.DeclareSignal<UpdatedScoreSignal>();
        Container.DeclareSignal<UpdatedShotsSignal>();
        Container.DeclareSignal<UpdatedHeadshotsSignal>();
        Container.BindInterfacesAndSelfTo<UIManager>().FromComponentsInHierarchy().AsSingle();
    }
}