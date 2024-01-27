using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] float promptDistance = 20f;
    [SerializeField] float interractDistance = 3f;
    [SerializeField] LayerMask layerMask;

    public Transform cvc;

    private PlayerController playerController;

    RaycastHit hitInfo;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();   
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hitInfo, promptDistance, layerMask))
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Passerbys"))
            {
                if (hitInfo.distance <= interractDistance)
                {
                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        playerController.BeginConversation(hitInfo.transform.gameObject.GetComponent<PasserbyAI>());
                    }

                    Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.green);
                }
            }
            else
            {
                Debug.Log("That's not a passerby.");
                Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.red);
            }
        }
        else
        {
            Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * promptDistance), Color.yellow);
        }
    }
}
