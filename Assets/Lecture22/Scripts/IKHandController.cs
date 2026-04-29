using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKHandController : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint _twoBoneIKConstraint;
    [SerializeField] private TwoBoneIKConstraint _twoBoneIKConstraintRight;
    [SerializeField] private Transform _weaponGrip;
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    [SerializeField] private float _followSpeed = 15f;
    [SerializeField] private TwoBoneIKConstraint[] _twoBoneIKConstraintArray;

    [Range(0f, 1f)]
    [SerializeField] float _ikWeight = 1f;

    void Start()
    {
        if (_twoBoneIKConstraint == null)
        {
            Debug.LogError("TwoBoneIKConstraint reference is missing on " + gameObject.name);
            return;
        }
        _twoBoneIKConstraint.weight = 0f;
        StartCoroutine(HideFirstFrame());
    }

    private void LateUpdate()
    {
        if (_weaponGrip != null)
        {
            if (_twoBoneIKConstraint != null)
            {
                _twoBoneIKConstraint.data.target.position = Vector3.Lerp(_twoBoneIKConstraint.data.target.position, _weaponGrip.position, _followSpeed * Time.deltaTime);
                _twoBoneIKConstraint.data.target.rotation = Quaternion.Lerp(_twoBoneIKConstraint.data.target.rotation, _weaponGrip.rotation, _followSpeed * Time.deltaTime);
            }
            if (_twoBoneIKConstraintRight != null)
            {
                _twoBoneIKConstraintRight.data.target.position = Vector3.Lerp(_twoBoneIKConstraintRight.data.target.position, _weaponGrip.position, _followSpeed * Time.deltaTime);
                _twoBoneIKConstraintRight.data.target.rotation = Quaternion.Lerp(_twoBoneIKConstraintRight.data.target.rotation, _weaponGrip.rotation, _followSpeed * Time.deltaTime);
            }
        }
    }

    public void SetIKActive(bool isActive)
    {
        /*
        if (_twoBoneIKConstraint != null)
        {
            _twoBoneIKConstraint.weight = isActive ? _ikWeight : 0f;
        }
        */
        foreach (var ikConstraint in _twoBoneIKConstraintArray)
        {
            if (ikConstraint != null)
            {
                ikConstraint.weight = isActive ? _ikWeight : 0f;
            }
        }
    }

    public void SetWeaponGrip(Transform weaponGrip, bool isActive = true)
    {
        if (weaponGrip != null)
        {
            _weaponGrip = weaponGrip;
            SetIKActive(isActive);
        }
    }

    public void SnapToWeaponGrip()
    {
        if (_weaponGrip != null)
        {
            if (_twoBoneIKConstraint != null)
            {
                _twoBoneIKConstraint.data.target.position = _weaponGrip.position;
                _twoBoneIKConstraint.data.target.rotation = _weaponGrip.rotation;
            }
            if (_twoBoneIKConstraintRight != null)
            {
                _twoBoneIKConstraintRight.data.target.position = _weaponGrip.position;
                _twoBoneIKConstraintRight.data.target.rotation = _weaponGrip.rotation;
            }
        }
    }

    private IEnumerator HideFirstFrame()
    {
        _meshRenderer.enabled = false;
        yield return null;
        _meshRenderer.enabled = true;

    }
}
