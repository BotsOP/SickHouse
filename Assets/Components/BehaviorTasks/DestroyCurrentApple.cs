using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Managers;
using VHierarchy.Libs;
using EventType = Managers.EventType;

public class DestroyCurrentApple : Action
{
    public SharedTransform target;
    public override TaskStatus OnUpdate()
    {
        target.Value.gameObject.Destroy();
        return TaskStatus.Success;
    }
}
