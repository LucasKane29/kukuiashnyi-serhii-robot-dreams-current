using System.Collections.Generic;
using UnityEngine;

// ═════════════════════════════════════════════════════════════
//  BEHAVIOUR TREE — БАЗОВІ КЛАСИ
// ═════════════════════════════════════════════════════════════

/// <summary>
/// Результат виконання ноди дерева поведінки.
/// </summary>
public enum NodeStatus
{
    Running,  // Виконується
    Success,  // Успіх
    Failure   // Невдача
}

/// <summary>
/// Спільний контекст (Blackboard) — дані, які ноди читають і записують.
/// Замість передачі параметрів між нодами, всі дані зберігаються тут.
/// </summary>
public class Blackboard
{
    private readonly Dictionary<string, object> data = new();

    public void Set<T>(string key, T value) => data[key] = value;

    public T Get<T>(string key, T fallback = default)
    {
        if (data.TryGetValue(key, out object val) && val is T typed)
            return typed;
        return fallback;
    }

    public bool Has(string key) => data.ContainsKey(key);
    public void Remove(string key) => data.Remove(key);
}

// ─────────────────────────────────────────────────────────────
//  Базова нода
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Абстрактна базова нода дерева поведінки.
/// </summary>
public abstract class BTNode
{
    public string Name { get; set; }
    protected Blackboard blackboard;

    public void SetBlackboard(Blackboard bb) => blackboard = bb;

    /// <summary>Виконати ноду і повернути статус.</summary>
    public abstract NodeStatus Tick();

    /// <summary>Скидання стану ноди (для переривання Running).</summary>
    public virtual void Reset() { }
}

// ─────────────────────────────────────────────────────────────
//  Композитні ноди
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Selector (Fallback) — виконує дітей по черзі, повертає Success
/// при першому успіху. Аналог логічного OR.
/// 
/// Використання: вибір між альтернативними поведінками.
/// Наприклад: [Атакувати] OR [Переслідувати] OR [Патрулювати]
/// </summary>
public class Selector : BTNode
{
    private readonly List<BTNode> children = new();
    private int currentChild;

    public Selector(string name, params BTNode[] nodes)
    {
        Name = name;
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        for (int i = currentChild; i < children.Count; i++)
        {
            children[i].SetBlackboard(blackboard);
            NodeStatus status = children[i].Tick();

            switch (status)
            {
                case NodeStatus.Running:
                    currentChild = i;
                    return NodeStatus.Running;

                case NodeStatus.Success:
                    currentChild = 0;
                    return NodeStatus.Success;
            }
            // Failure → спробувати наступну дитину
        }

        currentChild = 0;
        return NodeStatus.Failure;
    }

    public override void Reset()
    {
        currentChild = 0;
        foreach (var child in children) child.Reset();
    }
}

public class PrioritySelector : BTNode
{
    private readonly List<BTNode> children = new();
    private int currentChild;

    public PrioritySelector(string name, params BTNode[] nodes)
    {
        Name = name;
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetBlackboard(blackboard);
            NodeStatus status = children[i].Tick();

            switch (status)
            {
                case NodeStatus.Running:
                    if (currentChild != i)// перемикання на іншого дочірнього
                        children[currentChild].Reset();
                    currentChild = i;
                    return NodeStatus.Running;

                case NodeStatus.Success:
                    currentChild = 0;
                    return NodeStatus.Success;
            }
            // Failure → спробувати наступну дитину
        }

        currentChild = 0;
        return NodeStatus.Failure;
    }

    public override void Reset()
    {
        currentChild = 0;
        foreach (var child in children) child.Reset();
    }
}

/// <summary>
/// Sequence — виконує дітей по черзі, повертає Failure при першій
/// невдачі. Аналог логічного AND.
/// 
/// Використання: послідовність умов і дій.
/// Наприклад: [Бачу гравця?] AND [Достатньо близько?] AND [Стріляти]
/// </summary>
public class Sequence : BTNode
{
    private readonly List<BTNode> children = new();
    private int currentChild;

    public Sequence(string name, params BTNode[] nodes)
    {
        Name = name;
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        for (int i = currentChild; i < children.Count; i++)
        {
            children[i].SetBlackboard(blackboard);
            NodeStatus status = children[i].Tick();

            switch (status)
            {
                case NodeStatus.Running:
                    currentChild = i;
                    return NodeStatus.Running;

                case NodeStatus.Failure:
                    currentChild = 0;
                    return NodeStatus.Failure;
            }
            // Success → перейти до наступної дитини
        }

        currentChild = 0;
        return NodeStatus.Success;
    }
    public override void Reset()
    {
        currentChild = 0;
        foreach (var child in children) child.Reset();
    }
}

