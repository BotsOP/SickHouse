using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Managers;
using EventType = Managers.EventType;

public class DestroyCurrentApple : Action
{
    public SharedTransform target;
    public override TaskStatus OnUpdate()
    {
        EventSystem<int, Vector3>.RaiseEvent(EventType.GAIN_APPLES, 1, transform.position);
        EventSystem<int>.RaiseEvent(EventType.COLLECTED_APPLE, target.Value.GetComponent<Apple>().treeIndex);
        EventSystem<GameObject>.RaiseEvent(EventType.DESTROY_OBJECT, target.Value.gameObject);
        return TaskStatus.Success;
    }
}
