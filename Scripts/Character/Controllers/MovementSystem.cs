using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MovementSystem : MonoBehaviour
{
    public MovementState currentMovementState = MovementState.StayStill;
    public ActionType currentActionType;
    public float speedMultiplier = 1.0f;
    public bool isCrouching = false;
    
    private Vector3? _targetLocation;
    public Vector3? targetLocation
    {
        get => _targetLocation;
        set => _targetLocation = value;
    }
    
    private float targetSetTime = 0f;
    
    private VictimController controller;
    private NavMeshAgent agent;
    private Animator anim;
    private bool changeBaseOffset = true;
    
    public void Initialize(VictimController controller, NavMeshAgent agent, Animator anim)
    {
        this.controller = controller;
        this.agent = agent;
        this.anim = anim;
        
        // Configure NavMeshAgent
        agent.avoidancePriority = SimConfig.AgentAvoidancePriority;
        agent.radius = SimConfig.AgentRadius;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        speedMultiplier = Random.Range(1f, 1.5f);
        agent.speed = 0;
    }
    
    public bool HasTargetLocation()
    {
        return targetLocation.HasValue;
    }
    
    public void SetTargetLocation(Vector3? target)
    {
        // Only set target and update time if target is not null
        if (target.HasValue)
        {
            targetLocation = target;
            targetSetTime = Time.time;
        }
        else
        {
            // Clear any existing target when null is passed
            targetLocation = null;
        }
    }
    
    public void MoveToTarget()
    {
        if (!HasTargetLocation()) return;
        
        // Calculate direct distance to target (ignoring height/y-axis)
        float directDistance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(targetLocation.Value.x, targetLocation.Value.z)
        );
        
        // If we're not close to the target, try to move there
        if (directDistance > 0.5f)
        {
            agent.SetDestination(targetLocation.Value);
        }
        else
        {
            HandleReachedTarget();
        }
    }

    private void HandleReachedTarget()
    {
        targetLocation = null;
        currentMovementState = MovementState.StayStill;
        agent.speed /= 4;
        anim.SetFloat("velocity", 0);
        if (currentActionType == ActionType.MoveToHideSpot)
        {
            if (!isCrouching)
            {
                ToggleCrouch();
                controller.AddPendingEvent("found_hiding_spot", $"I have reached the hiding spot");
            }
        }
    }
    
    public void CheckTargetTimeout()
    {
        if (HasTargetLocation())
        {
            // If we've been trying to reach this target for too long, reset it
            if (Time.time - targetSetTime > SimConfig.TargetReachTimeout)
            {
                targetLocation = null;
                agent.velocity = Vector3.zero;
            }
        }
    }
    
    public void UpdateSpeedBasedOnMovementState()
    {
        switch (currentMovementState)
        {
            case MovementState.StayStill:
                agent.speed = SimConfig.StayStillSpeed;
                // Debug.Log("UpdateSpeedBasedOnMovementState: StayStill");
                break;
            case MovementState.Walk:
                agent.speed = SimConfig.WalkSpeed * speedMultiplier;
                // Debug.Log("UpdateSpeedBasedOnMovementState: Walk");
                break;
            case MovementState.Sprint:
                agent.speed = SimConfig.SprintSpeed * speedMultiplier;
                // Debug.Log("UpdateSpeedBasedOnMovementState: Sprint");
                break;
        }
    }
    
    public void ToggleCrouch()
    {
        if (anim.GetBool("crouching"))
        {
            // Debug.Log("Toggling crouch to false");
            anim.SetBool("crouching", false);
            isCrouching = false;
        }
        else if (!anim.GetBool("crouching"))
        {
            // Debug.Log("Toggling crouch to true");
            anim.SetBool("crouching", true);
            isCrouching = true;
            agent.speed /= 4;
        }
    }
    
    public void UpdateAgentParameters()
    {
        if (agent.velocity.magnitude > 1.0f && changeBaseOffset)
        {   
            agent.baseOffset = -0.01f;
            anim.applyRootMotion = false;
            changeBaseOffset = false;
        }
    }
    
    public void DrawDebugPath()
    {
        if (HasTargetLocation())
        {
            // Use the same height as the agent for the debug line
            Vector3 targetWithSameHeight = new Vector3(
                targetLocation.Value.x, 
                transform.position.y, 
                targetLocation.Value.z
            );
            Debug.DrawLine(transform.position, targetWithSameHeight, Color.green);
        }
    }
}