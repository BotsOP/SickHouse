using System;
using BehaviorDesigner.Runtime;
using UnityEngine;

[RequireComponent(typeof(BehaviorTree))]
public class Racoon : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5;
    private BehaviorTree behaviorTree;
    private void OnEnable()
    {
        behaviorTree = GetComponent<BehaviorTree>();
        SharedVector3 homePosition = new SharedVector3
        {
            Value = transform.position,
        };
        SharedFloat walkSpeedVariable = new SharedFloat()
        {
            Value = walkSpeed,
        };
        behaviorTree.SetVariable("home", homePosition);
        behaviorTree.SetVariable("walkSpeed", walkSpeedVariable);
    }
}
