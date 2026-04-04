using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private EventBus eventBus;

    [SerializeField]
    private bool isPauseAllowed = false;

    [SerializeField]
    private string pauseActionName = "Pause";

    private bool isGamePaused = false;
    private InputAction pauseAction;


    public void Start()
    {
        pauseAction = InputSystem.actions.FindAction(pauseActionName);
        eventBus.Publish(new GamePausedEvent(false));
        Time.timeScale = 1f;
    }

    public void Update()
    {
        if(pauseAction != null) {
            if (pauseAction.triggered && isPauseAllowed)
            {
                if (!isGamePaused)
                {
                    isGamePaused = true;
                    eventBus.Publish(new ShowedMenuEvent());
                    eventBus.Publish(new GamePausedEvent(isGamePaused));
                    Time.timeScale = 0f;
                }
                else
                {
                    eventBus.Publish(new ClosedMenuEvent());
                }
            }
        }
    }

    public void OnEnable()
    {
        eventBus.Subscribe<StartedGameEvent>(OnStartGame);
        eventBus.Subscribe<ClosedGameEvent>(OnGameClose);
        eventBus.Subscribe<ExitGameEvent>(OnExitGame);
        eventBus.Subscribe<ClosedMenuEvent>(OnClosedGameMenu);
    }

    public void OnDisable()
    {
        eventBus.Unsubscribe<StartedGameEvent>(OnStartGame);
        eventBus.Unsubscribe<ClosedGameEvent>(OnGameClose);
        eventBus.Unsubscribe<ExitGameEvent>(OnExitGame);
        eventBus.Unsubscribe<ClosedMenuEvent>(OnClosedGameMenu);
    }

    void OnStartGame(StartedGameEvent subscribedEvent)
    {
        SceneManager.LoadSceneAsync("Lecture19");
    }

    void OnGameClose(ClosedGameEvent subscribedEvent)
    {
        SceneManager.LoadSceneAsync("Lecture16");
    }

    void OnExitGame(ExitGameEvent subscribedEvent)
    {
        Application.Quit();
    }

    void OnClosedGameMenu(ClosedMenuEvent subscribedEvent)
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            eventBus.Publish(new GamePausedEvent(isGamePaused));
            Time.timeScale = 0f;
        }
        else
        {
            isGamePaused = false;
            eventBus.Publish(new GamePausedEvent(isGamePaused));
            Time.timeScale = 1f;
        }
    }
}
