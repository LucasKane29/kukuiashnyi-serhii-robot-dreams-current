using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerFireController : MonoBehaviour, IService
{
    [Header("Fire Settings")]

    [SerializeField]
    private Weapon[] weapons;
    [SerializeField]
    private Transform weaponSpawnPoint;

    [SerializeField]
    private InputActionReference triggerAction;  // XRI RightHand/Trigger

    [SerializeField]
    private InputActionReference gripAction;     // XRI RightHand/GripPressed (Reload)

    [SerializeField]
    private EventBus _eventBus;

    [SerializeField]
    private ActionBasedController _rightHandController;

    private AudioManager _audioManager;
    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;

    void Awake()
    {
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
    }

    private IEnumerator Start()
    {
        triggerAction.action.Enable();
        gripAction.action.Enable();

        yield return null;
        if (weapons.Length > 0)
            takeWeapon(weapons[currentWeaponIndex]);
    }

    void Update()
    {
        if (triggerAction.action.WasPressedThisFrame())
        {
            currentWeapon?.Fire();
            _eventBus.Publish(new PlayerShotEvent(transform.position));
        }

        if (gripAction.action.WasPressedThisFrame())
        {
            currentWeapon?.Reload();
        }
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
        if (!isGamePaused)
        {
            triggerAction.action.Enable();
            gripAction.action.Enable();
        }
        else
        {
            triggerAction.action.Disable();
            gripAction.action.Disable();
        }
    }

    public void SendHapticImpule(float amplitude, float duration)
    {
        _rightHandController?.SendHapticImpulse(amplitude, duration);
    }

    // У VR відмова екрану замінюється haptic-зворотнім зв'язком контролера
    public void ApplyRecoil(float x, float y)
    {
        SendHapticImpule(Mathf.Clamp01(x / 10f), 0.1f);
    }

    // У VR немає візуального recoil камери — повертаємо нуль
    public Vector3 GetCurrentRecoil()
    {
        return Vector3.zero;
    }

    public void EquipWeapon(WeaponItemData weaponItemData) { }

    public void UnArm()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);
        weapons = null;
        currentWeapon = null;
    }
}
