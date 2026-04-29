using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 10f;

    [SerializeField] protected float fireRate = 1f;

    [SerializeField] protected int maxAmmo = 10;
    [SerializeField] protected float reloadTime = 2f;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] private float _recoilX = 2f;
    [SerializeField] private float _recoilY = 0.5f;
    [SerializeField] protected bool _isPlayerWeapon = false;
    [SerializeField] protected Transform _weaponGrip;
    [SerializeField] protected string _soundId;
    private ScoreManager _scoreManager;
    private PlayerFireController _playerFireController;
    protected AudioManager _audioManager;

    protected int currentAmmo;
    protected float nextFireTime;
    protected bool isReloading;



    public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= nextFireTime;

    protected virtual void Awake()
    {
        currentAmmo = maxAmmo;
    }

    void Start()
    {
        _scoreManager = IServiceLocator.Instance.GetService<ScoreManager>();
        if(_isPlayerWeapon)
        {
            _playerFireController = IServiceLocator.Instance.GetService<PlayerFireController>();
        }
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
    }

    public abstract void Fire(Vector3? targetDirection = null);

    public virtual void Reload()
    {
        if (!isReloading && currentAmmo < maxAmmo)
            StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    protected void MadeShot()
    {
        if (_isPlayerWeapon)
        {
            _scoreManager.OnShotMade();
            _playerFireController.ApplyRecoil(_recoilX, _recoilY);
        }
    }

    public Transform GetWeaponGrip()
    {
        return _weaponGrip;
    }

    protected void PlayFireSound()
    {
        _audioManager.PlaySoundAtPosition(_soundId, transform.position);
    }
}
