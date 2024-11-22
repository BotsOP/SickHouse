using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class GoToTargetPosition : Action
{
    public SharedFloat speed = 0;
    public SharedVector3 target;
    public override TaskStatus OnUpdate()
    {
        if (Vector3.SqrMagnitude(transform.position -
                                 target.Value) < 0.1f) {
            return TaskStatus.Success;
        }
        transform.position = Vector3.MoveTowards(transform.position,
                                                 target.Value, speed.Value * Time.deltaTime);
        transform.LookAt(target.Value);
        return TaskStatus.Running;
    }
}
