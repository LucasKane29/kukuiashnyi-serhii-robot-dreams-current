using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField]
    private TextMeshProUGUI scoreText, headshotsText, shotsText, winScoreText;

    [SerializeField]
    private MainMenu mainMenu;

    [SerializeField]
    private RectTransform _playerHealthbarLevel;

    [SerializeField]
    private float _animationSpeed = 0.3f;

    [SerializeField]
    private Canvas _playerUI;

    private float initialValue = 0f;

    private float maxHealthBarWidth;

    private Coroutine currentAnimation;

    private GameManager _gameManager;

    public void Awake()
    {
        maxHealthBarWidth = _playerHealthbarLevel.sizeDelta.x;
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
    }

    void Start()
    {
        scoreText.text = $"Score: {initialValue}";
        headshotsText.text = $"Headshots: {initialValue}";
        shotsText.text = $"Shots: {initialValue}";
        winScoreText.text = $"Get {_gameManager.GetWinScore()} score to win!";
    }

    public void UpdateScore(float currentScore)
    {
        scoreText.text = $"Score: {currentScore}";
    }

    public void UpdateShots(int currentShots)
    {
        shotsText.text = $"Shots: {currentShots}";
    }
    public void UpdateHeadshots(int currentHeadshots)
    {
        headshotsText.text = $"Headshots: {currentHeadshots}";
    }

    public void ShowMainMenu()
    {
        mainMenu.ShowMainMenu();
    }

    public void HideMainMenu()
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

    public void ShowDeathScreen()
    {
        mainMenu.ShowDeathScreen();
        _playerUI.gameObject.SetActive(false);
    }

    public void ShowWinScreen()
    {
        mainMenu.ShowWinScreen();
        _playerUI.gameObject.SetActive(false);
    }
}
