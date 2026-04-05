using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Canvas _mainMenu;

    [SerializeField]
    private Canvas _deathScreen;

    [SerializeField]
    private Canvas _winScreen;

    private GameManager _gameManager;

    public void Start()
    {
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
    }

    public void OnStartGameButtonPressed()
    {
        _gameManager.StartGame();
    }

    public void OnCloseMenuButtonPressed()
    {
        HideMainMenu();
        _gameManager.SwitchPauseGame();
    }

    public void OnExitGameMenuButtonPressed()
    {
        _gameManager.ExitGame();
    }

    public void OnCloseGameMenuButtonPressed()
    {
        _gameManager.ToLobby();
    }

    public void ShowMainMenu()
    {
        _mainMenu.gameObject.SetActive(true);
    }

    public void HideMainMenu()
    {
        _mainMenu.gameObject.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        _deathScreen.gameObject.SetActive(true);
    }

    public void ShowWinScreen()
    {
        _winScreen.gameObject.SetActive(true);
    }
}
