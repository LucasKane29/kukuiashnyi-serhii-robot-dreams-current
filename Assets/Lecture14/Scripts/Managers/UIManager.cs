using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField]
    private EventBus _eventBus;

    [SerializeField]
    private TextMeshProUGUI _scoreText, _headshotsText, _shotsText, _winScoreText;

    [SerializeField]
    private MainMenu _mainMenu;

    [SerializeField]
    private RectTransform _playerHealthbarLevel;

    [SerializeField]
    private float _animationSpeed = 0.3f;

    [SerializeField]
    private Transform _playerUI;

    [SerializeField]
    private Transform _inventoryUI;

    [SerializeField]
    private Transform _itemsContainer;

    [SerializeField]
    private InventoryItem _itemPrefub;

    private float _maxHealthBarWidth;

    private Coroutine _currentAnimation;

    private GameManager _gameManager;

    private LocalizationManager _localizationManager;
    private ScoreManager _scoreManager;

    private List<InventoryItem> _itemsUI = new List<InventoryItem>();

    public void OnEnable()
    {
        if (_eventBus != null)
            _eventBus.Subscribe<UpdateInventoryEvent>(UpdateInventoryUI);
    }

    public void OnDisable()
    {
        if (_eventBus != null)
            _eventBus.Unsubscribe<UpdateInventoryEvent>(UpdateInventoryUI);
    }

    public void Awake()
    {
        _maxHealthBarWidth = _playerHealthbarLevel.sizeDelta.x;
        _gameManager = IServiceLocator.Instance?.GetService<GameManager>();
        _localizationManager = IServiceLocator.Instance?.GetService<LocalizationManager>();
        _localizationManager.OnLanguageChanged += UpdateStatisticLocalization;
        _scoreManager = IServiceLocator.Instance?.GetService<ScoreManager>();
    }

    void Start()
    {
        UpdateStatisticLocalization();
    }

    public void UpdateScore(float currentScore)
    {
        if (_scoreText != null)
            _scoreText.text = _localizationManager.GetLocalizedValue("ScoreText", currentScore);
    }

    public void UpdateShots(int currentShots)
    {
        if (_shotsText != null)
            _shotsText.text = _localizationManager.GetLocalizedValue("ShotsText", currentShots);
    }

    public void UpdateHeadshots(int currentHeadshots)
    {
        if (_headshotsText != null)
            _headshotsText.text = _localizationManager.GetLocalizedValue("HeadshotsText", currentHeadshots);
    }

    public void ShowMainMenu()
    {
        if (_mainMenu != null)
            _mainMenu.ShowMainMenu();
    }

    public void HideMainMenu()
    {
        if (_mainMenu != null)
        {
            _mainMenu.HideMainMenu();
        }   
    }

    public void HideSettingsMenu()
    {
        if (_mainMenu != null)
        {
            _mainMenu.HideSettingsMenu();
        }
    }

    public void UpdatePlayerHealthBar(float current, float max)
    {
        Debug.Log($"Health changed: {current} / {max}");

        float healthPercentage = current / max;

        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(AnimateHealthBar(_maxHealthBarWidth * healthPercentage));
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
        _mainMenu.ShowDeathScreen();
        _playerUI.gameObject.SetActive(false);
    }

    public void ShowWinScreen()
    {
        _mainMenu.ShowWinScreen();
        _playerUI.gameObject.SetActive(false);
    }

    public void ToggleInventory()
    {
        bool isActive = _inventoryUI.gameObject.activeSelf;
        _inventoryUI.gameObject.SetActive(!isActive);
    }

    public void UpdateInventoryUI(UpdateInventoryEvent subscribedEvent)
    {
        var items = new List<KeyValuePair<ItemData, int>>(subscribedEvent.Items);

        for (int i = items.Count; i < _itemsUI.Count; i++)
        {
            _itemsUI[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (i < _itemsUI.Count)
            {
                _itemsUI[i].gameObject.SetActive(true);
                _itemsUI[i].SetItem(items[i].Key, items[i].Value);
            }
            else
            {
                InventoryItem newItemUI = Instantiate(_itemPrefub, _itemsContainer);
                newItemUI.SetItem(items[i].Key, items[i].Value);
                _itemsUI.Add(newItemUI);
            }
        }
    }

    void OnDestroy()
    {
        if (_localizationManager != null)
            _localizationManager.OnLanguageChanged -= Start;
    }

    void UpdateStatisticLocalization()
    {
        if (_scoreText != null)
            _scoreText.text = _localizationManager.GetLocalizedValue("ScoreText", _scoreManager._score);
        if (_headshotsText != null)
            _headshotsText.text = _localizationManager.GetLocalizedValue("HeadshotsText", _scoreManager._headshots);
        if (_shotsText != null)
            _shotsText.text = _localizationManager.GetLocalizedValue("ShotsText", _scoreManager._shots);
        if (_gameManager != null && _winScoreText != null)
            _winScoreText.text = _localizationManager.GetLocalizedValue("WinText", _gameManager.GetWinScore());
    }
}
