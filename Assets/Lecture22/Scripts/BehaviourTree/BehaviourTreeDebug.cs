using UnityEngine;

// ═════════════════════════════════════════════════════════════
//  BT DEBUGGER — візуалізація дерева у консолі (опціонально)
// ═════════════════════════════════════════════════════════════

/// <summary>
/// Обгортка для BT-ноди, яка логує виконання у консоль Unity.
/// Корисно для дебагу: бачиш яка гілка активна кожен кадр.
/// 
/// Використання: замість ноди обгорніть її у DebugNode:
///   new DebugNode(new CheckPlayerVisible(this))
/// </summary>
public class DebugNode : BTNode
{
    private readonly BTNode child;
    private readonly bool logEveryTick;

    public DebugNode(BTNode child, bool logEveryTick = false)
    {
        Name = $"[DBG] {child.Name}";
        this.child = child;
        this.logEveryTick = logEveryTick;
    }

    public override NodeStatus Tick()
    {
        child.SetBlackboard(blackboard);
        NodeStatus status = child.Tick();

        if (logEveryTick || status != NodeStatus.Running)
        {
            string color = status switch
            {
                NodeStatus.Success => "green",
                NodeStatus.Failure => "red",
                _ => "yellow"
            };
            Debug.Log($"<color={color}>[BT] {child.Name} → {status}</color>");
        }

        return status;
    }

    public override void Reset()
    {
        child.Reset();
    }
}

/// <summary>
/// Компонент для відображення стану BT у OnGUI (для прототипування).
/// Прикріпіть до ворога поруч з EnemyAI_BT.
/// </summary>
public class BTDebugOverlay : MonoBehaviour
{
    private Enemy enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<Enemy>();
    }

    private void OnGUI()
    {
        if (enemyAI == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);

        if (screenPos.z <= 0) return;

        float x = screenPos.x - 75;
        float y = Screen.height - screenPos.y - 20;

        GUI.color = GetStateColor(enemyAI.CurrentFSMState);
        GUI.Label(new Rect(x, y, 150, 25),
            $"[{enemyAI.CurrentFSMState}] HP:{enemyAI.CurrentHealth}",
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 12
            });
    }

    private Color GetStateColor(Enemy.EnemyState state)
    {
        return state switch
        {
            Enemy.EnemyState.Patrol => Color.green,
            Enemy.EnemyState.Chase => Color.yellow,
            Enemy.EnemyState.RangedAttack => Color.red,
            Enemy.EnemyState.MeleeAttack => new Color(1f, 0.4f, 0f),
            Enemy.EnemyState.Search => Color.cyan,
            Enemy.EnemyState.TakeDamage => Color.magenta,
            Enemy.EnemyState.Death => Color.gray,
            _ => Color.white
        };
    }
}