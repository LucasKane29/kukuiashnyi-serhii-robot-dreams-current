using System;
using UnityEngine;

public class ScoreManager: MonoBehaviour
{
    [SerializeField]
    private EventBus eventBus;

    public float score { get; private set; }
    public float headshots { get; private set; }
    public float shots { get; private set; }

    public void AddScore(float value)
    {
        score += value;
        RedrawScore();
    }

    public void Awake()
    {
        score = 0;
        headshots = 0;
        shots = 0;
        RedrawScore();
    }

    public void OnEnable()
    {
        eventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        eventBus.Subscribe<ShotMadeEvent>(OnShotMade);
        eventBus.Subscribe<HeadshotMadeEvent>(OnHeadshotMade);
    }

    public void OnDisable()
    {
        eventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        eventBus.Unsubscribe<ShotMadeEvent>(OnShotMade);
        eventBus.Unsubscribe<HeadshotMadeEvent>(OnHeadshotMade);
    }

    void OnEnemyKilled(EnemyKilledEvent subscribedEvent)
    {
        AddScore(subscribedEvent.score);
    }

    void OnShotMade(ShotMadeEvent subscribedEvent)
    {
        shots += 1;
        RedrawShots();
        
    }

    void OnHeadshotMade(HeadshotMadeEvent subscribedEvent)
    {
        headshots += 1;
        RedrawHeadshots();
    }

    void RedrawScore()
    {
        eventBus.Publish(new UpdatedScoreEvent(score));
    }

    void RedrawShots()
    {
        eventBus.Publish(new UpdatedShotsEvent(shots));
    }

    void RedrawHeadshots()
    {
        eventBus.Publish(new UpdatedHeadshotsEvent(headshots));
    }
}
