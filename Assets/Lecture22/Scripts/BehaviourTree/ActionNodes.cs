using UnityEngine;
using UnityEngine.AI;

// Встановлює стан FSM на ворогу.
// BT контролює ЩО робити, FSM контролює ЯК (анімації, агент).
public class SetFSMState : BTNode
{
    private readonly Enemy owner;
    private readonly Enemy.EnemyState targetState;

    public SetFSMState(Enemy owner, Enemy.EnemyState state)
    {
        Name = $"Set state → {state}";
        this.owner = owner;
        this.targetState = state;
    }

    public override NodeStatus Tick()
    {
        owner.TransitionTo(targetState);
        return NodeStatus.Success;
    }
}

// Патрулює між точками. Повертає Running поки патрулює.
public class PatrolAction : BTNode
{
    private readonly Enemy owner;
    private readonly NavMeshAgent agent;
    private float waitTimer;
    private bool isWaiting;
    private bool destinationSet;

    public PatrolAction(Enemy owner)
    {
        Name = "Patrol";
        this.owner = owner;
        this.agent = owner.Agent;
    }

    public override NodeStatus Tick()
    {
        owner.TransitionTo(Enemy.EnemyState.Patrol);

        // Перший тік або після Reset — встановити ціль
        if (!destinationSet)
        {
            agent.SetDestination(owner.CurrentPatrolTarget);
            destinationSet = true;
        }

        // Дійшли до точки
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = owner.PatrolWaitTime;
            }

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                owner.NextPatrolPoint();
                agent.SetDestination(owner.CurrentPatrolTarget);
            }
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        isWaiting = false;
        waitTimer = 0;
        destinationSet = false;
    }
}

// Переслідує гравця. Running поки біжить, Success коли достатньо близько.
public class ChaseAction : BTNode
{
    private readonly Enemy owner;
    private readonly NavMeshAgent agent;

    public ChaseAction(Enemy owner)
    {
        Name = "Chase player";
        this.owner = owner;
        this.agent = owner.Agent;
    }

    public override NodeStatus Tick()
    {
        if (owner.Player == null) return NodeStatus.Failure;

        owner.TransitionTo(Enemy.EnemyState.Chase);
        agent.SetDestination(owner.Player.position);

        float dist = Vector3.Distance(owner.transform.position, owner.Player.position);

        // Досягли дистанції атаки
        if (dist <= owner.ShootRange)
            return NodeStatus.Success;

        return NodeStatus.Running;
    }
}

// Стріляє у гравця. Спочатку дістає зброю (DrawTime), потім відкриває вогонь.
// Повертає Running поки гравець у зоні стрільби.
public class ShootAction : BTNode
{
    private readonly Enemy owner;
    private float lastFireTime;
    private bool weaponReady;
    private float drawTimer;
    private const float DrawTime = 0.75f;

    public ShootAction(Enemy owner)
    {
        Name = "Shoot";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (owner.Player == null) return NodeStatus.Failure;

        owner.TransitionTo(Enemy.EnemyState.RangedAttack);
        owner.FaceTarget(owner.Player.position);

        // Якщо зброю сховали (перехід до Chase/Patrol) — скидаємо готовність
        if (!owner.IsWeaponDrawn)
        {
            weaponReady = false;
            drawTimer = 0f;
        }

        // Фаза діставання зброї
        if (!weaponReady)
        {
            if (drawTimer == 0f)
                owner.DrawWeapon("BlasterWeapon");

            drawTimer += Time.deltaTime;

            if (drawTimer >= DrawTime)
                weaponReady = true;

            return NodeStatus.Running;
        }

        // Фаза стрільби
        if (Time.time - lastFireTime >= owner.FireRate)
        {
            owner.PerformShoot();
            lastFireTime = Time.time;
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        weaponReady = false;
        drawTimer = 0f;
    }
}

// Атакує у ближньому бою. Running поки гравець у зоні мілі.
public class MeleeAction : BTNode
{
    private readonly Enemy owner;
    private float lastMeleeTime;
    private bool weaponReady;
    private float drawTimer;
    private const float DrawTime = 0.75f;

    public MeleeAction(Enemy owner)
    {
        Name = "Melee attack";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (owner.Player == null) return NodeStatus.Failure;

        owner.TransitionTo(Enemy.EnemyState.MeleeAttack);
        owner.FaceTarget(owner.Player.position);

        // Якщо зброю сховали (перехід до Chase/Patrol) — скидаємо готовність
        if (!owner.IsWeaponDrawn)
        {
            weaponReady = false;
            drawTimer = 0f;
        }

        // Фаза діставання зброї
        if (!weaponReady)
        {
            if (drawTimer == 0f)
                owner.DrawWeapon("MeleeWeapon");

            drawTimer += Time.deltaTime;

            if (drawTimer >= DrawTime)
                weaponReady = true;

            return NodeStatus.Running;
        }

        if (Time.time - lastMeleeTime >= owner.MeleeRate)
        {
            owner.PerformMelee();
            lastMeleeTime = Time.time;
        }

        return NodeStatus.Running;
    }
    public override void Reset()
    {
        weaponReady = false;
        drawTimer = 0f;
    }
}

// Шукає гравця в останній відомій позиції. 
// Running → крутиться і чекає. Failure → час вийшов.
public class SearchAction : BTNode
{
    private readonly Enemy owner;
    private readonly NavMeshAgent agent;
    private float searchTimer;
    private bool initialized;

