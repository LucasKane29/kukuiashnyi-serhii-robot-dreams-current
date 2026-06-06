using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Transform _mainMenu;

    [SerializeField]
    private Transform _deathScreen;

    [SerializeField]
    private Transform _winScreen;

    [SerializeField]
    private Transform _settingsMenu;

    [SerializeField]
    private Transform _loadButton;

    [SerializeField]
    private Slider _masterVolumeSlider;

    [SerializeField]
    private Slider _musicVolumeSlider;

    [SerializeField]
    private Slider _sfxVolumeSlider;

    private GameManager _gameManager;
    private SaveSystemManager _saveSystemManager;
    private AudioManager _audioManager;
    private Coroutine _playSoundCoroutine;

    public void Start()
    {
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        if ( _saveSystemManager != null  && _saveSystemManager.CheckIfSaveFileExists() && _loadButton != null)
        {
            _loadButton.gameObject.SetActive(true);
        }
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();

        StartCoroutine(UpdateUiSliders());

        UpdateSliders(_audioManager.GetMasterVolume(), _audioManager.GetMusicVolume(), _audioManager.GetSFXVolume());

        if (_masterVolumeSlider != null)
            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeValueChanged);
        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeValueChanged);
        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeValueChanged);
    }

    public void OnStartGameButtonPressed()
    {
        _gameManager.StartGame();
    }

    public void OnCloseMenuButtonPressed()
    {
        _gameManager.ResumeGame();
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

    public void ShowSettingsMenu()
    {
        _settingsMenu.gameObject.SetActive(true);
    }

    public void HideSettingsMenu()
    {
        _settingsMenu.gameObject.SetActive(false);
    }

    public void OnSettingsMenuButtonPressed()
    {
        ShowSettingsMenu();
        HideMainMenu();
    }

    public void OnReturnSettingsMenuButtonPressed()
    {
        HideSettingsMenu();
        ShowMainMenu();
    }

    public void OnSaveGameButtonPressed()
    {
        _saveSystemManager.SaveGame();
    }

    public void OnLoadGameButtonPressed()
    {
        _saveSystemManager.RequestLoad();
        _gameManager.ReloadScene();
        UpdateSliders(_audioManager.GetMasterVolume(), _audioManager.GetMusicVolume(), _audioManager.GetSFXVolume());
    }

    public void OnMasterVolumeValueChanged(float newValue)
    {
        _audioManager.SetMasterVolume(newValue);
        PlaySoundWithCheck("sfx_check");
    }

    public void OnMusicVolumeValueChanged(float newValue)
    {
        _audioManager.SetMusicVolume(newValue);
        PlaySoundWithCheck("music_check");
    }

    public void OnSFXVolumeValueChanged(float newValue)
    {
        _audioManager.SetSFXVolume(newValue);
        PlaySoundWithCheck("sfx_check");
    }

    private void UpdateSliders(float masterVolume, float musicVolume, float sfxVolume)
    {
        Debug.Log($"Updating sliders: Master={masterVolume}, Music={musicVolume}, SFX={sfxVolume}");
        if (_masterVolumeSlider != null)
            _masterVolumeSlider.value = masterVolume;
        if (_musicVolumeSlider != null)
            _musicVolumeSlider.value = musicVolume;
        if (_sfxVolumeSlider != null)
            _sfxVolumeSlider.value = sfxVolume;
    }

    private IEnumerator UpdateUiSliders()
    {
        yield return null;
        if (_audioManager != null)
        {
            UpdateSliders(_audioManager.GetMasterVolume(), _audioManager.GetMusicVolume(), _audioManager.GetSFXVolume());
        }
    }

    private void PlaySoundWithCheck(string soundName)
    {
        if(_playSoundCoroutine != null) { 
            StopCoroutine(_playSoundCoroutine);
        }
        _playSoundCoroutine = StartCoroutine(PlayVolumeChangeSound(soundName));
    }

    private IEnumerator PlayVolumeChangeSound(string soundName)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        _audioManager.PlaySound(soundName);
    }
}