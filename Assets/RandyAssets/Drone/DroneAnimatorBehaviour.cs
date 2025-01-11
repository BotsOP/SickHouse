using System.Collections.Generic;
using UnityEngine;

public class DroneAnimatorBehaviour : MonoBehaviour {
    [field: SerializeField] private List<Animator> animators { get; set; }
    [field: SerializeField] private string idleTrigger { get; set; }
    [field: SerializeField] private string moveTrigger { get; set; }

    private void Awake() {
        //DoIdle();
        DoMove(); //if you want to start in move state.
    }

    public void DoIdle() {
        SetAnimatorTriggers(idleTrigger);
    }

    public void DoMove() {
        SetAnimatorTriggers(moveTrigger);
    }

    private void SetAnimatorTriggers(string trigger) {
        foreach (var animator in animators) {
            animator.SetTrigger(trigger);
        }
    }
}