using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireController : MonoBehaviour, IService
{
    [Header("Fire Settings")]

    [SerializeField]
    private Weapon[] weapons;
    [SerializeField]
    private Transform weaponSpawnPoint;

    // TODO: призначити в Inspector → XRI Default Input Actions → XRI RightHand/Trigger
    [SerializeField]
    private InputActionReference triggerAction;

    // TODO: призначити в Inspector → XRI Default Input Actions → XRI RightHand/GripPressed (для Reload)
    [SerializeField]
    private InputActionReference gripAction;

    [SerializeField]
    private EventBus _eventBus;

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
        // TODO: перевірити triggerAction.action.WasPressedThisFrame()
        //       якщо так — викликати currentWeapon?.Fire() та _eventBus.Publish(new PlayerShotEvent(...))

        if(triggerAction.action.WasPressedThisFrame())
        {
            currentWeapon?.Fire();
            _eventBus.Publish(new PlayerShotEvent(currentWeapon));
        }

        // TODO: перевірити gripAction.action.WasPressedThisFrame()
        //       якщо так — викликати currentWeapon?.Reload()

        // NOTE: SwitchWeapon через scroll прибрано — у VR буде окремий механізм (наступний крок)
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
        if(!isGamePaused) {
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
