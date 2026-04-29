using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ServiceInstaller : MonoBehaviour
{
    [SerializeField] UIManager _uiManager;
    [SerializeField] SpawnManager _spawnManager;
    [SerializeField] ScoreManager _scoreManager;
    [SerializeField] GameManager _gameManager;
    [SerializeField] PlayerMoveController _playerMoveController;
    [SerializeField] PlayerFireController _playerFireController;
    [SerializeField] InventoryManager _inventoryManager;
    [SerializeField] MerchantController _merchantController;
    [SerializeField] LocalizationManager _localizationManager;
    [SerializeField] SaveSystemManager _saveSystemManager;
    [SerializeField] AudioManager _audioManager;
    private readonly LogService _logService = new();

    private void Awake()
    {
        IServiceLocator.Instance.TryRegisterService<LogService>(_logService);
        IServiceLocator.Instance.TryRegisterService<UIManager>(_uiManager);
        IServiceLocator.Instance.TryRegisterService<SpawnManager>(_spawnManager);
        IServiceLocator.Instance.TryRegisterService<ScoreManager>(_scoreManager);
        IServiceLocator.Instance.TryRegisterService<GameManager>(_gameManager);
        IServiceLocator.Instance.TryRegisterService<PlayerMoveController>(_playerMoveController);
        IServiceLocator.Instance.TryRegisterService<PlayerFireController>(_playerFireController);
        IServiceLocator.Instance.TryRegisterService<InventoryManager>(_inventoryManager);
        IServiceLocator.Instance.TryRegisterService<MerchantController>(_merchantController);
        IServiceLocator.Instance.TryRegisterService<LocalizationManager>(_localizationManager);
        IServiceLocator.Instance.TryRegisterService<SaveSystemManager>(_saveSystemManager);
        IServiceLocator.Instance.TryRegisterService<AudioManager>(_audioManager);
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        IServiceLocator.Instance.TryUnregisterService<LogService>(_logService);
        IServiceLocator.Instance.TryUnregisterService<UIManager>(_uiManager);
        IServiceLocator.Instance.TryUnregisterService<SpawnManager>(_spawnManager);
        IServiceLocator.Instance.TryUnregisterService<ScoreManager>(_scoreManager);
        IServiceLocator.Instance.TryUnregisterService<GameManager>(_gameManager);
        IServiceLocator.Instance.TryUnregisterService<PlayerMoveController>(_playerMoveController);
        IServiceLocator.Instance.TryUnregisterService<PlayerFireController>(_playerFireController);
        IServiceLocator.Instance.TryUnregisterService<InventoryManager>(_inventoryManager);
        IServiceLocator.Instance.TryUnregisterService<MerchantController>(_merchantController);
        IServiceLocator.Instance.TryUnregisterService<LocalizationManager>(_localizationManager);
        IServiceLocator.Instance.TryUnregisterService<SaveSystemManager>(_saveSystemManager);
        IServiceLocator.Instance.TryUnregisterService<AudioManager>(_audioManager);
    }
}
