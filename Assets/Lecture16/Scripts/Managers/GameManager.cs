
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
    private string _pauseActionName = "Pause", _inventoryActionName = "Inventory", _interactionActionName = "Interact";

    private GameState _currentState = GameState.Playing;

    private InputAction _pauseAction, _inventoryAction, _interactionAction;

    private UIManager _uiManager;
    private PlayerMoveController _playerMoveController;
    private PlayerFireController _playerFireController;
    private MerchantController _merchantController;

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
        }
    }

    public void Start()
    {
        _pauseAction = InputSystem.actions.FindAction(_pauseActionName);
        _inventoryAction = InputSystem.actions.FindAction(_inventoryActionName);
        _interactionAction = InputSystem.actions.FindAction(_interactionActionName);

        if (_isGameScene)
            EnterState(GameState.Playing);
        else
            Time.timeScale = 1f;
    }

    public void Update()
    {
        if (!_isGameScene) return;

        switch(_currentState)
        {
            case GameState.Playing:
                if (_pauseAction != null && _pauseAction.triggered)
                    ChangeState(GameState.Paused);
                else if (_inventoryAction != null && _inventoryAction.triggered)
                    ChangeState(GameState.Inventory);
                else if (_interactionAction != null && _interactionAction.triggered && _isCanInteract)
                    ChangeState(GameState.Shopping);
                break;
            case GameState.Paused:
                if (_pauseAction != null && _pauseAction.triggered)
                    ChangeState(GameState.Playing);
                break;
            case GameState.Inventory:
                if ((_inventoryAction != null && _inventoryAction.triggered) ||
                    (_pauseAction != null && _pauseAction.triggered))
                    ChangeState(GameState.Playing);
                break;
            case GameState.Shopping:
                if (_pauseAction != null && _pauseAction.triggered)
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
                _playerFireController.OnGamePaused(false);
                _playerMoveController.OnGamePaused(false);
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                _uiManager.ShowMainMenu();
                _playerFireController.OnGamePaused(true);
                _playerMoveController.OnGamePaused(true);
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
}