    public SearchAction(Enemy owner)
    {
        Name = "Search area";
        this.owner = owner;
        this.agent = owner.Agent;
    }

    public override NodeStatus Tick()
    {
        owner.TransitionTo(Enemy.EnemyState.Search);

        if (!initialized)
        {
            searchTimer = owner.SearchDuration;
            Vector3 lastPos = blackboard.Get("lastKnownPos", owner.transform.position);
            agent.SetDestination(lastPos);
            initialized = true;
        }

        searchTimer -= Time.deltaTime;

        // Крутимось на місці коли дійшли
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            owner.transform.Rotate(0, 120f * Time.deltaTime, 0);
        }

        if (searchTimer <= 0f)
        {
            initialized = false;
            blackboard.Set("playerDetected", false);
            owner.ClearHitFlag();
            return NodeStatus.Failure; // Не знайшли → Selector спробує наступне
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        initialized = false;
        searchTimer = 0;
    }
}

// Реакція на отримання шкоди. Програє анімацію і скидає прапорець.
public class TakeDamageAction : BTNode
{
    private readonly Enemy owner;
    private bool started;
    private bool enteredHitState;
    private int lastHitCount;
    private float timeout;

    private const string HitStateName = "Take Damage";
    private const int HitLayerIndex = 0;
    private const float MaxWaitTime = 1.5f;

    public TakeDamageAction(Enemy owner)
    {
        Name = "Take damage reaction";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (!started)
        {
            if (owner.CurrentFSMState == Enemy.EnemyState.TakeDamage)
                owner.ReTriggerHit(); // FSM вже TakeDamage — просто перезапускаємо тригер
            else
                owner.TransitionTo(Enemy.EnemyState.TakeDamage);

            lastHitCount = owner.HitCount;
            started = true;
            enteredHitState = false;
            timeout = MaxWaitTime;
            return NodeStatus.Running;
        }

        // Нове попадання поки анімація ще грає — перезапускаємо
        if (owner.HitCount != lastHitCount)
        {
            owner.ReTriggerHit();
            lastHitCount = owner.HitCount;
            enteredHitState = false;
            timeout = MaxWaitTime;
            return NodeStatus.Running;
        }

        bool inHitState = owner.EnemyAnimator
            .GetCurrentAnimatorStateInfo(HitLayerIndex)
            .IsName(HitStateName);
        bool isBlending = owner.EnemyAnimator.IsInTransition(HitLayerIndex);

        if (!enteredHitState && inHitState)
            enteredHitState = true;

        // Чекаємо поки вийшли зі стейту І blend повністю завершився
        if (enteredHitState && !inHitState && !isBlending)
        {
            owner.ClearHitFlag();
            started = false;
            return NodeStatus.Success;
        }

        // Fallback: анімація не відтворилась коректно — завершуємо після таймауту
        timeout -= Time.deltaTime;
        if (timeout <= 0f)
        {
            owner.ClearHitFlag();
            started = false;
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        started = false;
        enteredHitState = false;
    }
}

// Смерть ворога. Програє анімацію та знищує об'єкт.
public class DeathAction : BTNode
{
    private readonly Enemy owner;
    private bool started;

    public DeathAction(Enemy owner)
    {
        Name = "Die";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (!started)
        {
            owner.TransitionTo(Enemy.EnemyState.Death);
            started = true;
            return NodeStatus.Running;
        }

        var info = owner.EnemyAnimator.GetCurrentAnimatorStateInfo(0);
        bool animationFinished = info.IsName("Die") && info.normalizedTime >= 0.75f;

        if (animationFinished)
        {
            owner.PerformDeath();
            started = false;
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }
}

// Рухається до позиції з Blackboard.
public class MoveToPosition : BTNode
{
    private readonly Enemy owner;
    private readonly NavMeshAgent agent;
    private readonly string positionKey;

    public MoveToPosition(Enemy owner, string blackboardKey = "lastKnownPos")
    {
        Name = $"Move to [{blackboardKey}]";
        this.owner = owner;
        this.agent = owner.Agent;
        this.positionKey = blackboardKey;
    }

    public override NodeStatus Tick()
    {
        Vector3 target = blackboard.Get(positionKey, owner.transform.position);
        agent.SetDestination(target);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            return NodeStatus.Success;

        return NodeStatus.Running;
    }
}