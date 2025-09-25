using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimController : MonoBehaviour
{
    public static bool hasStarted = false;
    public static bool doorsAreNowLocked = false;
    public static bool shootingHasStarted = false;
    public static float doorTimerStatic;
    public float doorTimer = 20;
    public bool lockAllDoors = true;

    bool playOnce = true;
    GameObject[] doors;
    VictimController[] victims;
    
    // Add a variable to track when the simulation started
    private static float simulationStartTime;
    private bool timerStarted = false;

    void Awake()
    {
        Physics.IgnoreLayerCollision(9, 10);
        doors = GameObject.FindGameObjectsWithTag("Door");
        victims = FindObjectsOfType<VictimController>();
        
        // Record the start time of this simulation instance
        simulationStartTime = Time.time;
        
        // Reset static variables to ensure clean state
        ResetSimulationState();
    }

    void Start()
    {
        doorTimerStatic = doorTimer;
    }

    void Update()
    {
        if (!timerStarted)
        {
            StartCoroutine("StartTimer", 10);
            timerStarted = true;
        }

        if(hasStarted && playOnce && lockAllDoors)
        {
            DoorLock();
            NotifyAllVictimsOfGunshot();
            
            playOnce = false;
        }
    }

    void DoorLock()
    {
        StartCoroutine("DoorTimer", doorTimer);
    }

    void NotifyAllVictimsOfGunshot()
    {
        shootingHasStarted = true;
        
        foreach (VictimController victim in victims)
        {
            if (victim != null && !victim.isDead)
            {
                victim.AddPendingEvent("gunshot", "I heard a loud gunshot. There might be an active shooter in the building.");
            }
        }
    }

    IEnumerator DoorTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        doorsAreNowLocked = true;
    }

    IEnumerator StartTimer(float timer)
    {
        yield return new WaitForSeconds(timer);
        hasStarted = true;
        Physics.IgnoreLayerCollision(9, 10, false);
    }
    
    // Static method to reset simulation state
    public static void ResetSimulationState()
    {
        // Reset static variables
        hasStarted = false;
        doorsAreNowLocked = false;
        shootingHasStarted = false;
        doorTimerStatic = 0f;
        simulationStartTime = Time.time;
        
        PersonDataManager.ClearAssignedPersonaIndices();
        
        // Reset all PersonDataManagers
        PersonDataManager[] personDataManagers = GameObject.FindObjectsOfType<PersonDataManager>();
        foreach (PersonDataManager pdm in personDataManagers)
        {
            if (pdm != null)
            {
                // Reset conversation history
                pdm.ResetConversationHistory();
                
                // Reset memory
                pdm.memory = new Memory { events = new List<MemoryEvent>() };
                
                // Reset mood and movement state
                pdm.currentMood = "neutral";
                pdm.currentMovementState = "stay_still";
                
                // Reset health
                pdm.health = 1;
                pdm.healthStatus = "Alive";
            }
        }
        
        // Clear any static managers that might persist
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ClearHistory();
        }
        
        // Destroy any persistent SimulationLogger instance
        if (SimulationLogger.Instance != null)
        {
            GameObject.Destroy(SimulationLogger.Instance.gameObject);
        }
        
        // Reset OpenAIUtils log file and token usage
        OpenAIUtils.InitializeLogFile();
        OpenAIUtils.ResetTokenUsage();
        
        // Force garbage collection to clean up memory
        System.GC.Collect();
        
    }
}
