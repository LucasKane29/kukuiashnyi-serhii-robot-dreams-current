using UnityEngine;

public class ServiceInstaller : MonoBehaviour
{
    [SerializeField] EnemyFireController _enemyFireController;
    [SerializeField] UIManager _uiManager;
    private readonly LogService _logService = new();

    private void Awake()
    {
        IServiceLocator.Instance.TryRegisterService<LogService>(_logService);
        IServiceLocator.Instance.TryRegisterService<EnemyFireController>(_enemyFireController);
        IServiceLocator.Instance.TryRegisterService<UIManager>(_uiManager);
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        IServiceLocator.Instance.TryUnregisterService<LogService>(_logService);
        IServiceLocator.Instance.TryUnregisterService<EnemyFireController>(_enemyFireController);
        IServiceLocator.Instance.TryUnregisterService<UIManager>(_uiManager);
    }
}
