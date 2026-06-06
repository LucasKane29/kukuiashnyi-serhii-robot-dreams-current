
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using System.Collections;

public enum GameState
{
    Playing,
    Paused,
    Inventory,
    Shopping,
    GameOver
}

public class GameManager : MonoBehaviour, IService
{
    [FormerlySerializedAs("_isNotLobbyScene")]
    [SerializeField]
    private bool _isGameScene = false;

    [SerializeField]
    private float _winScore = 10f;

    [SerializeField]
    private InputActionReference _pauseActionRef;      // LeftHand / MenuButton
    [SerializeField]
    private InputActionReference _inventoryActionRef;  // LeftHand / SecondaryButton (Y)
    [SerializeField]
    private InputActionReference _interactionActionRef; // LeftHand / PrimaryButton (X)

    private GameState _currentState = GameState.Playing;

    private UIManager _uiManager;
    private PlayerMoveController _playerMoveController;
    private PlayerFireController _playerFireController;
    private MerchantController _merchantController;
    private AudioManager _audioManager;

    [SerializeField]
    private string _lobbySceneName;

    [SerializeField]
    private string _gameSceneName;

    private bool _isCanInteract = false;

    private void Awake()
    {
        if (_isGameScene)
        {
            _uiManager = IServiceLocator.Instance.GetService<UIManager>();
            _playerMoveController = IServiceLocator.Instance.GetService<PlayerMoveController>();
            _playerFireController = IServiceLocator.Instance.GetService<PlayerFireController>();
            _merchantController = IServiceLocator.Instance.GetService<MerchantController>();
            _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
        }
    }

    public void Start()
    {
        _pauseActionRef?.action.Enable();
        _inventoryActionRef?.action.Enable();
        _interactionActionRef?.action.Enable();

        if (_isGameScene)
            EnterState(GameState.Playing);
        else
            Time.timeScale = 1f;
    }

    public void Update()
    {
        if (!_isGameScene) return;

        bool pausePressed     = _pauseActionRef != null     && _pauseActionRef.action.WasPressedThisFrame();
        bool inventoryPressed = _inventoryActionRef != null && _inventoryActionRef.action.WasPressedThisFrame();
        bool interactPressed  = _interactionActionRef != null && _interactionActionRef.action.WasPressedThisFrame();

        switch(_currentState)
        {
            case GameState.Playing:
                if (pausePressed)
                    ChangeState(GameState.Paused);
                else if (inventoryPressed)
                    ChangeState(GameState.Inventory);
                else if (interactPressed && _isCanInteract)
                    ChangeState(GameState.Shopping);
                break;
            case GameState.Paused:
                if (pausePressed)
                    ChangeState(GameState.Playing);
                break;
            case GameState.Inventory:
                if (inventoryPressed || pausePressed)
                    ChangeState(GameState.Playing);
                break;
            case GameState.Shopping:
                if (pausePressed)
                    ChangeState(GameState.Playing);
                break;
        }
    }

    private void ChangeState(GameState state)
    {
        ExitState(_currentState);
        _currentState = state;
        EnterState(_currentState);
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                _uiManager.HideSettingsMenu();
                _playerFireController.OnGamePaused(false);
                _playerMoveController.OnGamePaused(false);
                _audioManager.ResumeAll();
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                _uiManager.ShowMainMenu();
                _playerFireController.OnGamePaused(true);
                _playerMoveController.OnGamePaused(true);
                _audioManager.PauseAll();
                break;
            case GameState.Inventory:
                Time.timeScale = 0f;
                _uiManager.ToggleInventory();
                _playerFireController.OnGamePaused(true);
                _playerMoveController.OnGamePaused(true);
                break;
            case GameState.Shopping:
                Time.timeScale = 0f;
                _merchantController.OpenShop();
                _playerFireController.OnGamePaused(true);
                _playerMoveController.OnGamePaused(true);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                _playerFireController.OnGamePaused(true);
                _playerMoveController.OnGamePaused(true);
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                _uiManager.HideMainMenu();
                break;
            case GameState.Inventory:
                _uiManager.ToggleInventory();
                break;
            case GameState.Shopping:
                _merchantController.CloseShop();
                break;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(_gameSceneName);
    }

    public void ToLobby()
    {
        SceneManager.LoadSceneAsync(_lobbySceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnPlayerDeath()
    {
        ChangeState(GameState.GameOver);
         _uiManager.ShowDeathScreen();
    }

    public void OnPlayerWin()
    {
        ChangeState(GameState.GameOver);
        _uiManager.ShowWinScreen();
    }

    public float GetWinScore()
    {
        return _winScore;
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Playing);
    }

    public void SetCanInteract(bool canInteract)
    {
        _isCanInteract = canInteract;
    }

    public void ReloadScene()
    {                                                                                                                                                                                                                               //    бо після LoadScene GetActiveScene() може повернути вже нову сцену
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(sceneName);
    }
}
