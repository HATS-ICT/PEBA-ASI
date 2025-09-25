using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using UnityEditor.Animations;

public enum MovementState
{
    StayStill,
    Walk,
    Sprint,
}

public class VictimController : MonoBehaviour
{
    [Header("Components")]
    public HealthSystem healthSystem;
    public MovementSystem movementSystem;
    public DialogVisualizationSystem dialogVisualizationSystem;
    public ObservationSystem observationSystem;
    public AnimationController animationController;
    
    [Header("State")]
    public bool isWaiting = false;
    private bool isWaitingForAction = false;
    private bool isDestroyed = false;
    [SerializeField] private bool showDebugPath = false;  // Add this toggle for debug path visualization
    
    // Components
    private NavMeshAgent agent;
    private Animator anim;
    private CapsuleCollider capsule;
    public PersonDataManager personDataManager;
    private NavigationManager navigationManager;
    
    public List<PendingEvent> pendingEvents = new List<PendingEvent>();
    private float nextLogTime = 0f;
    
    public bool isDead => healthSystem?.isDead ?? false;
    public bool isImmune => healthSystem?.isImmune ?? false;
    public bool isInjured => healthSystem?.isInjured ?? false;
    public HealthStatus healthStatus => healthSystem?.healthStatus ?? HealthStatus.Alive;

    private Vector3 originalCapsuleCenter;

    private int currentTestActionIndex = 0;
    private List<Action> testActions = new List<Action>
    {
        new Action
        {
            actionType = ActionType.MoveToHideSpot,
            targetLocation = new Vector3(-0.9109039f, 0f, -39.79232f),
            dialogText = "Go to the hiding spot 1!",
            movementState = MovementState.Sprint
        },
        new Action
        {
            actionType = ActionType.MoveToHideSpot,
            targetLocation = new Vector3(-3.090904f, 0f, -43.24132f),
            dialogText = "Go to the hiding spot 2!",
            movementState = MovementState.Walk
        },
        new Action
        {
            actionType = ActionType.StayStill,
            targetLocation = null,
            dialogText = "Stay still and be quiet.",
            movementState = MovementState.StayStill
        },
        new Action
        {
            actionType = ActionType.MoveToHideSpot,
            targetLocation = new Vector3(-0.88591f, 0f, -38.66032f),
            dialogText = "Go to the hiding spot 3!",
            movementState = MovementState.Walk
        },
        new Action
        {
            actionType = ActionType.MoveToHideSpot,
            targetLocation = new Vector3(-0.88591f, 0f, -38.66032f),
            dialogText = "Go to the hiding spot 3 again!",
            movementState = MovementState.Walk
        },
        new Action
        {
            actionType = ActionType.MoveToRegion,
            targetLocation = new Vector3(-15.67991f, 0f, -52.15532f),
            dialogText = "Go to the kitchen area!",
            movementState = MovementState.Sprint
        },
        new Action
        {
            actionType = ActionType.MoveToRegion,
            targetLocation = new Vector3(-9.67991f, 0f, -51.15532f),
            dialogText = "Go to the kitchen area!",
            movementState = MovementState.Walk
        }
    };

    private void Awake()
    {
        InitializeComponents();
        
        navigationManager = gameObject.AddComponent<NavigationManager>();
        Region _ = navigationManager.GetCurrentLocation();
        personDataManager = gameObject.AddComponent<PersonDataManager>();
    }
    
    private void InitializeComponents()
    {
        // Initialize or add required components
        if (healthSystem == null)
            healthSystem = gameObject.AddComponent<HealthSystem>();
            
        if (movementSystem == null)
            movementSystem = gameObject.AddComponent<MovementSystem>();
            
        if (dialogVisualizationSystem == null)
            dialogVisualizationSystem = gameObject.AddComponent<DialogVisualizationSystem>();
            
        if (observationSystem == null)
            observationSystem = gameObject.AddComponent<ObservationSystem>();
            
        if (animationController == null)
            animationController = gameObject.AddComponent<AnimationController>();
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        
        originalCapsuleCenter = capsule.center;
        
        healthSystem.Initialize(this);
        movementSystem.Initialize(this, agent, anim);
        dialogVisualizationSystem.Initialize(this, capsule);
        observationSystem.Initialize(this, navigationManager, personDataManager);
        animationController.Initialize(this, anim, agent);
        
        capsule.isTrigger = true;
        isWaiting = false;
        isWaitingForAction = false;
    }

    private void Update()
    {
        if (!healthSystem.isDead)
        {
            dialogVisualizationSystem.UpdateTextBubbleRotation();
            movementSystem.CheckTargetTimeout();
            
            Behave();

            if (!isDestroyed) {
                movementSystem.UpdateSpeedBasedOnMovementState();
                movementSystem.UpdateAgentParameters();
                animationController.UpdateAnimationParameters();
            }
            
            if (Time.time >= nextLogTime && personDataManager != null)
            {
                LogPositionData();
                nextLogTime = Time.time + SimulationManager.Instance.positionLogInterval;
            }
            
            if (showDebugPath)
            {
                movementSystem.DrawDebugPath();
            }
        }
        else
        {
            if (Time.time >= nextLogTime && personDataManager != null)
            {
                LogDeadPositionData();
                nextLogTime = Time.time + SimulationManager.Instance.positionLogInterval;
            }
        }
    }
    
