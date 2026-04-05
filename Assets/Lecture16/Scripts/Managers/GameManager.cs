using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IService
{
    [SerializeField]
    private bool _isNotLobbyScene = false;

    [SerializeField]
    private float _winScore = 10f;

    [SerializeField]
    private string pauseActionName = "Pause";

    private bool isGamePaused = false;
    private InputAction pauseAction;

    private UIManager _uiManager;
    private PlayerMoveController _playerMoveController;
    private PlayerFireController _playerFireController;

    [SerializeField]
    private string _lobbySceneName;

    [SerializeField]
    private string _gameSceneName;

    private bool _isCanPause = true;

    private void Awake()
    {
        if(_isNotLobbyScene) {
            _uiManager = IServiceLocator.Instance.GetService<UIManager>();
            _playerMoveController = IServiceLocator.Instance.GetService<PlayerMoveController>();
            _playerFireController = IServiceLocator.Instance.GetService<PlayerFireController>();
        }
    }


    public void Start()
    {
        pauseAction = InputSystem.actions.FindAction(pauseActionName);
        if(_isNotLobbyScene) 
        {
            _playerFireController.OnGamePaused(isGamePaused);
            _playerMoveController.OnGamePaused(isGamePaused);
        }

        Time.timeScale = 1f;
    }

    public void Update()
    {
        if(pauseAction != null) {
            if (pauseAction.triggered && _isNotLobbyScene && _isCanPause)
            {
                SwitchPauseGame();
            }
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

    public void SwitchPauseGame()
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            _uiManager.ShowMainMenu();
        }
        else
        {
            isGamePaused = false;
            Time.timeScale = 1f;
            _uiManager.HideMainMenu();
        }
        _playerFireController.OnGamePaused(isGamePaused);
        _playerMoveController.OnGamePaused(isGamePaused);
    }

    public void OnPlayerDeath()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        _playerFireController.OnGamePaused(isGamePaused);
        _playerMoveController.OnGamePaused(isGamePaused);
        _uiManager.ShowDeathScreen();
        _isCanPause = false;

    }

    public void OnPlayerWin()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        _playerFireController.OnGamePaused(isGamePaused);
        _playerMoveController.OnGamePaused(isGamePaused);
        _uiManager.ShowWinScreen();
        _isCanPause = false;
    }

    public float GetWinScore()
    {
        return _winScore;
    }
}
