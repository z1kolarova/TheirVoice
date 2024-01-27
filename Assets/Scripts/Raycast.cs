using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] float promptDistance = 20f;
    [SerializeField] float interractDistance = 5f;
    [SerializeField] LayerMask layerMask;

    public Transform cvc;

    RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hitInfo, promptDistance, layerMask))
        {
            Debug.Log("Hit something.");
            if (hitInfo.distance <= interractDistance)
            {
                Debug.Log("I am close enough to interract.");
                Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.green);
            }
            else
                Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.red);
        }
        else
        {
            Debug.Log("Hit nothing.");
            Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * promptDistance), Color.yellow);
        }
    }
}
