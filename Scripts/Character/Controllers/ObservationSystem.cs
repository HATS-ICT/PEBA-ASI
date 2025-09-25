using System.Collections.Generic;
using UnityEngine;

public class ObservationSystem : MonoBehaviour
{
    [Header("Observation Settings")]
    [Tooltip("Include mood information in observations")]
    public bool includeMood = true;
    [Tooltip("Include movement state in observations")]
    public bool includeMovementState = true;
    [Tooltip("Include shooter information in observations")]
    public bool includeShooterInfo = true;
    [Tooltip("Include location information in observations")]
    public bool includeLocation = true;
    [Tooltip("Include surrounding people in observations")]
    public bool includeSurroundingPeople = true;
    [Tooltip("Include neighboring regions in observations")]
    public bool includeNeighborRegions = true;
    [Tooltip("Include interest points in observations")]
    public bool includeInterestPoints = true;
    [Tooltip("Include surrounding conversations in observations")]
    public bool includeSurroundingConversation = true;
    [Tooltip("Include pending events in observations")]
    public bool includePendingEvents = true;

    private VictimController controller;
    private NavigationManager navigationManager;
    private PersonDataManager personDataManager;

    const string STAY_SILL_ACTION_ID = "stay_still";
    const string FIGHT_THE_SHOOTER_ACTION_ID = "fight_the_shooter";
    
    private ShooterInfo previousShooterInfo = null;

    public void Initialize(VictimController controller, NavigationManager navigationManager, PersonDataManager personDataManager)
    {
        this.controller = controller;
        this.navigationManager = navigationManager;
        this.personDataManager = personDataManager;
    }
    
    /// <summary>
    /// Get the observation of the victim.
    /// </summary>
    /// <param name="observesShooting">Whether the victim should observe shooting related information. 
    /// This helps reduce the hallucination of the LLM to choose hiding or sensing the shooter info where there is no danger.</param>
    /// <returns>The observation of the victim.</returns>
    public Observation GetObservation(bool observesShooting)
    {
        Observation observation = new Observation();
        
        if (includeMood)
            observation.mood = personDataManager.currentMood;
        
        if (includeMovementState)
            observation.current_movement_state = personDataManager.currentMovementState;
        
        // Add isHiding state when shooter is observed
        if (observesShooting)
        {
            // Get the MovementSystem component to check if the agent is crouching
            MovementSystem movementSystem = controller.movementSystem;
            observation.isHiding = movementSystem != null && movementSystem.isCrouching;
        }
        
        // Initialize available_action_ids list first
        observation.available_action_ids = new List<string>();
        observation.available_action_ids.Add(STAY_SILL_ACTION_ID);
        
        if (includeShooterInfo && observesShooting)
        {
            observation.shooter_info = GetShooterObservation();
            
            // Add fight_the_shooter action if shooter is in the same region
            if (observation.shooter_info != null && observation.shooter_info.isInSameRegion)
            {
                observation.available_action_ids.Add(FIGHT_THE_SHOOTER_ACTION_ID);
            }
        }
        
        if (includeLocation)
            observation.location = navigationManager.GetCurrentLocation();
        
        observation.surrounding_people = new List<SurroundingPeople>();
        
        if (includeNeighborRegions) {
            observation.neighbor_regions = navigationManager.GetNeighborRegions();
            // Add neighbor region IDs to available actions
            if (observation.neighbor_regions != null) {
                foreach (var region in observation.neighbor_regions) {
                    observation.available_action_ids.Add(region.id);
                }
            }
        }
        
        if (includeInterestPoints && observesShooting) {
            observation.interest_points = navigationManager.GetInterestPointsInCurrentRegion();
            // Add interest point IDs to available actions
            if (observation.interest_points != null) {
                foreach (var point in observation.interest_points) {
                    observation.available_action_ids.Add(point.id);
                }
            }
        }
        
        if (includeSurroundingConversation)
            observation.surrounding_conversation = DialogManager.Instance.GetSurroundingDialogue(transform.position, SimConfig.ConversationLimit);
        
        // Add pending events to the observation
        if (includePendingEvents) {
            observation.pending_events = new List<PendingEvent>(controller.pendingEvents);
            // Clear pending events after they've been observed
            controller.pendingEvents.Clear();
        }
        
        if (includeSurroundingPeople) {
            List<VictimController> nearbyPeople = GetNearbyPeople(SimConfig.NearbyPeopleRadius);
            foreach (VictimController person in nearbyPeople)
            {
                var surroundingPerson = new SurroundingPeople
                {
                    name = person.personDataManager.persona.name,
                    health_status = person.healthStatus
                };
                observation.surrounding_people.Add(surroundingPerson);
                
                // Add person's name (converted to ID format) to available actions
                string personId = person.personDataManager.persona.name.ToLower().Replace(" ", "_");
                observation.available_action_ids.Add(personId);
            }
        }
        
        return observation;
    }