    private void LogPositionData()
    {
        personDataManager.health = healthSystem.health;
        personDataManager.healthStatus = healthSystem.healthStatus.ToString();
        personDataManager.LogPosition(transform.position, transform.forward, 
                                     healthSystem.health, healthSystem.healthStatus.ToString());
    }
    
    private void LogDeadPositionData()
    {
        personDataManager.health = 0;
        personDataManager.healthStatus = "Dead";
        personDataManager.LogPosition(transform.position, transform.forward, 0, "Dead");
    }

    private async void Behave()
    {
        if (isWaitingForAction || isDestroyed)
        {
            return;
        }
        
        if (isWaiting)
        {
            return; // Simply return while waiting, don't reset the flag here
        }

        if (movementSystem.HasTargetLocation())
        {
            movementSystem.MoveToTarget();
        } 
        else 
        {
            bool observesShooting = SimController.shootingHasStarted;
            Observation observation = observationSystem.GetObservation(observesShooting);
            isWaitingForAction = true;  
            
            try {
                Action action = await personDataManager.PlanAction(observation);

                if (isDestroyed) return;
                
                isWaitingForAction = false;
                ProcessAction(action);
                
                if (action.actionType == ActionType.StayStill) {
                    StartWaitTimer();
                }
            }
            catch (System.Exception ex) {
                if (!isDestroyed) {
                    Debug.LogError($"Error in Behave: {ex.Message}");
                    isWaitingForAction = false;
                }
            }
        }
    }
    
    private async void StartWaitTimer()
    {
        isWaiting = true;
        await Task.Delay(5000); // Wait for 5 seconds
        
        if (!isDestroyed)
        {
            isWaiting = false;
        }
    }

    private void ProcessAction(Action action)
    {
        if (isDestroyed || action == null) return;

        if (action.dialogText != null)
        {
            DialogManager.Instance.AddDialog(personDataManager.persona.name, action.dialogText, transform.position);
        }

        movementSystem.currentActionType = action.actionType;

        switch (action.actionType)
        {
            case ActionType.MoveToRegion:
            case ActionType.MoveToPerson:
            case ActionType.MoveToHideSpot:
            case ActionType.MoveToExit:
                // Only set target location if it's not null
                if (action.targetLocation.HasValue)
                {
                    movementSystem.SetTargetLocation(action.targetLocation);
                    // Ensure animation state is updated when starting movement
                    animationController.UpdateAnimationParameters();
                }
                else
                {
                    movementSystem.currentMovementState = MovementState.StayStill;
                    // Explicitly reset animation when no target
                    Debug.LogError($"{personDataManager.persona.name} received null target location, standing still");
                }
                break;
            
            case ActionType.StayStill:
                movementSystem.currentMovementState = MovementState.StayStill;
                // anim.SetFloat("velocity", 0);
                break;
        }
        
        dialogVisualizationSystem.CurrentDialogText = action.dialogText;

        movementSystem.currentMovementState = action.movementState;
        if (action.movementState == MovementState.Sprint || action.movementState == MovementState.Walk)
        {
            if (movementSystem.isCrouching)
            {
                movementSystem.ToggleCrouch();
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("ExitPlace"))
        {
            personDataManager.logger.SetFinalStatus("Escaped");
            gameObject.SetActive(false);
        }

    }

    public void AddPendingEvent(string eventType, string description)
    {
        pendingEvents.Add(new PendingEvent(eventType, description));
    }
    
    public void DamageThis()
    {
        if (movementSystem.isCrouching) return;
        healthSystem.TakeDamage();
        
        AddPendingEvent("got_shot", "I was just shot and injured");
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        VictimController otherVictim = collision.gameObject.GetComponent<VictimController>();
        if (otherVictim != null)
        {
            Vector3 pushDirection = (transform.position - otherVictim.transform.position).normalized;
            agent.velocity += pushDirection * 0.5f;
            
            if (movementSystem.HasTargetLocation())
            {
                Vector3? target = movementSystem.targetLocation;
                if (target.HasValue)
                {
                    agent.SetDestination(target.Value);
                }
            }
        }
    }

    // Add OnDestroy to set the destroyed flag
    private void OnDestroy()
    {
        isDestroyed = true;
    }

    // Helper method to check if victim is in cafeteria
    private bool IsInCafeteria()
    {
        if (navigationManager != null)
        {
            Region currentRegion = navigationManager.GetCurrentLocation();
            return currentRegion != null && (currentRegion.regionId == "cafeteria" || currentRegion.regionId == "cafeteria_kitchen" || currentRegion.regionId == "kitchen_yard");
        }
        return false;
    }
}
