using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        RangedAttack,
        MeleeAttack,
        Search,
        TakeDamage,
        Death
    }

    [Header("Поточний стан (дебаг)")]
    [SerializeField] private EnemyState _currentFSMState;
    public EnemyState CurrentFSMState => _currentFSMState;
    public bool WasHitByPlayer => _wasHitByPlayer;
    public Transform Player => _player;
    public float CurrentHealth => _hitPoints;
    public float MaxHealth => _maxHealth;
    public NavMeshAgent Agent => _navMeshAgent;
    public float PatrolWaitTime => _patrolWaitTime;
    public float ShootRange => _shootRange;
    public Vector3 CurrentPatrolTarget => _currentPatrolTarget;
    public float FireRate => _fireRate;
    public float MeleeRate => _meleeRate;
    public Vector3? HeardSoundPosition => _heardSoundPosition;
    public float SearchDuration => _searchDuration;

    public Animator Animator => _animator;

    [Header("Детекція")]
    [SerializeField] private float _sightRange = 20f;
    [SerializeField] private float _fieldOfViewAngle = 120f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _hearingRange = 30f;

    [Header("Патрулювання")]
    [SerializeField] private float _patrolWaitTime = 2f;
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _patrolRadius = 10f;

    [Header("Переслідування")]
    [SerializeField] private float _chaseSpeed = 5f;
    [SerializeField] private float _searchDuration = 5f;

    [Header("Дальня атака")]
    [SerializeField] private float _shootRange = 15f;
    [SerializeField] private float _fireRate = 0.5f;

    [Header("Ближній бій")]
    [SerializeField] private float _meleeRange = 2.5f;
    [SerializeField] private float _meleeRate = 1f;

    [Header("Здоров'я")]
    [SerializeField] private float _hitPoints = 100f;

    [SerializeField] private float _scoreForDeath = 1.0f;
    [SerializeField] private GameObject _shotEffect;
    [SerializeField] private float _shotEffectDuration = 0.5f;
    [SerializeField] private NavMeshAgent _navMeshAgent;

    [SerializeField] protected Animator _animator;
    [SerializeField] private Transform _weaponContainer;
    [SerializeField] private EnemyFireController _enemyFireController;
    [SerializeField] private HealthbarController _healthbarController;

    [SerializeField] private LayerMask layer;
    [SerializeField] private GameObject _dropItem;
    [SerializeField] private float _dampTime = 0.1f;
    [SerializeField] private IKHandController _iKHandController;

    private float currentScoreForDeath;
    private HashSet<VisualEffect> activeEffects = new HashSet<VisualEffect>();

    private float _maxHealth;
    private bool isDead = false;
    private LogService _logService;
    private ScoreManager _scoreManager;
    [SerializeField] private Transform _player;
    [SerializeField] private EventBus _eventBus;
    [SerializeField] private string _hurtSoundId;
    [SerializeField] private string _deathSoundId;

    private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
    private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsShootingHash = Animator.StringToHash("IsShooting");
    private static readonly int IsMeleeAttackHash = Animator.StringToHash("IsMeleeAttack");
    private static readonly int IsTakingDamageHash = Animator.StringToHash("IsTakingDamage");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int IsArmedHash = Animator.StringToHash("IsArmed");

    public Animator EnemyAnimator => _animator;
    public int HitCount { get; private set; }
    public bool IsWeaponDrawn => _animator.GetBool(IsArmedHash);

    private BTNode _behaviourTree;
    private Blackboard _blackboard;
    private EnemyState _previousFSMState;
    private bool _wasHitByPlayer;
    private Vector3 _currentPatrolTarget;
    private Vector3? _heardSoundPosition;
    private System.Action<PlayerShotEvent> _onPlayerShot;
    private AudioManager _audioManager;


    void Awake()
    {
        _maxHealth = _hitPoints;
        currentScoreForDeath = _scoreForDeath;
        _logService = IServiceLocator.Instance.GetService<LogService>();
        _scoreManager = IServiceLocator.Instance.GetService<ScoreManager>();
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
        if (_enemyFireController == null)
            _enemyFireController = GetComponent<EnemyFireController>();
    }

    void Start()
    {
        _navMeshAgent.speed = _patrolSpeed;
        BuildBehaviourTree();
        _onPlayerShot = e => AlertToSound(e.playerPosition);
        _eventBus.Subscribe<PlayerShotEvent>(_onPlayerShot);
    }

    void OnDestroy()
    {
        if (_eventBus != null && _onPlayerShot != null)
            _eventBus.Unsubscribe<PlayerShotEvent>(_onPlayerShot);
    }

    void Update()
    {

        if (_currentFSMState == EnemyState.Death) return;

        Vector3 moveDirection = _navMeshAgent.velocity;
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        float speed = moveDirection.magnitude / _chaseSpeed;

        _animator.SetFloat(VelocityXHash, localMoveDirection.x / _chaseSpeed, _dampTime, Time.deltaTime);
        _animator.SetFloat(VelocityZHash, localMoveDirection.z / _chaseSpeed, _dampTime, Time.deltaTime);
        _animator.SetFloat(SpeedHash, speed, _dampTime, Time.deltaTime);

        // Тікаємо дерево поведінки
        _behaviourTree.SetBlackboard(_blackboard);
        _behaviourTree.Tick();
    }

    private void BuildBehaviourTree()
    {
        _blackboard = new Blackboard();

        _behaviourTree = new PrioritySelector("ROOT",

            // ── 1. СМЕРТЬ (найвищий пріоритет) ──
            new ReactiveSequence("Death check",
                new Inverter("Not alive?", new CheckIsAlive(this)),
                new DeathAction(this)
            ),

            // ── 2. РЕАКЦІЯ НА ШКОДУ ──
            new ReactiveSequence("Damage reaction",
                new CheckWasHit(this),
                new TakeDamageAction(this)
            ),

            // ── 3. БІЙ (бачить гравця) ──
            new ReactiveSequence("Combat",
                new CheckPlayerVisible(this),
                new PrioritySelector("Choose attack",

                    // 3a. Ближній бій
                    new ReactiveSequence("Melee",
                        new CheckDistanceToPlayer(this, _meleeRange, lessThan: true),
                        new MeleeAction(this)
                    ),

                    // 3b. Стрільба
                    new ReactiveSequence("Ranged",
                        new CheckDistanceToPlayer(this, _shootRange, lessThan: true),
                        new Cooldown("Fire cooldown",
                            new ShootAction(this),
                            _fireRate
                        )
                    ),

                    // 3c. Переслідування (бачить, але далеко)
                    new ChaseAction(this)
                )
            ),

            // ── 4. ТРИВОГА (чув звук, але не бачить) ──
            new ReactiveSequence("Alert investigation",
                new PrioritySelector("Alert trigger",
                    new CheckHeardSound(this),
                    new CheckBlackboard("playerDetected", true)
                ),
                new SearchAction(this)
            ),

            // ── 5. ПАТРУЛЮВАННЯ (за замовчуванням) ──
            new PatrolAction(this)
        );
    }

    public void TransitionTo(EnemyState newState)
    {
        if (_currentFSMState == newState) return;

        _previousFSMState = _currentFSMState;
        _currentFSMState = newState;

        switch (newState)
        {
            case EnemyState.Patrol:
                _enemyFireController.HideWeapons();
                _animator.SetBool(IsArmedHash, false);
                _navMeshAgent.isStopped = false;
                _navMeshAgent.speed = _patrolSpeed;
                _iKHandController.SetIKActive(false);
                break;

            case EnemyState.Chase:
                _enemyFireController.HideWeapons();
                _animator.SetBool(IsArmedHash, false);
                _navMeshAgent.isStopped = false;
                _navMeshAgent.speed = _chaseSpeed;
                _iKHandController.SetIKActive(false);
                _animator.SetFloat(SpeedHash, 1f);
                break;

            case EnemyState.RangedAttack:
                _enemyFireController.HideWeapons();
                _animator.SetBool(IsArmedHash, false);
                _animator.ResetTrigger(IsMeleeAttackHash);
                if (_previousFSMState == EnemyState.MeleeAttack)
                    _animator.CrossFade("IsShooting", 0.1f);
                _navMeshAgent.isStopped = true;
                _iKHandController.SetIKActive(true);
                break;

            case EnemyState.MeleeAttack:
                _enemyFireController.HideWeapons();
                _animator.SetBool(IsArmedHash, false);
                _navMeshAgent.isStopped = true;
                _animator.applyRootMotion = false;
                _iKHandController.SetIKActive(false);
                break;

            case EnemyState.Search:
                _enemyFireController.HideWeapons();
                _animator.SetBool(IsArmedHash, false);
                _navMeshAgent.isStopped = false;
                _navMeshAgent.speed = _patrolSpeed;
                _iKHandController.SetIKActive(false);
                break;

            case EnemyState.TakeDamage:
                _enemyFireController.HideWeapons();
                _iKHandController.SetIKActive(false);
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
                if (_hitPoints > 0 && !isDead)
                {
                    _animator.SetTrigger(IsTakingDamageHash);
                    _healthbarController.UpdateBar(_hitPoints, _maxHealth);
                    _audioManager.PlaySoundAtPosition(_hurtSoundId, transform.position);
                }

                break;

            case EnemyState.Death:
                _animator.applyRootMotion = true;
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;
                _healthbarController.Hide();
                _animator.SetTrigger(DieHash);
                _audioManager.PlaySoundAtPosition(_deathSoundId, transform.position);
                break;
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 damagerPosition)
    {
        if (isDead)
            return;
        string objectName = gameObject.name;
        _hitPoints -= damage;
        _wasHitByPlayer = true;
        HitCount++;
        if (_hitPoints < 0)
            _hitPoints = 0;
        _logService.Log($"{objectName} took {damage} damage! Current HP: {_hitPoints}/{_maxHealth}");
        StartCoroutine(ShowShotEffect(hitPoint));
    }

    public void PerformDeath()
    {
        if (isDead) return;

        isDead = true;
        StopAllCoroutines();
        TransitionTo(EnemyState.Death);

        foreach (var effect in activeEffects)
        {
            if (effect != null)
                Destroy(effect.gameObject);
        }
        activeEffects.Clear();

        FinalizeDeath();
    }

    protected void FinalizeDeath()
    {
        _logService.Log($"{gameObject.name} died!");
        _scoreManager.AddScore(currentScoreForDeath);

        if (_dropItem != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.15f;
            Instantiate(_dropItem, dropPosition, Quaternion.identity);
        }

        gameObject.SetActive(false);
    }

    public void IncreaseScoreForDeath(float additionalScore)
    {
        currentScoreForDeath += additionalScore;
        _logService.Log($"Score for killing {gameObject.name} is now {currentScoreForDeath}");
    }

    private IEnumerator ShowShotEffect(Vector3 hitPoint)
    {
        if (isDead || _shotEffect == null) yield break;

        GameObject effect = Instantiate(_shotEffect, hitPoint, _shotEffect.transform.rotation, this.transform);
        VisualEffect vfx = effect.GetComponent<VisualEffect>();

        if (vfx == null || !activeEffects.Add(vfx))
        {
            Destroy(effect);
            yield break;
        }

        vfx.Play();

        yield return new WaitForSeconds(_shotEffectDuration);

        if (this == null || effect == null) yield break;

        activeEffects.Remove(vfx);
        Destroy(effect);
    }

    public bool CanSeePlayer()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance > _sightRange) return false;

        Vector3 dirToPlayer = (_player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _fieldOfViewAngle / 2f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = _player.position + Vector3.up * 1f;

        if (Physics.Raycast(eyePos, (targetPos - eyePos).normalized,
                out RaycastHit hit, _sightRange, _obstacleMask))
        {
            return hit.transform.CompareTag("Player");
        }
        return false;
    }

    public void SetPlayer(Transform player)
    {
        _player = player;
    }

    public void SetEventBus(EventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetStatus()
    {
        if (_behaviourTree != null)
            _behaviourTree.Reset();
        _navMeshAgent.enabled = true;

        isDead = false;
        _hitPoints = _maxHealth;
        _animator.applyRootMotion = false;
        _animator.transform.localPosition = Vector3.zero;
        _animator.transform.localRotation = Quaternion.identity;

        _wasHitByPlayer = false;
        _heardSoundPosition = null;
        HitCount = 0;

        _currentFSMState = EnemyState.Patrol;
        _previousFSMState = EnemyState.Patrol;

        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _patrolSpeed;

        _animator.applyRootMotion = false;
        _animator.Rebind();
        _animator.Update(0f);

        _enemyFireController.HideWeapons();
        _iKHandController.SetIKActive(false);

        _healthbarController.Show();
        _healthbarController.ResetBar();
        TransitionTo(EnemyState.Patrol);

    }

    public void ReTriggerHit()
    {
        if (_hitPoints > 0 && !isDead)
        {
            _animator.SetTrigger(IsTakingDamageHash);
            _healthbarController.UpdateBar(_hitPoints, _maxHealth);
        }
    }

    public void NextPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * _patrolRadius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _patrolRadius, NavMesh.AllAreas))
        {
            _currentPatrolTarget = hit.position;
        }
    }

    public void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
    }

    public void DrawWeapon(string weaponName)
    {
        _animator.SetBool(IsArmedHash, true);
        _enemyFireController.SwitchWeapon(weaponName);
        if (weaponName == "MeleeWeapon")
        {
            _iKHandController.SetWeaponGrip(_enemyFireController.GetCurrentWeaponGrip(), false);
        }
        else
        {
            _iKHandController.SetWeaponGrip(_enemyFireController.GetCurrentWeaponGrip());
            _iKHandController.SnapToWeaponGrip();
        }
    }

    public void PerformShoot()
    {
        _animator.SetTrigger(IsShootingHash);
        _enemyFireController.FireIntoTarget(_player.position);
    }

    public void PerformMelee()
    {
        _animator.SetTrigger(IsMeleeAttackHash);
        _enemyFireController.FireIntoTarget(_player.position);
    }

    public void ClearHitFlag()
    {
        _wasHitByPlayer = false;
        _heardSoundPosition = null;
    }

    /// <summary>Зовнішній виклик коли гравець стріляє поблизу.</summary>
    public void AlertToSound(Vector3 soundPosition)
    {
        if (_currentFSMState == EnemyState.Death) return;

        float dist = Vector3.Distance(transform.position, soundPosition);
        if (dist <= _hearingRange)
        {
            _heardSoundPosition = soundPosition;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _shootRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _meleeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _hearingRange);

        Vector3 fovL = Quaternion.AngleAxis(_fieldOfViewAngle / 2f, Vector3.up) * transform.forward * _sightRange;
        Vector3 fovR = Quaternion.AngleAxis(-_fieldOfViewAngle / 2f, Vector3.up) * transform.forward * _sightRange;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + fovL);
        Gizmos.DrawLine(transform.position, transform.position + fovR);
    }

    public void OnDeathAnimationFinished()
    {
        FinalizeDeath(); 
    }

    public void UnArm()
    {
        _animator.SetBool(IsArmedHash, false);
    }
}
