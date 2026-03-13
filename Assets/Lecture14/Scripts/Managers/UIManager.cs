using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText, headshotsText, shotsText;
    [SerializeField]
    private EventBus eventBus;

    private float initialValue = 0f;

    void Start()
    {
        scoreText.text = $"Score: {initialValue}";
        headshotsText.text = $"Headshots: {initialValue}";
        shotsText.text = $"Shots: {initialValue}";
    }

    public void OnEnable()
    {
        eventBus.Subscribe<UpdatedScoreEvent>(OnScoreUpdated);
        eventBus.Subscribe<UpdatedShotsEvent>(OnShotsUpdated);
        eventBus.Subscribe<UpdatedHeadshotsEvent>(OnHeadshotsUpdated);
    }

    public void OnDisable()
    {
        eventBus.Unsubscribe<UpdatedScoreEvent>(OnScoreUpdated);
        eventBus.Unsubscribe<UpdatedShotsEvent>(OnShotsUpdated);
        eventBus.Unsubscribe<UpdatedHeadshotsEvent>(OnHeadshotsUpdated);
    }

    void OnScoreUpdated(UpdatedScoreEvent subscribedEvent)
    {
        scoreText.text = $"Score: {subscribedEvent.currentScore}";
    }

    void OnShotsUpdated(UpdatedShotsEvent subscribedEvent)
    {
        shotsText.text = $"Shots: {subscribedEvent.currentValue}";
    }

    void OnHeadshotsUpdated(UpdatedHeadshotsEvent subscribedEvent)
    {
        headshotsText.text = $"Headshots: {subscribedEvent.currentValue}";
    }
}
