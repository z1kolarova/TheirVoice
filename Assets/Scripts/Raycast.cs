using TMPro;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] float interactDistance = 5f;
    [SerializeField] LayerMask layerMask;
    [SerializeField] TMP_Text interactText;

    public Transform cvc;

    RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        if (ConversationManager.I.InDialog)
            return;

        Ray ray = new Ray(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hitInfo, interactDistance, layerMask))
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Passerbys"))
            {
                var passerby = hitInfo.transform.gameObject.GetComponent<PasserbyAI>();
                if (passerby != null && passerby.CanBeApproached())
                {
                    interactText.gameObject.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactText.gameObject.SetActive(false);
                        PlayerController.I.BeginConversation(passerby);
                    }

                    Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.green);
                }
                else
                {
                    Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.red);
                }
            }
            else
            {
                interactText.gameObject.SetActive(false);
                Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * hitInfo.distance), Color.red);
            }
        }
        else
        {
            interactText.gameObject.SetActive(false);
            Debug.DrawRay(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward * interactDistance), Color.yellow);
        }
    }
}
