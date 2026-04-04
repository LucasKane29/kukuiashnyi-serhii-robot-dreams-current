using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private EventBus eventBus;

    [SerializeField]
    private Canvas mainMenu;

    public void OnStartGameButtonPressed()
    {
        eventBus.Publish(new StartedGameEvent());
    }

    public void OnCloseMenuButtonPressed()
    {
        eventBus.Publish(new ClosedMenuEvent());
    }

    public void OnExitGameMenuButtonPressed()
    {
        eventBus.Publish(new ExitGameEvent());
    }

    public void OnCloseGameMenuButtonPressed()
    {
        eventBus.Publish(new ClosedGameEvent());
    }

    public void ShowMainMenu()
    {
        mainMenu.gameObject.SetActive(true);
    }

    public void HideMainMenu()
    {
        mainMenu.gameObject.SetActive(false);
    }
}
