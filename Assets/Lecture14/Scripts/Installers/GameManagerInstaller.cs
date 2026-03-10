using UnityEngine;
using Zenject;

public class GameManagerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.DeclareSignal<EnemyKilledSignal>();
        Container.DeclareSignal<ShotMadeSignal>();
        Container.DeclareSignal<HeadshotMadeSignal>();
        Container.BindInterfacesAndSelfTo<GameManager>().AsSingle();
        Container.DeclareSignal<UpdatedScoreSignal>().OptionalSubscriber();
    }
}