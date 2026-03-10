using System;
using UnityEngine;
using Zenject;

public class GameManager : IInitializable, IDisposable
{
    private SignalBus signalBus;

    public float score { get; private set; }
    public float headshots { get; private set; }
    public float shots { get; private set; }

    public GameManager(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public void AddScore(float value)
    {
        score += value;
        Debug.Log($"Score: {score}");
        RedrawScore();
    }

    public void StartGame()
    {
        score = 0;
        headshots = 0;
        shots = 0;
        Debug.Log("Game Started");
        RedrawScore();
    }

    public void Initialize()
    {
        signalBus.Subscribe<EnemyKilledSignal>(OnEnemyKilled);
        signalBus.Subscribe<ShotMadeSignal>(OnShotMade);
        signalBus.Subscribe<HeadshotMadeSignal>(OnHeadshotMade);
        StartGame();
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<EnemyKilledSignal>(OnEnemyKilled);
        signalBus.Unsubscribe<ShotMadeSignal>(OnShotMade);
        signalBus.Unsubscribe<HeadshotMadeSignal>(OnHeadshotMade);
    }

    void OnEnemyKilled(EnemyKilledSignal signal)
    {
        AddScore(signal.score);
    }

    void OnShotMade(ShotMadeSignal signal)
    {
        shots += 1;
        RedrawShots();
        
    }

    void OnHeadshotMade(HeadshotMadeSignal signal)
    {
        headshots += 1;
        RedrawHeadshots();
    }

    void RedrawScore()
    {
        signalBus.Fire(new UpdatedScoreSignal(score));
    }

    void RedrawShots()
    {
        signalBus.Fire(new UpdatedShotsSignal(shots));
    }

    void RedrawHeadshots()
    {
        signalBus.Fire(new UpdatedHeadshotsSignal(headshots));
    }
}
