using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomRigidbody : MonoBehaviour
{
    Rigidbody body;
    float floatDelay = 0;

    [SerializeField]
    bool floatToSleep = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }
    private void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (body.IsSleeping())
            {
                floatDelay = 0;
                return;
            }
            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                return;
            }
            else
            {
                floatDelay = 0f;
            }
        }
        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }
}
