using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchAnimationSetter : MonoBehaviour
{
    [SerializeField] FloatVariable crouchPercentage;
    [SerializeField] Animator animator;

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Blend", 1 - Mathf.InverseLerp(0.3f, 0.8f, crouchPercentage.value));
    }
}
