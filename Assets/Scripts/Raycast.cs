using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] float promptDistance = 20f;
    [SerializeField] float interractDistance = 3f;
    [SerializeField] LayerMask layerMask;

    public Transform cvc;

    RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        if (ConversationManager.I.InDialog)
            return;

        Ray ray = new Ray(cvc.transform.position, cvc.transform.TransformDirection(Vector3.forward));

        if (Physics.Raycast(ray, out hitInfo, promptDistance, layerMask))
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Passerbys"))
            {
                if (hitInfo.distance <= interractDistance)
                {
                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        PlayerController.I.BeginConversation(hitInfo.transform.gameObject.GetComponent<PasserbyAI>());
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
