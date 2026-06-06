using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour, IService, ISaveable
{
    [Header("Move Settings")]

    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private float _walkSpeed = 0.5f, _runSpeed = 1f, _rotationSpeed = 5f;

    [SerializeField]
    private float _topClamp = 10f, _bottomClamp = -45f;

    [SerializeField]
    private float _lookRange = 100f;

    [SerializeField]
    private Transform _followTarget, _weaponTarget, _playerCamera;

    [SerializeField]
    private string _moveActionName = "Move", _lookActionName = "Look", _sprintActionName = "Sprint";

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private Transform _aimTarget;

    [SerializeField]
    private float _minAimDistance = 3f;
    [SerializeField]
    private float _dampTime = 0.1f;
    [SerializeField]
    private bool _isVRMode = false;

    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");

    private InputAction _moveAction, _lookAction, _sprintAction;
    private float _yaw = 0f;
    private float _pitch = 0f;
    private Vector2 _movementVector;

    private SaveSystemManager _saveSystemManager;
    private PlayerFireController _playerFireController;

    void Awake()
    {
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _saveSystemManager.RegisterSaveable(this);
        _playerFireController = IServiceLocator.Instance.GetService<PlayerFireController>();
    }

    void OnDestroy()
    {
        if (_saveSystemManager != null)
        {
            _saveSystemManager.UnregisterSaveable(this);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }

        _moveAction = InputSystem.actions.FindAction(_moveActionName);
        _lookAction = InputSystem.actions.FindAction(_lookActionName);
        _sprintAction = InputSystem.actions.FindAction(_sprintActionName);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (_playerCamera != null)
            _playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        if (_weaponTarget != null)
            _weaponTarget.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void Update()
    {
        _movementVector = _moveAction.ReadValue<Vector2>();
        Vector2 lookVector = _lookAction.ReadValue<Vector2>();

        _yaw += lookVector.x * _rotationSpeed * Time.deltaTime;

        _pitch += lookVector.y * _rotationSpeed * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _bottomClamp, _topClamp);

        float targetSpeed = _sprintAction != null && _sprintAction.IsPressed() ? 1f : 0.5f;
        float vx = _movementVector.x * targetSpeed;
        float vz = _movementVector.y * targetSpeed;

        if (_animator != null)
        {
            _animator.SetFloat(VelocityXHash, vx, _dampTime, Time.deltaTime);
            _animator.SetFloat(VelocityZHash, vz, _dampTime, Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        Move(_movementVector);
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Move(Vector2 input)
    {
        // У VR переміщення керується XR Interaction Toolkit (DynamicMoveProvider)
        if (_isVRMode) return;

        float speed = _sprintAction != null && _sprintAction.IsPressed() ? _runSpeed : _walkSpeed;
        Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed;
        _characterController.SimpleMove(move);
    }

    private void Look()
    {
        // У VR ротацію камери та тіла задає HMD — не перезаписуємо
        if (!_isVRMode)
        {
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 currentRecoil = _playerFireController != null ? _playerFireController.GetCurrentRecoil() : Vector3.zero;
            _playerCamera.transform.localRotation = Quaternion.Euler(_pitch + currentRecoil.x, currentRecoil.y, 0f);
        }

        // aimTarget завжди оновлюємо — у VR base forward = HMD forward
        Vector3 aimPoint = _playerCamera.position + _playerCamera.forward * _lookRange;

        if (Physics.Raycast(_playerCamera.position, _playerCamera.forward, out RaycastHit hit, _lookRange))
        {
            aimPoint = hit.point;
            if (hit.distance < _minAimDistance)
            {
                aimPoint = _playerCamera.position + _playerCamera.forward * _minAimDistance;
            }
        }
        if (_aimTarget != null)
            _aimTarget.position = aimPoint;
        if (_weaponTarget != null)
            _weaponTarget.transform.rotation = Quaternion.LookRotation((aimPoint - _weaponTarget.position).normalized);
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !focus;
    }

    public void OnGamePaused(bool isGamePaused)
    {
        if (isGamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnPlayerDeath()
    {
        enabled = false;
    }

    public Transform GetPlayer()
    {
        return transform;
    }

    public Transform GetPlayerCamera()
    {
        return _playerCamera;
    }

    public SaveData GetSaveData(SaveData data)
    {
        data.playerPosition = transform.position;
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        _characterController.enabled = false;
        transform.position = data.playerPosition;
        _characterController.enabled = true;
        Debug.Log($"SetSaveData called on {gameObject.name}");
    }
}
