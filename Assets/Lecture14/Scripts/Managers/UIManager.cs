using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using System;

public class UIManager : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField]
    private TextMeshProUGUI scoreText, headshotsText, shotsText;

    private SignalBus signalBus;

    private float initialValue = 0f;

    void Start()
    {
        scoreText.text = $"Score: {initialValue}";
        headshotsText.text = $"Headshots: {initialValue}";
        shotsText.text = $"Shots: {initialValue}";
    }

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    public void Initialize()
    {
        signalBus.Subscribe<UpdatedScoreSignal>(OnScoreUpdated);
        signalBus.Subscribe<UpdatedShotsSignal>(OnShotsUpdated);
        signalBus.Subscribe<UpdatedHeadshotsSignal>(OnHeadshotsUpdated);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<UpdatedScoreSignal>(OnScoreUpdated);
        signalBus.Unsubscribe<UpdatedShotsSignal>(OnShotsUpdated);
        signalBus.Unsubscribe<UpdatedHeadshotsSignal>(OnHeadshotsUpdated);
    }

    void OnScoreUpdated(UpdatedScoreSignal signal)
    {
        scoreText.text = $"Score: {signal.currentScore}";
    }

    void OnShotsUpdated(UpdatedShotsSignal signal)
    {
        shotsText.text = $"Shots: {signal.currentValue}";
    }

    void OnHeadshotsUpdated(UpdatedHeadshotsSignal signal)
    {
        headshotsText.text = $"Headshots: {signal.currentValue}";
    }
}
