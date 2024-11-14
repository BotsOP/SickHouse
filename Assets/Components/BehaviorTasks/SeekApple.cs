using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SeekApple : Conditional
{
    private Transform[] possibleTargets;
    public SharedTransform target;
    public override TaskStatus OnUpdate()
    {
        GameObject apple = GameObject.FindWithTag("Apple");
        
        if (apple == null)
            return TaskStatus.Failure;

        apple.tag = "TakenApple";
        target.Value = apple.transform;

        return TaskStatus.Success;
    }
}
