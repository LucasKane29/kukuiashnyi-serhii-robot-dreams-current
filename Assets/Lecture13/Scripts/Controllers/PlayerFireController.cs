using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Lesson13
{
    public class PlayerFireController : MonoBehaviour
    {
        [Header("Fire Settings")]

        [SerializeField] private Transform firePoint;

        [SerializeField] private Weapon[] weapons;
        [SerializeField]
        private string attackActionName = "Attack", reloadActionName = "Reload", switchWeaponActionName = "SwitchWeapon";
        [SerializeField] private Transform weaponSpawnPoint;
        [SerializeField] private EventBus eventBus;


        private int currentWeaponIndex = 0;
        private Weapon currentWeapon;
        private InputAction attackAction;
        private InputAction reloadAction;
        private InputAction switchWeaponAction;
        private bool scrollUsed;

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
        }

        private void FixedUpdate()
        {   
            
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
            currentWeapon.SetEventBus(eventBus);
        }   
    }
}