public class ReactiveSequence : BTNode
{
    private readonly List<BTNode> children = new();
    private int currentChild;

    public ReactiveSequence(string name, params BTNode[] nodes)
    {
        Name = name;
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetBlackboard(blackboard);
            NodeStatus status = children[i].Tick();

            switch (status)
            {
                case NodeStatus.Running:
                    return NodeStatus.Running;

                case NodeStatus.Failure:
                    return NodeStatus.Failure;
            }
            // Success → перейти до наступної дитини
        }

        currentChild = 0;
        return NodeStatus.Success;
    }

    public override void Reset()
    {
        currentChild = 0;
        foreach (var child in children) child.Reset();
    }
}

/// <summary>
/// Parallel — виконує ВСІХ дітей одночасно кожен тік.
/// Повертає Success коли мінімум successThreshold дітей успішні.
/// Повертає Failure коли успіх вже неможливий.
/// 
/// Використання: одночасні дії (рухатись І стріляти).
/// </summary>
public class Parallel : BTNode
{
    private readonly List<BTNode> children = new();
    private readonly int successThreshold;

    public Parallel(string name, int successThreshold, params BTNode[] nodes)
    {
        Name = name;
        this.successThreshold = successThreshold;
        children.AddRange(nodes);
    }

    public override NodeStatus Tick()
    {
        int successCount = 0;
        int failureCount = 0;

        foreach (var child in children)
        {
            child.SetBlackboard(blackboard);
            NodeStatus status = child.Tick();

            if (status == NodeStatus.Success) successCount++;
            else if (status == NodeStatus.Failure) failureCount++;
        }

        if (successCount >= successThreshold)
            return NodeStatus.Success;

        if (failureCount > children.Count - successThreshold)
            return NodeStatus.Failure;

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        foreach (var child in children) child.Reset();
    }
}

// ─────────────────────────────────────────────────────────────
//  Декоратори
// ─────────────────────────────────────────────────────────────

/// <summary>
/// Inverter — інвертує результат дитини.
/// Success → Failure, Failure → Success, Running → Running.
/// </summary>
public class Inverter : BTNode
{
    private readonly BTNode child;

    public Inverter(string name, BTNode child)
    {
        Name = name;
        this.child = child;
    }

    public override NodeStatus Tick()
    {
        child.SetBlackboard(blackboard);
        NodeStatus status = child.Tick();

        return status switch
        {
            NodeStatus.Success => NodeStatus.Failure,
            NodeStatus.Failure => NodeStatus.Success,
            _ => NodeStatus.Running
        };
    }

    public override void Reset() => child.Reset();
}

/// <summary>
/// Repeater — повторює дитину задану кількість разів.
/// -1 = нескінченно (поки не буде Failure).
/// </summary>
public class Repeater : BTNode
{
    private readonly BTNode child;
    private readonly int maxRepeats;
    private int currentCount;

    public Repeater(string name, BTNode child, int maxRepeats = -1)
    {
        Name = name;
        this.child = child;
        this.maxRepeats = maxRepeats;
    }

    public override NodeStatus Tick()
    {
        child.SetBlackboard(blackboard);
        NodeStatus status = child.Tick();

        if (status == NodeStatus.Running)
            return NodeStatus.Running;

        if (status == NodeStatus.Failure)
            return NodeStatus.Failure;

        currentCount++;
        if (maxRepeats > 0 && currentCount >= maxRepeats)
        {
            currentCount = 0;
            return NodeStatus.Success;
        }

        return NodeStatus.Running;
    }

    public override void Reset()
    {
        currentCount = 0;
        child.Reset();
    }
}

/// <summary>
/// Cooldown — блокує виконання дитини на заданий час після успіху.
/// Корисно для обмеження частоти атак тощо.
/// </summary>
public class Cooldown : BTNode
{
    private readonly BTNode child;
    private readonly float cooldownTime;
    private float lastSuccessTime = -Mathf.Infinity;

    public Cooldown(string name, BTNode child, float cooldownTime)
    {
        Name = name;
        this.child = child;
        this.cooldownTime = cooldownTime;
    }

    public override NodeStatus Tick()
    {
        if (Time.time - lastSuccessTime < cooldownTime)
            return NodeStatus.Failure;

        child.SetBlackboard(blackboard);
        NodeStatus status = child.Tick();

        if (status == NodeStatus.Success)
            lastSuccessTime = Time.time;

        return status;
    }

    public override void Reset()
    {
        lastSuccessTime = -Mathf.Infinity;
        child.Reset();
    }
}