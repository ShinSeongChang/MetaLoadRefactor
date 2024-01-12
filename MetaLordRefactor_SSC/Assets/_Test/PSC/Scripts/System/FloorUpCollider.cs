using System.Collections;
using UnityEngine;

public class FloorUpCollider : MonoBehaviour
{
    [SerializeField]
    LayerMask layerMask;


    private void OnTriggerEnter(Collider other)
    {

        Rigidbody rb = other.GetComponent <Rigidbody>();

        if (rb)
        {
            
            rb.velocity = Vector3.zero;
            Vector2 random = Random.insideUnitCircle.normalized;
            rb.AddForce(Vector3.up* 20, ForceMode.VelocityChange);
            rb.AddForce(new Vector3(random.x, 0, random.y) , ForceMode.VelocityChange);
            StartCoroutine(ResetLayerRoutine(other));
        }
    }
    private void OnTriggerStay(Collider other)
    {

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb)
        {

            rb.velocity = Vector3.zero;
            Vector2 random = Random.insideUnitCircle.normalized;
            rb.AddForce(Vector3.up * 20, ForceMode.VelocityChange);
            rb.AddForce(new Vector3(random.x, 0, random.y), ForceMode.VelocityChange);
            StartCoroutine(ResetLayerRoutine(other));
        }
    }

    IEnumerator ResetLayerRoutine(Collider other)
    {
        yield return new WaitForSeconds(1);
        other.excludeLayers = 0;
    }
}
