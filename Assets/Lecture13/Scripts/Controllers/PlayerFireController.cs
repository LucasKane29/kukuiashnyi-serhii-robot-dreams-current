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


    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction switchWeaponAction;
    private bool scrollUsed;
    [SerializeField]
    private float _recoilRecoverySpeed = 5f;
    [SerializeField]
    private float _snapSpeed = 20f; 

    private Vector3 _currentRecoil; 
    private Vector3 _targetRecoil;

    void Awake()
    {
        _playerCamera = IServiceLocator.Instance.GetService<PlayerMoveController>()?.GetPlayerCamera();
    }

    void Start()
    {
        attackAction = InputSystem.actions.FindAction(attackActionName);
        reloadAction = InputSystem.actions.FindAction(reloadActionName);
        switchWeaponAction = InputSystem.actions.FindAction(switchWeaponActionName);

        if (weapons.Length > 0)
            takeWeapon(weapons[currentWeaponIndex]);
    }

    void Update()
    {
        if (attackAction.WasPressedThisFrame())
            currentWeapon?.Fire();

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
        _playerCamera.localRotation = Quaternion.Euler(_currentRecoil);
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
}
