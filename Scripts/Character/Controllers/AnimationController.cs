using UnityEngine;
using UnityEngine.AI;
using UnityEditor.Animations;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
    private VictimController controller;
    private Animator anim;
    private NavMeshAgent agent;
    
    // Animation state tracking
    private float currentRotation = 0;
    private float lastRotation = 0;
    private float changeInRotation = 0;
    
    public void Initialize(VictimController controller, Animator anim, NavMeshAgent agent)
    {
        this.controller = controller;
        this.anim = anim;
        this.agent = agent;
        
        // Initialize animation parameters
        anim.SetBool("death", false);
        anim.SetFloat("rotation", 0);
        anim.SetFloat("velocity", 0);
        
        // Update animation transition threshold
        UpdateAnimationTransitionThreshold();
    }
    
    public void UpdateAnimationParameters()
    {
        currentRotation = transform.rotation.eulerAngles.y;

        // Divide by deltaTime to get the change in rotation per second
        changeInRotation = (currentRotation - lastRotation) / Time.deltaTime;
        lastRotation = currentRotation;

        if (agent.velocity.magnitude < 8f)
        {
            changeInRotation = 0f;
        }

        anim.SetFloat("rotation", changeInRotation);
        anim.SetFloat("velocity", agent.velocity.magnitude);
    }
    
    private void UpdateAnimationTransitionThreshold()
    {
        if (anim == null) return;
        
        // Get the animator controller
        AnimatorController controller = anim.runtimeAnimatorController as AnimatorController;
        if (controller == null) return;
        
        bool madeChanges = false;
        
        // Check each layer and state machine
        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                // Look for Idle state
                if (state.state.name == "Idle")
                {
                    foreach (AnimatorStateTransition transition in state.state.transitions)
                    {
                        // Look for transitions to movement states
                        if (transition.destinationState != null && 
                             transition.destinationState.name.Contains("Movement"))
                        {
                            // Check each condition
                            for (int i = 0; i < transition.conditions.Length; i++)
                            {
                                AnimatorCondition condition = transition.conditions[i];
                                
                                // Find velocity parameter with threshold near 1.0
                                if (condition.parameter == "velocity")
                                {
                                    // Create a new condition array
                                    AnimatorCondition[] newConditions = new AnimatorCondition[transition.conditions.Length];
                                    System.Array.Copy(transition.conditions, newConditions, transition.conditions.Length);
                                    
                                    // Update the velocity threshold
                                    newConditions[i] = new AnimatorCondition
                                    {
                                        mode = AnimatorConditionMode.Greater,
                                        parameter = "velocity",
                                        threshold = 0.1f
                                    };
                                    
                                    // Apply the new conditions
                                    transition.conditions = newConditions;
                                    madeChanges = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        if (madeChanges)
        {
            // Debug.Log($"Updated animation transition threshold for {gameObject.name}");
        }
        
        // Update animation blend tree values based on the provided settings
        UpdateAnimationBlendTreeValues(controller);
    }

    private void UpdateAnimationBlendTreeValues(AnimatorController controller)
    {
        if (controller == null) return;
        
        bool madeChanges = false;
        
        // Define the correct motion settings
        Dictionary<string, Vector2> motionSettings = new Dictionary<string, Vector2>
        {
            { "HumanoidIdle", new Vector2(0, 0) },
            { "HumanoidWalk", new Vector2(1, 0) },
            { "HumanoidRun", new Vector2(3, 0) },
            { "HumanoidRunLeft", new Vector2(3, -120) },
            { "HumanoidRunRight", new Vector2(3, 120) },
            { "HumanoidRunLeftSharp", new Vector2(3, -180) },
            { "HumanoidRunRightSharp", new Vector2(3, 180) }
        };
        
        // Check each layer for blend trees
        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            // Search through all states in the state machine
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                // Check if the state has a motion that is a blend tree
                BlendTree blendTree = state.state.motion as BlendTree;
                if (blendTree != null)
                {
                    // Process each child motion in the blend tree
                    for (int i = 0; i < blendTree.children.Length; i++)
                    {
                        ChildMotion childMotion = blendTree.children[i];
                        string motionName = childMotion.motion != null ? childMotion.motion.name : "";
                        
                        // Check if this motion is in our settings dictionary
                        if (motionSettings.ContainsKey(motionName))
                        {
                            Vector2 newPosition = motionSettings[motionName];
                            
                            // Only update if the values are different
                            if (childMotion.position.x != newPosition.x || childMotion.position.y != newPosition.y)
                            {
                                // Create a new array of child motions
                                ChildMotion[] newChildren = new ChildMotion[blendTree.children.Length];
                                System.Array.Copy(blendTree.children, newChildren, blendTree.children.Length);
                                
                                // Update the position for this child motion
                                newChildren[i].position = newPosition;
                                
                                // Apply the updated children to the blend tree
                                blendTree.children = newChildren;
                                madeChanges = true;
                                
                                Debug.Log($"Updated {motionName} position to ({newPosition.x}, {newPosition.y})");
                            }
                        }
                    }
                }
            }
        }
        
        if (madeChanges)
        {
            Debug.Log($"Updated animation blend tree values for {gameObject.name}");
        }
    }
}