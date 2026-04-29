using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireController : MonoBehaviour, IService
{
    [Header("Fire Settings")]

    [SerializeField] private Transform firePoint;

    [SerializeField] private Weapon[] weapons;
    [SerializeField]
    private string attackActionName = "Attack", reloadActionName = "Reload", switchWeaponActionName = "SwitchWeapon";
    [SerializeField] private Transform weaponSpawnPoint;
    private Transform _playerCamera;
    private AudioManager _audioManager;
    [SerializeField]
    private float _recoilRecoverySpeed = 5f;
    [SerializeField]
    private float _snapSpeed = 20f;
    [SerializeField] private Animator _animator;
    private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");
    private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");
    [SerializeField] private IKHandController _ikHandController;
    [SerializeField] private EventBus _eventBus;


    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction switchWeaponAction;
    private bool scrollUsed;


    private Vector3 _currentRecoil; 
    private Vector3 _targetRecoil;

    void Awake()
    {
        _playerCamera = IServiceLocator.Instance.GetService<PlayerMoveController>()?.GetPlayerCamera();
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
    }

    private IEnumerator Start()
    {
        attackAction = InputSystem.actions.FindAction(attackActionName);
        reloadAction = InputSystem.actions.FindAction(reloadActionName);
        switchWeaponAction = InputSystem.actions.FindAction(switchWeaponActionName);

        attackAction.Enable();
        reloadAction.Enable();
        switchWeaponAction.Enable();

        yield return null;
        if (weapons.Length > 0)
            takeWeapon(weapons[currentWeaponIndex]);
    }

    void Update()
    {
        if (attackAction.WasPressedThisFrame())
        {
            currentWeapon?.Fire();
            _animator.SetTrigger(IsShootingHash);
            _eventBus.Publish(new PlayerShotEvent(transform.position));
        }
            
        if (reloadAction.WasPressedThisFrame())
            currentWeapon?.Reload();

        int direction = (int)switchWeaponAction.ReadValue<float>();

        if (direction != 0 && !scrollUsed)
        {
            SwitchWeapon(direction > 0 ? 1 : -1);
            scrollUsed = true;
        }
        else if (direction == 0)
            scrollUsed = false;

        _targetRecoil = Vector3.Lerp(_targetRecoil, Vector3.zero, _recoilRecoverySpeed * Time.deltaTime);
        _currentRecoil = Vector3.Lerp(_currentRecoil, _targetRecoil, _snapSpeed * Time.deltaTime);
    }

    private void SwitchWeapon(int direction)
    {
        if (weapons.Length == 0)
            return;

        currentWeaponIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;
        takeWeapon(weapons[currentWeaponIndex]);
    }

    private void takeWeapon(Weapon weapon)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);
        GameObject spawnedWeapon = Instantiate(weapon.gameObject, weaponSpawnPoint, false);

        currentWeapon = spawnedWeapon.GetComponent<Weapon>();
        if (currentWeapon != null)
        {
            if(currentWeapon.GetWeaponGrip() != null)
            {
                _ikHandController.SetWeaponGrip(currentWeapon.GetWeaponGrip());
                _ikHandController.SetIKActive(true);
                _animator.SetBool(IsArmedHash, true);
                return;
            }
        }
        _animator.SetBool(IsArmedHash, false);
        _ikHandController.SetIKActive(false);
    }

    public void OnGamePaused(bool isGamePaused)
    {
        if (isGamePaused)
        {
            attackAction.Disable();
            reloadAction.Disable();
            switchWeaponAction.Disable();
        }
        else
        {
            attackAction.Enable();
            reloadAction.Enable();
            switchWeaponAction.Enable();
        }
    }

    public void ApplyRecoil(float recoilX, float recoilY)
    {
        _targetRecoil += new Vector3(-recoilY, Random.Range(-recoilX, recoilX), 0f);
    }

    public void EquipWeapon(WeaponItemData weaponItemData)
    {
        return;
    }

    public void UnArm()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);
        weapons = null;
        currentWeapon = null;
    }

    public Vector3 GetCurrentRecoil()
    {
        return _currentRecoil;
    }
}
