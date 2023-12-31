using Assets.Classes;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AISensor))]
public class PasserbyAI : MonoBehaviour
{
    private Rigidbody _rb;
    private NavMeshAgent _agent;
    private AISensor _sensor;

    [SerializeField]
    Transform target;

    PasserbyStates state = PasserbyStates.WanderingAround;
    Vector3 tempDestination;
    GameObject watchedObject;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _sensor = GetComponent<AISensor>();
    }

    // Update is called once per frame
    void Update()
    {
        EvaluateStateChange();
        switch (state)
        {
            case PasserbyStates.WanderingAround:
                WanderAround();
                break;
            case PasserbyStates.Watching:
                ReactToSeenObject(watchedObject);
                break;
            case PasserbyStates.Leaving:
                Leave();
                break;
            default:
                break;
        }
    }

    private void EvaluateStateChange()
    {
        switch (state)
        {
            case PasserbyStates.WanderingAround:
                if (_sensor.objects.Any())
                {
                    ReactToSeenObject(_sensor.objects[0]);
                }
                break;
            case PasserbyStates.Watching:
                break;
            case PasserbyStates.Leaving:
                break;
            default:
                break;
        }
    }

    private void WanderAround()
    {
        if (tempDestination.IsNullOrBegining() || tempDestination.IsApproximately(_rb.position))
        {
            ChooseNewTempDestination();
            _agent.destination = tempDestination;
        }
    }
    private void ReactToSeenObject(GameObject gameObject)
    {
        var layerName = LayerMask.LayerToName(gameObject.layer);
        switch (layerName)
        {
            case "Outreachers":
                break;

            case "Cubers":
                StopAndTurnTowards(gameObject);

                watchedObject = gameObject;
                state = PasserbyStates.Watching;

                break;

            default:
                break;
        }        
    }
    private void Engage()
    {

    }
    private void Leave()
    {
        if (target != null && !target.position.IsApproximately(_agent.transform.position))
        {
            _agent.destination = target.position;
            return;
        }

        //MoveTowardsDestination(target.position);
    }

    private void ChooseNewTempDestination()
    {
        var v2 = UnityEngine.Random.insideUnitCircle.ProjectInto(Utilities.Borders);
        tempDestination = new Vector3(v2.x, _rb.position.y, v2.y);
    }

    private void StopAndTurnTowards(Vector3 position)
    {
        _agent.isStopped = true;

        Vector3 direction = position - _agent.transform.position;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        _agent.transform.rotation = Quaternion.RotateTowards(_agent.transform.rotation, targetRotation, _agent.angularSpeed * Time.deltaTime);
    }
    private void StopAndTurnTowards(GameObject gameObject) => StopAndTurnTowards(gameObject.transform.position);
}
