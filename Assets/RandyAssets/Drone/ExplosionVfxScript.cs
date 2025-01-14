using UnityEngine;

public class ExplosionVfxScript : MonoBehaviour {
    [SerializeField] private float destroyTime = 1f;
    private float timeStamp;

    private void Awake() {
        timeStamp = Time.time;
    }

    private void Update() {
        if(Time.time > timeStamp + destroyTime) {
            Destroy(gameObject);
        }
    }
}