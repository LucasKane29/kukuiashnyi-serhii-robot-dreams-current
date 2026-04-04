using System.Collections.Generic;

namespace BehaviourTrees
{
    public class Node
    {
        private readonly string _name;

        protected readonly List<Node> _children = new();

        protected int _currentChild = 0;
        public enum Status
        {
            Running,
            Success,
            Failure
        }

        public Node(string name = "Node")
        {
            _name = name;
        }

        public void AddChild(Node child)
        {
            _children.Add(child);
        }
        public virtual Status Process()
        {
            return _children[_currentChild].Process();
        }

        public virtual void Reset()
        {
            _currentChild = 0;
            foreach (var _child in _children)
                _child.Reset();
        }
    }

    public interface IStrategy
    {
        Node.Status Process();
        void Reset();
    }

    public class Leaf : Node
    {
        private readonly IStrategy strategy;
        public Leaf(string name, IStrategy strategy) : base(name)
        {
            this.strategy = strategy;
        }
        public override Status Process()
        {
            return strategy.Process();
        }

        public override void Reset()
        {
            strategy.Reset();
        }
    }

    public class SelectorNode : Node
    {
        public SelectorNode(List<Node> children)
        {
            foreach (var _child in children)
                _children.Add(_child);
        }
        public override Status Process()
        {
            _currentChild = 0;
            while (_currentChild < _children.Count)
            {
                var status = _children[_currentChild].Process();
                if (status == Status.Running || status == Status.Success)
                    return status;
                _currentChild++;
            }
            Reset();
            return Status.Failure;
        }
    }

    public class SequenceNode : Node
    {
        public SequenceNode(string name = "Sequence") : base(name) { }
        public override Status Process()
        {
            _currentChild = 0;
            while (_currentChild < _children.Count)
            {
                var status = _children[_currentChild].Process();
                switch (status)
                {
                    case Status.Running:
                        return status;
                    case Status.Failure:
                        Reset();
                        return status;
                    default:
                        _currentChild++;
                        break;
                }
            }
            Reset();
            return Status.Success;
        }
    }

    public class ActionNode : Node
    {
        public System.Func<Status> action;
        public ActionNode(System.Func<Status> action, string name = "ActionNode") : base(name)
        {
            this.action = action;
        }
        public override Status Process()
        {
            return action();
        }
    }

    public class ConditionNode : Node
    {
        public System.Func<bool> condition;
        public ConditionNode(System.Func<bool> condition, string name = "ConditionNode") : base(name)
        {
            this.condition = condition;
        }
        public override Status Process()
        {
            return condition() ? Status.Success : Status.Failure;
        }
    }

    public class BehaviourTree : Node
    {
        public BehaviourTree(string name = "BehaviourTree") : base(name) { }
        public override Status Process()
        {
            while (_currentChild < _children.Count)
            {
                var status = _children[_currentChild].Process();
                if (status != Status.Success)
                {
                    return status;
                }
                _currentChild++;
            }
            Reset();
            return Status.Success;
        }
    }
}
