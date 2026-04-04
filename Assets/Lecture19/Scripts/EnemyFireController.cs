using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyFireController : MonoBehaviour, IService
{
    [Header("Fire Settings")]

    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponSpawnPoint;
    [SerializeField] private EventBus eventBus;


    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;

    void Start()
    {
        if (weapons.Length > 0)
            takeWeapon(weapons[currentWeaponIndex]);
    }

    void Update()
    {
        
    }

    public void SwitchWeapon(string weaponClassName)
    {
        if (weapons.Length == 0)
            return;

        if (currentWeapon != null && currentWeapon.GetType().Name == weaponClassName)
            return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].GetType().Name == weaponClassName)
            {
                currentWeaponIndex = i;
                takeWeapon(weapons[currentWeaponIndex]);
                return;
            }
        }
    }

    private void takeWeapon(Weapon weapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }
        weapon.gameObject.SetActive(true);
        currentWeapon = weapon;

    }

    public void FireIntoTarget(Vector3 targetPosition)
    {
        if (currentWeapon == null)
            return;
        currentWeapon?.Fire();
    }

    public void SetEventBus(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void HideWeapons()
    {
        foreach (var weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }
    }
}
