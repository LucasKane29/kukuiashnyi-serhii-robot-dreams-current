using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField]
    private TextMeshProUGUI scoreText, headshotsText, shotsText;
    [SerializeField]
    private MainMenu mainMenu;

    [SerializeField]
    private RectTransform _playerHealthbarLevel;

    [SerializeField]
    private float _animationSpeed = 0.3f;

    [SerializeField]
    private EventBus eventBus;

    private float initialValue = 0f;

    private float maxHealthBarWidth;

    private Coroutine currentAnimation;

    public void Awake()
    {
        maxHealthBarWidth = _playerHealthbarLevel.sizeDelta.x;
    }

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
        eventBus.Subscribe<ShowedMenuEvent>(OnShowMainMenu);
        eventBus.Subscribe<ClosedMenuEvent>(OnCloseMainMenu);
    }

    public void OnDisable()
    {
        eventBus.Unsubscribe<UpdatedScoreEvent>(OnScoreUpdated);
        eventBus.Unsubscribe<UpdatedShotsEvent>(OnShotsUpdated);
        eventBus.Unsubscribe<UpdatedHeadshotsEvent>(OnHeadshotsUpdated);
        eventBus.Unsubscribe<ClosedMenuEvent>(OnCloseMainMenu);
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

    void OnShowMainMenu(ShowedMenuEvent subscribedEvent)
    {
        mainMenu.ShowMainMenu();
    }

    void OnCloseMainMenu(ClosedMenuEvent subscribedEvent)
    {
        mainMenu.HideMainMenu();
    }

    public void UpdatePlayerHealthBar(float current, float max)
    {
        Debug.Log($"Health changed: {current} / {max}");

        float healthPercentage = current / max;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateHealthBar(maxHealthBarWidth * healthPercentage));
    }

    private IEnumerator AnimateHealthBar(float targetWidth)
    {
        float initialWidth = _playerHealthbarLevel.sizeDelta.x;
        float elapsedTime = 0f;
        while (elapsedTime < _animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float newWidth = Mathf.Lerp(initialWidth, targetWidth, elapsedTime / _animationSpeed);
            _playerHealthbarLevel.sizeDelta = new Vector2(newWidth, _playerHealthbarLevel.sizeDelta.y);
            yield return null;
        }
        _playerHealthbarLevel.sizeDelta = new Vector2(targetWidth, _playerHealthbarLevel.sizeDelta.y);
    }

}
