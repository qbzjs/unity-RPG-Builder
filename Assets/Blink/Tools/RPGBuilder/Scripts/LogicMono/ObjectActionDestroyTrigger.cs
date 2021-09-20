using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectActionDestroyTrigger : StateMachineBehaviour
{
    public GameObject particlePrefab;
    public Vector3 particleOffset;
    public float destroyTime;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject particleGO = Instantiate(particlePrefab, animator.transform.position + particleOffset, Quaternion.identity);
        Destroy(particleGO, destroyTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
