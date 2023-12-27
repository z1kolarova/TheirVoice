using Assets.Classes;
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class PasserbyAI : MonoBehaviour
{
    private Rigidbody _rb;
    private NavMeshAgent _agent;

    [SerializeField]
    float fieldOfViewDegrees = 90f;
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    Transform target;

    PasserbyStates state = PasserbyStates.WanderingAround;
    Vector3 tempDestination;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case PasserbyStates.WanderingAround:
                WanderAround();
                break;
            case PasserbyStates.Watching:
                break;
            case PasserbyStates.Leaving:
                Leave();
                break;
            default:
                break;
        }
    }

    private void WanderAround()
    {
        if (tempDestination == null || tempDestination.IsApproximately(_rb.position))
        {
            ChooseNewTempDestination();
        }

        _agent.destination = tempDestination;
        
        //MoveTowardsDestination(tempDestination);
    }
    private void Engage()
    {

    }
    private void Leave()
    {
        if (target == null || target.position.IsApproximately(_rb.position))
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

    private void MoveTowardsDestination(Vector3 destination)
    {
        var vectorDistance = destination - _rb.position;
        var vectorMovement = new Vector3(vectorDistance.x, 0, vectorDistance.z).normalized;
        _rb.velocity = vectorMovement * speed;
    }
}
