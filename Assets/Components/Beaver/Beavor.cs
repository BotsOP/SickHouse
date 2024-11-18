using System;
using BehaviorDesigner.Runtime;
using UnityEngine;
using Object = System.Object;

[RequireComponent(typeof(BehaviorTree))]
public class Beavor : MonoBehaviour
{
    public GridManager gridManager;
    private BehaviorTree behaviorTree;
    private void OnEnable()
    {
        
    }
}
