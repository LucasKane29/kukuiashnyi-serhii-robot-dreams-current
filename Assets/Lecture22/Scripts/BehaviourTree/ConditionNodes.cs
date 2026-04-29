using UnityEngine;

// ═════════════════════════════════════════════════════════════
//  CONDITION NODES — перевірки, що повертають Success / Failure
// ═════════════════════════════════════════════════════════════

/// <summary>
/// Перевіряє, чи ворог бачить гравця (Raycast + кут зору).
/// Якщо бачить — записує позицію гравця на Blackboard.
/// </summary>
public class CheckPlayerVisible : BTNode
{
    private readonly Enemy owner;

    public CheckPlayerVisible(Enemy owner)
    {
        Name = "Player visible?";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (owner.Player == null) return NodeStatus.Failure;
        if (owner.CanSeePlayer())
        {
            blackboard.Set("lastKnownPos", owner.Player.position);
            blackboard.Set("playerDetected", true);
            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє, чи ворог отримав шкоду (wasHit flag).
/// Записує позицію гравця як джерело шкоди.
/// </summary>
public class CheckWasHit : BTNode
{
    private readonly Enemy owner;

    public CheckWasHit(Enemy owner)
    {
        Name = "Was hit?";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (owner.WasHitByPlayer)
        {
            if (owner.Player != null)
                blackboard.Set("lastKnownPos", owner.Player.position);

            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє, чи гравець у межах заданої дистанції.
/// </summary>
public class CheckDistanceToPlayer : BTNode
{
    private readonly Enemy owner;
    private readonly float maxDistance;
    private readonly bool lessThan; // true = dist < max, false = dist > max

    public CheckDistanceToPlayer(Enemy owner, float maxDistance, bool lessThan = true)
    {
        Name = lessThan ? $"Dist < {maxDistance}?" : $"Dist > {maxDistance}?";
        this.owner = owner;
        this.maxDistance = maxDistance;
        this.lessThan = lessThan;
    }

    public override NodeStatus Tick()
    {
        if (owner.Player == null) return NodeStatus.Failure;

        float dist = Vector3.Distance(owner.transform.position, owner.Player.position);
        blackboard.Set("distToPlayer", dist);

        bool result = lessThan ? dist <= maxDistance : dist > maxDistance;
        return result ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє, чи ворог живий.
/// </summary>
public class CheckIsAlive : BTNode
{
    private readonly Enemy owner;

    public CheckIsAlive(Enemy owner)
    {
        Name = "Is alive?";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        return owner.CurrentHealth > 0 ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє, чи здоров'я нижче порогу (для зміни тактики).
/// </summary>
public class CheckHealthBelow : BTNode
{
    private readonly Enemy owner;
    private readonly float threshold;

    public CheckHealthBelow(Enemy owner, float threshold)
    {
        Name = $"HP < {threshold}%?";
        this.owner = owner;
        this.threshold = threshold;
    }

    public override NodeStatus Tick()
    {
        float ratio = owner.CurrentHealth / owner.MaxHealth;
        return ratio < threshold ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє значення на Blackboard.
/// </summary>
public class CheckBlackboard : BTNode
{
    private readonly string key;
    private readonly bool expectedValue;

    public CheckBlackboard(string key, bool expectedValue = true)
    {
        Name = $"BB[{key}] == {expectedValue}?";
        this.key = key;
        this.expectedValue = expectedValue;
    }

    public override NodeStatus Tick()
    {
        bool val = blackboard.Get(key, false);
        return val == expectedValue ? NodeStatus.Success : NodeStatus.Failure;
    }
}

/// <summary>
/// Перевіряє, чи ворог чує звук (постріл гравця поблизу).
/// </summary>
public class CheckHeardSound : BTNode
{
    private readonly Enemy owner;

    public CheckHeardSound(Enemy owner)
    {
        Name = "Heard sound?";
        this.owner = owner;
    }

    public override NodeStatus Tick()
    {
        if (owner.HeardSoundPosition.HasValue)
        {
            blackboard.Set("lastKnownPos", owner.HeardSoundPosition.Value);
            return NodeStatus.Success;
        }
        return NodeStatus.Failure;
    }
}