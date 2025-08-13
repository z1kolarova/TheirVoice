using Assets.Classes;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

enum AnimationType {
    Idle,
    Walk
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AISensor))]
public class PasserbyAI : MonoBehaviour
{
    private Rigidbody _rb;
    private NavMeshAgent _agent;
    private AISensor _sensor;

    public Transform target;
    public PersonalityCore personality;
    
    Animator animator;

    [SerializeField] Transform speechBubbleParent;
    public TMP_Text textMesh;
    public Animator speechBubbleAnimator;

    PasserbyStates state = PasserbyStates.WanderingAround;
    public PasserbyStates State { get { return state; } }

    private GameObject npcModel;
    Vector3 tempDestination;
    GameObject watchedObject;

    bool isGettingApproached = false;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _sensor = GetComponent<AISensor>();

        // load model
        npcModel = PasserbyModelManager.I.GetRandomModel();
        var model = GameObject.Instantiate(npcModel, this.transform);
        model.transform.localPosition = new Vector3(0, -1.1f, 0);
        animator = model.GetComponent<Animator>();
        animator.runtimeAnimatorController = PasserbyModelManager.I.animatorController;
        animator.SetTrigger("StartWalking");

        personality = PersonalityGenerator.I.GetNewPersonality();
        CompletePersonality();
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
            case PasserbyStates.InConversation:
                ReactToSeenObject(watchedObject);
                break;
            case PasserbyStates.Leaving:
                Leave();
                break;
            default:
                break;
        }
    }

    #region Personality

    public void CompletePersonality()
    {
        StartCoroutine(GetPromptLocText(personality.Prompt.Id, UserSettingsManager.I.ConversationLanguage.Id));
    }

    private IEnumerator GetPromptLocText(int promptId, int langId)
    {
        Debug.Log($"start of GetPromptLocText {promptId}, {langId}");
        var idTuple = (promptId, langId);
        if (!ClientDataManager.I.PromptLocTextDic.TryGetValue(idTuple, out string promptLocText))
        {
            if (!ClientDataUtils.PromptLocRequester.IsCurrentlyWaiting(idTuple))
            {
                Debug.Log($"calls GetPromptLoc for {promptId}, {langId}");
                ClientDataUtils.PromptLocRequester.RequestData(idTuple);
            }
            yield return new WaitWhile(() => { 
                return ClientDataUtils.PromptLocRequester.IsCurrentlyWaiting(idTuple);
            });
            ClientDataManager.I.PromptLocTextDic.TryGetValue(idTuple, out promptLocText);
        }

        personality.EnrichedText = promptLocText.ProduceEnrichedLocText(personality.Prompt.EndConvoAbility, PersonalityGenerator.I.EndingConversationAbilityChance);
    }
    #endregion Personality

    private void EvaluateStateChange()
    {
        switch (state)
        {
            case PasserbyStates.WanderingAround:
                if (!isGettingApproached && _sensor.objects.Any())
                {
                    StartWatchingObject(_sensor.objects[0]);
                }
                break;
            case PasserbyStates.Watching:
                break;
            case PasserbyStates.InConversation:
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

    private void StartWatchingObject(GameObject gameObject)
    {
        watchedObject = gameObject;
        if (state != PasserbyStates.Watching)
        {
            state = PasserbyStates.Watching;
            animator.SetTrigger("GoIdle");
        }
    }

    private void ReactToSeenObject(GameObject gameObject)
    {    
        StopAndTurnTowards(gameObject);
    }

    public bool CanBeApproached() => State != PasserbyStates.Leaving && State != PasserbyStates.InConversation;

    public void BeApproached(GameObject player)
    {
        isGettingApproached = true;
        EngageWith(player);
    }

    public void EndConversation()
    {
        state = PasserbyStates.Leaving;
        _agent.isStopped = false;
        animator.SetTrigger("StartWalking");
    }

    private void EngageWith(GameObject player)
    {
        watchedObject = player;
        if (state != PasserbyStates.Watching)
        {
            animator.SetTrigger("GoIdle");
        }
        state = PasserbyStates.InConversation;
    }
    private void Leave()
    {
        if (target != null && !target.position.IsApproximately(_agent.transform.position))
        {
            _agent.destination = target.position;
            return;
        }

        PasserbySpawnManager.I.Remove(this);
        PasserbyModelManager.I.RemoveFromModelsInScene(npcModel);
        Destroy(transform.gameObject);
    }

    private void ChooseNewTempDestination()
    {
        var v2 = UnityEngine.Random.insideUnitCircle.ProjectInto(Utils.Borders);
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
