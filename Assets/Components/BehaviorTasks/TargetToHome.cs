using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class TargetToHome : Action
{
    public SharedFloat speed = 0;
    public SharedVector3 home;
    public override TaskStatus OnUpdate()
    {
        if (Vector3.SqrMagnitude(transform.position -
                                 home.Value) < 0.1f) {
            return TaskStatus.Success;
        }
        transform.position = Vector3.MoveTowards(transform.position,
                                                 home.Value, speed.Value * Time.deltaTime);
        transform.LookAt(home.Value);
        return TaskStatus.Running;
    }
}