    private ShooterInfo GetShooterObservation()
    {
        if (navigationManager == null)
        {
            Debug.LogError("NavigationManager is not assigned");
            return previousShooterInfo ?? null;
        }
        Vector3 shooterLocation = navigationManager.GetShooterLocation();
        Region shooterRegion = navigationManager.GetRegionByCoordinate(shooterLocation);

        string regionId = "unknown";
        string distance = "unknown";
        bool isInLineOfSight = false;
        string direction = "unknown";
        bool isInSameRegion = false;  // Add flag to track if shooter is in same region

        if (shooterRegion != null)
        {
            regionId = shooterRegion.regionId;
            distance = Vector3.Distance(transform.position, shooterLocation).ToString("F2");
            
            // Check if shooter is in the same region as the victim
            Region victimRegion = navigationManager.GetRegionByCoordinate(transform.position);
            isInSameRegion = (victimRegion != null && victimRegion.regionId == shooterRegion.regionId);
            
            // Calculate direction from victim to shooter
            Vector3 directionVector = shooterLocation - transform.position;
            direction = GetCardinalDirection(directionVector);
            
            // Perform ray test to check if shooter is in line of sight
            Vector3 directionToShooter = shooterLocation - transform.position;
            // Adjust ray start position to be at eye level
            Vector3 rayStart = transform.position + new Vector3(0, 1.6f, 0);
            
            // Cast a ray toward the shooter
            RaycastHit hit;
            if (Physics.Raycast(rayStart, directionToShooter.normalized, out hit, 100f))
            {
                // Check if the ray hit the shooter or something else
                if (hit.collider.CompareTag("Shooter"))
                {
                    isInLineOfSight = true;
                }
                // Debug.DrawRay(rayStart, directionToShooter.normalized * hit.distance, Color.red, 1f);
            }
        }
        else if (previousShooterInfo != null)
        {
            // Use previous shooter info if current info is unknown
            return previousShooterInfo;
        }

        ShooterInfo currentInfo = new ShooterInfo
        {
            regionId = regionId,
            distance = distance,
            isInLineOfSight = isInLineOfSight,
            direction = direction,
            isInSameRegion = isInSameRegion  // Add the new property to ShooterInfo
        };
        
        // Store the current shooter info for future reference
        previousShooterInfo = currentInfo;
        
        return currentInfo;
    }

    // Helper method to convert a direction vector to a cardinal direction string
    private string GetCardinalDirection(Vector3 direction)
    {
        // Ignore the y component for determining cardinal direction
        Vector2 flatDirection = new Vector2(direction.x, direction.z).normalized;
        
        // Calculate the angle in degrees (0° is east, increases counter-clockwise)
        float angle = Mathf.Atan2(flatDirection.y, flatDirection.x) * Mathf.Rad2Deg;
        
        // Convert to 0-360 range
        if (angle < 0) angle += 360f;
        
        // Convert angle to cardinal direction
        if (angle >= 337.5f || angle < 22.5f)
            return "east";
        else if (angle >= 22.5f && angle < 67.5f)
            return "northeast";
        else if (angle >= 67.5f && angle < 112.5f)
            return "north";
        else if (angle >= 112.5f && angle < 157.5f)
            return "northwest";
        else if (angle >= 157.5f && angle < 202.5f)
            return "west";
        else if (angle >= 202.5f && angle < 247.5f)
            return "southwest";
        else if (angle >= 247.5f && angle < 292.5f)
            return "south";
        else // angle >= 292.5f && angle < 337.5f
            return "southeast";
    }

    public List<VictimController> GetNearbyPeople(float radius)
    {
        List<VictimController> nearbyPeople = new List<VictimController>();
        Transform victimsParent = transform.parent;  // Gets the "Victims" GameObject
        
        foreach (Transform child in victimsParent)
        {
            if (child.gameObject == gameObject) continue;  // Skip self
            
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance <= radius)
            {
                VictimController victim = child.GetComponent<VictimController>();
                if (victim != null)
                {
                    nearbyPeople.Add(victim);
                }
            }
        }
        
        return nearbyPeople;
    }
}
