using BehaviourTrees;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private float hitPoints = 100f;
    [SerializeField] private float scoreForDeath = 1.0f;
    [SerializeField] private GameObject shotEffect;
    [SerializeField] private float shotEffectDuration = 0.5f;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private int patrolPointLimit = 5;
    [SerializeField] private float restDuration = 5f;
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private float sightRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float meleeAttackRate = 2f;
    [SerializeField] private float rangeAttackRate = 3f;
    [SerializeField] private float aggroTimeOut = 10f;
    [SerializeField] private float meleeAttackRange = 1f; 
    [SerializeField] private Animator animator;
    [SerializeField] private Transform _weaponContainer;
    [SerializeField] private EnemyFireController _enemyFireController;
    [SerializeField] private HealthbarController _healthbarController;

    [SerializeField] private LayerMask layer;

    private float currentScoreForDeath;
    private HashSet<VisualEffect> activeEffects = new HashSet<VisualEffect>();

    private float maxHealth;
    private bool isAttacking = false;
    private bool isAggro = false;
    private bool isResettingAggro = false;
    private bool isDead = false;
    private bool _isPlayerVisible = false;
    private Coroutine resetDamagerPosition;
    private Coroutine resetAggro;
    private Vector3 damagerPosition = Vector3.zero;
    private Collider[] colliders = new Collider[10];
    private LogService _logService;
    private ScoreManager _scoreManager;
    private Transform _player;
    private BehaviourTree _behaviourTree = new BehaviourTree("EnemyBehaviour");


    void Awake()
    {
        maxHealth = hitPoints;
        currentScoreForDeath = scoreForDeath;
        _logService = IServiceLocator.Instance.GetService<LogService>();
        _scoreManager = IServiceLocator.Instance.GetService<ScoreManager>();
        if (_enemyFireController == null)
            _enemyFireController = GetComponent<EnemyFireController>();
    }

    void Start()
    {
        var chaseNode = new SequenceNode("Chase");
        chaseNode.AddChild(new Leaf("CheckIfPlayerInRange", new ConditionStrategy(() => { 
            return isPlayerVisible() && !isPlayerInRangeAttackRange();
        })));

        chaseNode.AddChild(new Leaf("Chasing", new ChaseStrategy(navMeshAgent, () => _player.position, isPlayerVisible)));

        var chaseToDamagerNode = new SequenceNode("ChaseToDamager");
        chaseToDamagerNode.AddChild(new Leaf("CheckIsCanChaising", new ConditionStrategy(() => {
            return isCanChasing();
        })));

        chaseToDamagerNode.AddChild(new Leaf("ChasingToDamager", new ChaseStrategy(navMeshAgent, GetDamagerPosition, isCanChasing)));

        var meleeAttackNode = new SequenceNode("MeleeAttack");
        meleeAttackNode.AddChild(new Leaf("CheckIfPlayerInMeleeRange", new ConditionStrategy(() => {
            return isPlayerInMeleeAttackRange();
        })));
        meleeAttackNode.AddChild(new Leaf("MeleeAttack", new MeleeAttackStrategy(navMeshAgent, () => _player.position, this, isPlayerInMeleeAttackRange)));

        var rangeAttackNode = new SequenceNode("RangeAttack");
        rangeAttackNode.AddChild(new Leaf("CheckIfPlayerInRangeAttack", new ConditionStrategy(() => {
            return isPlayerInRangeAttackRange();
        })));
        rangeAttackNode.AddChild(new Leaf("RangeAttack", new RangeAttackStrategy(navMeshAgent, () => _player.position, this, isPlayerInRangeAttackRange)));

        var patrolAndRestNode = new SequenceNode("PatrolAndRest");
        patrolAndRestNode.AddChild(new Leaf("Patroling", new PatrolStrategy(navMeshAgent, transform, patrolRadius, patrolPointLimit)));
        patrolAndRestNode.AddChild(new Leaf("Resting", new RestStrategy(restDuration, this)));

        var selectorNode = new SelectorNode(new List<Node>
        {
            chaseToDamagerNode,
            meleeAttackNode,
            rangeAttackNode,
            chaseNode,
            patrolAndRestNode
        });

        _behaviourTree.AddChild(selectorNode);
    }

    void Update()
    {
        _isPlayerVisible = isPlayerVisible();
        _behaviourTree.Process();
        if (GetAggro() && !isResettingAggro && !_isPlayerVisible)
        {
            resetAggro = StartCoroutine(ResetAggroStatus());
        }

        float speed = navMeshAgent.desiredVelocity.magnitude;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        navMeshAgent.updateRotation = !_isPlayerVisible && !navMeshAgent.isStopped;
        if (_isPlayerVisible)
        {
            Vector3 direction = (_player.position - transform.position);
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 damagerPosition)
    {
        if(isDead)
            return;

        string objectName = gameObject.name;
        hitPoints -= damage;
        if (hitPoints < 0)
            hitPoints = 0;
        _logService.Log($"{objectName} took {damage} damage! Current HP: {hitPoints}/{maxHealth}");
        this.damagerPosition = damagerPosition;
        _healthbarController.UpdateBar(hitPoints, maxHealth);
        StartCoroutine(ShowShotEffect(hitPoint));

        if(resetAggro != null)
        {
            StopCoroutine(resetAggro);
            isResettingAggro = false;
        }

        isAggro = true;
        
        if (resetDamagerPosition != null)
            StopCoroutine(resetDamagerPosition);
        StartCoroutine(ResetDamagerPosition());
    }

    void Die()
    {
        isDead = true;
        foreach (var effect in activeEffects)
            Destroy(effect);

        activeEffects.Clear();

        _logService.Log($"{gameObject.name} died!");
        _scoreManager.AddScore(currentScoreForDeath);
        gameObject.SetActive(false);
    }

    public abstract void runDieAnimation();
    public abstract void runTakeDamageAnimation();

    public void IncreaseScoreForDeath(float additionalScore)
    {
        currentScoreForDeath +=additionalScore;
        _logService.Log($"Score for killing {gameObject.name} is now {currentScoreForDeath}");
    }

    private IEnumerator ShowShotEffect(Vector3 hitPoint)
    {
        if (!isDead)
        {
            if (shotEffect != null)
            {
                GameObject effect = Instantiate(shotEffect, hitPoint, shotEffect.transform.rotation, this.transform);
                VisualEffect vfx = effect.GetComponent<VisualEffect>();
                if (activeEffects.Add(vfx))
                {
                    vfx.Play();

                    yield return new WaitForSeconds(shotEffectDuration);
                    activeEffects.Remove(vfx);
                    Destroy(effect);
                }
            }
            if (!isDead && hitPoints <= 0)
                Die();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 leftEdge = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;
        Vector3 rightEdge = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + rightEdge * sightRange);
        Gizmos.DrawLine(transform.position, transform.position + leftEdge * sightRange);
        Gizmos.DrawLine(transform.position + leftEdge * sightRange, transform.position + rightEdge * sightRange);

        Gizmos.color = Color.cyan;
        if(_player != null)
        {
            Gizmos.DrawLine(transform.position, _player.position);
        }

        if (GetAggro())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, sightRange);
        }
    }

    public bool isPlayerVisible()
    {
        if (GetAggro())
        {
            return IsPlayerVisibleAggro();
        }
        else
        {
            Vector3 directionToPlayer = (_player.position - transform.position);
            float fieldOfViewAngle = Vector3.Angle(transform.forward, directionToPlayer);
            if (directionToPlayer.magnitude <= sightRange && fieldOfViewAngle <= fieldOfView / 2)
            {
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, sightRange))
                {
                    if(hit.transform == _player)
                    {
                        isAggro = true;
                        return true; 
                    }
                }
            }
            return false;
        }
    }

    public bool isPlayerInMeleeAttackRange()
    {
        return _isPlayerVisible && Vector3.Distance(transform.position, _player.position) <= meleeAttackRange && navMeshAgent.desiredVelocity.magnitude == 0;
    }

    public bool isPlayerInRangeAttackRange()
    {
        float distanceToAim = Vector3.Distance(transform.position, _player.position);
        return _isPlayerVisible && distanceToAim <= attackRange && distanceToAim > meleeAttackRange && navMeshAgent.desiredVelocity.magnitude == 0;
    }

    public bool isCanChasing()
    {
        return damagerPosition != Vector3.zero && !isPlayerInRangeAttackRange();
    }

    public void meleeAttack(Vector3 target)
    {
        if(meleeAttackRate > 0 && !isAttacking)
            StartCoroutine(PerformMeleeAttack(target));
    }

    public void rangeAttack(Vector3 target)
    {
        if(rangeAttackRate > 0 && !isAttacking)
            StartCoroutine(PerformRangeAttack(target));
    }

    private IEnumerator PerformMeleeAttack(Vector3 target)
    {
        isAttacking = true;
        _enemyFireController.SwitchWeapon("MeleeWeapon");

        while (isPlayerInMeleeAttackRange())
        {
            animator.SetTrigger("IsMeleeAttack");
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("HandAttack"));

            _enemyFireController.FireIntoTarget(target);

            yield return new WaitForSeconds(meleeAttackRate);
            animator.ResetTrigger("IsMeleeAttack");
        }
        animator.ResetTrigger("IsMeleeAttack");
        isAttacking = false;
    }

    private IEnumerator PerformRangeAttack(Vector3 target)
    {
        isAttacking = true;
        _enemyFireController.SwitchWeapon("BlasterWeapon");

        while (isPlayerInRangeAttackRange())
        {
            animator.SetTrigger("IsRangeAttack");
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("ShotAttack"));

            _enemyFireController.FireIntoTarget(target);

            yield return new WaitForSeconds(rangeAttackRate);
            animator.ResetTrigger("IsRangeAttack");

        }
        animator.ResetTrigger("IsRangeAttack");
        isAttacking = false;
    }

    private IEnumerator ResetDamagerPosition()
    {
        yield return new WaitUntil(() =>
            !navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);
        damagerPosition = Vector3.zero;
    }

    private IEnumerator ResetAggroStatus()
    {
        isResettingAggro = true;

        yield return new WaitForSeconds(aggroTimeOut);
        if (!_isPlayerVisible)
        {
            isAggro = false;
        }
        isResettingAggro = false;
    }

    public Vector3 GetDamagerPosition()
    {
        return damagerPosition; 
    }

    public bool IsPlayerVisibleAggro()
    {
        int elementCount = Physics.OverlapSphereNonAlloc(transform.position, sightRange, colliders, layer);
        for (int i = 0; i < elementCount; i++)
        {
            if (colliders[i].transform == _player)
            {
                Vector3 directionToPlayer = (_player.position - transform.position);
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, out RaycastHit hit, sightRange, layer))
                {
                    if (hit.transform == _player)
                    {
                        isAggro = true;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool GetAggro()
    {
        return isAggro;
    }

    public void SetPlayer(Transform player)
    {
        _player = player;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetStatus()
    {
        ResetAggroStatus();
        _behaviourTree.Reset();
        isDead = false;
        isAttacking = false;    
        hitPoints = maxHealth;
        _healthbarController.ResetBar();
    }

}
