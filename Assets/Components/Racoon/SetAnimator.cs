using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[System.Serializable]
public class SharedAnimator : SharedVariable<Animator>
{
    public static implicit operator SharedAnimator(Animator value) { return new SharedAnimator { Value = value }; }
}

public class SetAnimator : Action
{
    public SharedAnimator sharedAnimator;
    public string parameterName;
    public float value;
    
    public override TaskStatus OnUpdate()
    {
        sharedAnimator.Value.SetFloat(parameterName, value);
        return TaskStatus.Success;
    }
}
