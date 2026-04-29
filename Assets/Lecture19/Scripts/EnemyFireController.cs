using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyFireController : MonoBehaviour
{
    [Header("Fire Settings")]

    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponSpawnPoint;


    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;
    private Dictionary<Weapon, (Vector3 pos, Quaternion rot)> _originalTransforms = new();
    private AudioManager _audioManager;

    private void Awake()
    {
        foreach (var weapon in weapons)
        {
            if (weapon != null)
                _originalTransforms[weapon] = (weapon.transform.localPosition, weapon.transform.localRotation);
        }
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
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
            currentWeapon.gameObject.SetActive(false);

        if (_originalTransforms.TryGetValue(weapon, out var original))
        {
            weapon.transform.localPosition = original.pos;
            weapon.transform.localRotation = original.rot;
        }

        weapon.gameObject.SetActive(true);
        currentWeapon = weapon;
    }

    public void FireIntoTarget(Vector3 targetPosition)
    {
        if (currentWeapon == null)
            return;
        targetPosition.y += 0.75f; // Aim at the upper body
        currentWeapon?.Fire(targetPosition);
    }

    public void HideWeapons()
    {
        foreach (var weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }
        currentWeapon = null;
    }

    public Transform GetCurrentWeaponGrip()
    {
        if (currentWeapon == null) return null;

        return currentWeapon.GetWeaponGrip();
    }
}
