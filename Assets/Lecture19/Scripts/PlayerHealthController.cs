using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerHealthController : MonoBehaviour, IDamageable, IHealable, ISaveable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Animator _animator;
    [SerializeField] private IKHandController _ikHandController;
    [SerializeField] private RigBuilder _rigBuilder;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private string _deathSoundId;
    [SerializeField] private string _isHurtSoundId;

    private readonly int _isTakingDamageHash = Animator.StringToHash("IsTakingDamage");
    private static readonly int _isArmedHash = Animator.StringToHash("IsArmed");
    private static readonly int _dieHash = Animator.StringToHash("Die");

    private UIManager _uiManager;
    private GameManager _gameManager;
    private PlayerFireController _playerFireController;
    private SaveSystemManager _saveSystemManager;
    private AudioManager _audioManager;
    private float currentHealth;
    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        _uiManager = IServiceLocator.Instance.GetService<UIManager>();
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
        _playerFireController = IServiceLocator.Instance.GetService<PlayerFireController>();
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
        _saveSystemManager.RegisterSaveable(this);
    }

    void OnDestroy()
    {         
        if (_saveSystemManager != null)
        {
            _saveSystemManager.UnregisterSaveable(this);
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 damagerPosition)
    {
        if (isDead) return;

        string objectName = gameObject.name;
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        Debug.Log($"{objectName} took {damage} damage!");
        _uiManager.UpdatePlayerHealthBar(currentHealth, maxHealth);
        _audioManager.PlaySoundAtPosition(_isHurtSoundId, transform.position);

        if (currentHealth <= 0)
        {
            isDead = true;
            _playerFireController.UnArm();
            _audioManager.PlaySoundAtPosition(_deathSoundId, transform.position);
            if (_rigBuilder != null) _rigBuilder.enabled = false;
            if (_ikHandController != null) _ikHandController.SetIKActive(false);
            if (_animator != null)
            {
                _animator.SetLayerWeight(1, 0f);
                _animator.SetBool(_isArmedHash, false);
                _animator.SetTrigger(_dieHash);
                _animator.applyRootMotion = true;
                // OnDeathAnimationFinished() буде викликано Animation Event
            }
            else
            {
                // У VR немає анімації смерті — викликаємо одразу
                OnDeathAnimationFinished();
            }
        }
        else
        {
            if (_animator != null)
                _animator.SetTrigger(_isTakingDamageHash);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        _uiManager.UpdatePlayerHealthBar(currentHealth, maxHealth);
    }

    public void OnDeathAnimationFinished()
    {
        string objectName = gameObject.name;
        Debug.Log($"{objectName} has died!");
        _gameManager.OnPlayerDeath();
    }

    public SaveData GetSaveData(SaveData data)
    {
        data.playerHealth = currentHealth;
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        currentHealth = data.playerHealth;
        _uiManager.UpdatePlayerHealthBar(currentHealth, maxHealth);
        Debug.Log($"SetSaveData called on {gameObject.name}");
    }
}
