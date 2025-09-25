using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class Region : MonoBehaviour
{
    public string regionId;
    public string regionDescription;

    // // Add a dictionary to store region descriptions
    // public static readonly Dictionary<string, string> regionDescriptions = new Dictionary<string, string>
    // {
    //     {"entrance_hall", "hallway conecting the main entrance to the building"},
    //     {"reception_desk", "Front desk where visitors check in"},
    //     {"entrance_chatting_area1", "Small seating area near the entrance"},
    //     {"entrance_chatting_area2", "Another seating area near the entrance"},
    //     {"hallway1", "Hallway connecting entrance to other areas"},
    //     {"cafeteria", "Large dining area for students and staff"},
    //     {"cafeteria_kitchen", "Kitchen area where food is prepared."},
    //     {"garden", "Outdoor garden area with plants and benches"},
    //     {"hallway2", "Main hallway connecting classrooms and several exits"},
    //     {"classroom1", "Classroom for general studies"},
    //     {"classroom2", "Classroom for general studies"},
    //     {"classroom3", "Classroom for general studies"},
    //     {"classroom4", "Classroom for general studies"},
    //     {"classroom5", "Classroom for general studies"},
    //     {"classroom6", "Classroom for general studies"},
    //     {"classroom7", "Classroom for general studies"},
    //     {"classroom8", "Classroom for general studies"},
    //     {"man_bathroom", "Men's restroom"},
    //     {"woman_bathroom", "Women's restroom"},
    //     {"lounge", "Staff and student lounge area"},
    //     {"hallway3", "Connecting chatting area and outside courtyard."},
    //     {"hallway4", "Connecting main classroom hallway and the garden and the outside courtyard."},
    //     {"hallway5", "Connecting main classroom hallway and the north yard"},
    //     {"far_right_yard", "Yard on the far right of the building, with an exit point."},
    //     {"outside_courtyard", "Outside courtyard to the outside of the building, with an exit point."},
    //     {"north_yard", "North yard of the building, with an exit point."},
    //     {"kitchen_yard", "Kitchen yard of the building, with an exit point."},
    // };

    // public static readonly Dictionary<string, string[]> regionConnectivity = new Dictionary<string, string[]>
    // {
    //     {"entrance_hall", new[] {"reception_desk", "entrance_chatting_area1", "entrance_chatting_area2", "hallway1"}},
    //     {"reception_desk", new[] {"entrance_hall", "cafeteria"}},
    //     {"entrance_chatting_area1", new[] {"entrance_hall", "entrance_chatting_area2", "hallway3"}},
    //     {"entrance_chatting_area2", new[] {"entrance_hall", "entrance_chatting_area1"}},
    //     {"hallway1", new[] {"entrance_hall", "cafeteria", "garden", "hallway2"}},
    //     {"cafeteria", new[] {"reception_desk", "hallway1", "cafeteria_kitchen"}},
    //     {"cafeteria_kitchen", new[] {"cafeteria", "kitchen_yard"}},
    //     {"garden", new[] {"hallway1", "outside_courtyard", "hallway4"}},
    //     {"hallway2", new[] {"hallway1", "hallway4", "hallway5", "classroom1", "classroom2", "classroom3", "classroom4", "classroom5", "classroom6", "classroom7", "classroom8", "man_bathroom", "woman_bathroom", "lounge", "far_right_yard"}},
    //     {"classroom1", new[] {"hallway2"}},
    //     {"classroom2", new[] {"hallway2"}},
    //     {"classroom3", new[] {"hallway2"}},
    //     {"classroom4", new[] {"hallway2"}},
    //     {"classroom5", new[] {"hallway2"}},
    //     {"classroom6", new[] {"hallway2"}},
    //     {"classroom7", new[] {"hallway2"}},
    //     {"classroom8", new[] {"hallway2"}},
    //     {"man_bathroom", new[] {"hallway2"}},
    //     {"woman_bathroom", new[] {"hallway2"}},
    //     {"lounge", new[] {"hallway2"}},
    //     {"hallway3", new[] {"entrance_chatting_area1", "outside_courtyard"}},
    //     {"hallway4", new[] {"hallway2", "garden"}},
    //     {"hallway5", new[] {"hallway2", "north_yard"}},
    //     {"far_right_yard", new[] {"hallway2"}},
    //     {"outside_courtyard", new[] {"hallway3", "garden"}},
    //     {"north_yard", new[] {"hallway5"}},
    //     {"kitchen_yard", new[] {"cafeteria_kitchen"}},
    // };

    // Add a dictionary to store region descriptions
    public static readonly Dictionary<string, string> regionDescriptions = new Dictionary<string, string>
    {
        {"entrance_hall", "Main lobby connecting the main entrance to the office building"},
        {"reception_desk", "Front desk where visitors check in and receive visitor badges"},
        {"entrance_chatting_area1", "Small lounge area near the entrance for informal meetings"},
        {"entrance_chatting_area2", "Another lounge area near the entrance with comfortable seating"},
        {"hallway1", "Main corridor connecting entrance to other office areas"},
        {"cafeteria", "Large dining area for employees and visitors"},
        {"cafeteria_kitchen", "Kitchen area where meals are prepared for the cafeteria"},
        {"garden", "Outdoor garden area with seating for breaks and informal meetings"},
        {"hallway2", "Main hallway connecting office spaces and several exits"},
        {"meeting_rooms", "Conference room for team meetings and client presentations"},
        {"cubicles_area2", "Open office space with cubicles for employees"},
        {"cubicles_area3", "Open office space with cubicles for marketing team"},
        {"cubicles_area4", "Open office space with cubicles for sales team"},
        {"cubicles_area5", "Open office space with cubicles for engineering team"},
        {"cubicles_area6", "Open office space with cubicles for finance department"},
        {"cubicles_area7", "Open office space with cubicles for HR department"},
        {"cubicles_area8", "Open office space with cubicles for HR department"},
        {"man_bathroom", "Men's restroom facilities"},
        {"woman_bathroom", "Women's restroom facilities"},
        {"conference_room", "Large meeting room for company-wide presentations"},
        {"hallway3", "Corridor connecting lounge area and outside courtyard"},
        {"hallway4", "Corridor connecting main office hallway, garden and the outside courtyard"},
        {"hallway5", "Corridor in the main hallway"},
        {"far_right_yard", "East side exterior area of the building, with an emergency exit"},
        {"outside_courtyard", "Central outdoor courtyard with an exit to the parking lot"},
    };

    public static readonly Dictionary<string, string[]> regionConnectivity = new Dictionary<string, string[]>
    {
        {"entrance_hall", new[] {"reception_desk", "entrance_chatting_area1", "entrance_chatting_area2", "hallway1"}},
        {"reception_desk", new[] {"entrance_hall", "cafeteria"}},
        {"entrance_chatting_area1", new[] {"entrance_hall", "entrance_chatting_area2", "hallway3"}},
        {"entrance_chatting_area2", new[] {"entrance_hall", "entrance_chatting_area1"}},
        {"hallway1", new[] {"entrance_hall", "cafeteria", "garden", "hallway2"}},
        {"cafeteria", new[] {"reception_desk", "hallway1", "cafeteria_kitchen"}},
        {"cafeteria_kitchen", new[] {"cafeteria"}},
        {"garden", new[] {"hallway1"}},
        {"hallway2", new[] {"hallway1", "hallway4", "hallway5", "meeting_rooms", "cubicles_area2", "cubicles_area3", "cubicles_area4", "cubicles_area5", "cubicles_area6", "cubicles_area7", "man_bathroom", "woman_bathroom", "conference_room", "far_right_yard"}},
        {"meeting_rooms", new[] {"hallway2"}},
        {"cubicles_area2", new[] {"hallway2"}},
        {"cubicles_area3", new[] {"hallway2"}},
        {"cubicles_area4", new[] {"hallway2"}},
        {"cubicles_area5", new[] {"hallway2"}},
        {"cubicles_area6", new[] {"hallway2"}},
        {"cubicles_area7", new[] {"hallway2"}},
        {"cubicles_area8", new[] {"hallway2"}},
        {"man_bathroom", new[] {"hallway2"}},
        {"woman_bathroom", new[] {"hallway2"}},
        {"conference_room", new[] {"hallway2"}},
        {"hallway3", new[] {"entrance_chatting_area1", "outside_courtyard"}},
        {"hallway4", new[] {"hallway2"}},
        {"hallway5", new[] {"hallway2"}},
        {"far_right_yard", new[] {"hallway2"}},
        {"outside_courtyard", new[] {"hallway3"}},
    };

    // List of exit points
    public static readonly string[] exitRegions = new string[] 
    {
        "far_right_yard", "outside_courtyard"
    };

    // Method to find the shortest path from a region to an exit
    public static string FindPathToExit(string startRegionId, Vector3 agentPosition)
    {
        // Dictionary to store the shortest path to each region
        Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
        // Dictionary to store the distance to each region
        Dictionary<string, float> distances = new Dictionary<string, float>();
        // Queue for BFS
        Queue<string> queue = new Queue<string>();
        // Set of visited regions
        HashSet<string> visited = new HashSet<string>();

        // Initialize the start region
        paths[startRegionId] = new List<string> { startRegionId };
        distances[startRegionId] = 0;
        
        // Store the agent's position to use as the starting point
        Vector3 startPosition = agentPosition;
        
        queue.Enqueue(startRegionId);
        visited.Add(startRegionId);

        // BFS to find the shortest path to each region
        while (queue.Count > 0)
        {
            string currentRegion = queue.Dequeue();
            
            // Get neighbors of the current region
            if (regionConnectivity.TryGetValue(currentRegion, out string[] neighbors))
            {
                foreach (string neighbor in neighbors)
                {
                    // Calculate physical distance
                    float physicalDistance;
                    
                    // If this is the first step from the start region, use agent's position
                    if (currentRegion == startRegionId)
                    {
                        Region neighborRegion = FindRegionById(neighbor);
                        // Use agent's position to the neighbor's center, ignoring height
                        physicalDistance = NavigationManager.CalculateDistanceIgnoringHeight(
                            startPosition, 
                            neighborRegion.GetComponent<BoxCollider>().bounds.center
                        );
                    }
                    else
                    {
                        // For subsequent steps, calculate between region centers
                        physicalDistance = CalculatePhysicalDistanceIgnoringHeight(currentRegion, neighbor);
                    }
                    
                    // Update the distance if shorter path found
                    float newDistance = distances[currentRegion] + physicalDistance;
                    
                    if (!visited.Contains(neighbor))
                    {
                        // Create a new path by copying the path to the current region and adding the neighbor
                        List<string> newPath = new List<string>(paths[currentRegion]);
                        newPath.Add(neighbor);
                        paths[neighbor] = newPath;
                        
                        // Update the distance
                        distances[neighbor] = newDistance;
                        
                        // Add the neighbor to the queue and mark it as visited
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        // Find the closest exit
        string closestExit = null;
        float shortestDistance = float.MaxValue;
        
        foreach (string exitRegion in exitRegions)
        {
            if (distances.ContainsKey(exitRegion) && distances[exitRegion] < shortestDistance)
            {
                closestExit = exitRegion;
                shortestDistance = distances[exitRegion];
            }
        }

        // If no exit is reachable, return a message
        if (closestExit == null)
            return "No exit is reachable from " + startRegionId;

        // Format the path as a string
        string pathString = string.Join("->", paths[closestExit]);
        return $"Route to exit: {pathString}->exit_point (distance: {shortestDistance:F1} meters)";
    }

    // Method to get all exit paths from a region
    public static string GetAllExitPaths(string startRegionId, Vector3 agentPosition)
    {
        StringBuilder allPaths = new StringBuilder();
        
        // Find paths to all exits
        foreach (string exitRegion in exitRegions)
        {
            string path = FindPathToSpecificExit(startRegionId, exitRegion, agentPosition);
            allPaths.AppendLine(path);
        }
        
        return allPaths.ToString();
    }

    // Method to find path to a specific exit
    private static string FindPathToSpecificExit(string startRegionId, string exitRegionId, Vector3 agentPosition = default)
    {
        // Dictionary to store the shortest path to each region
        Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
        // Dictionary to store the distance to each region
        Dictionary<string, float> distances = new Dictionary<string, float>();
        // Dictionary to store intermediate distances between regions
        Dictionary<string, float> intermediateDistances = new Dictionary<string, float>();
        
        // Queue for BFS
        Queue<string> queue = new Queue<string>();
        // Set of visited regions
        HashSet<string> visited = new HashSet<string>();

        // Initialize the start region
        paths[startRegionId] = new List<string> { startRegionId };
        distances[startRegionId] = 0;
        
        // Store the agent's position to use as the starting point
        Vector3 startPosition = agentPosition;
        
        queue.Enqueue(startRegionId);
        visited.Add(startRegionId);

        // BFS to find the shortest path to the exit
        while (queue.Count > 0)
        {
            string currentRegion = queue.Dequeue();
            
            // If we've reached the exit, we can stop
            if (currentRegion == exitRegionId)
                break;
            
            // If the current region is not in the connectivity map, skip it
            if (!regionConnectivity.ContainsKey(currentRegion))
                continue;

            // Check all neighbors of the current region
            foreach (string neighbor in regionConnectivity[currentRegion])
            {
                if (!visited.Contains(neighbor))
                {
                    // Create a new path by copying the path to the current region and adding the neighbor
                    List<string> newPath = new List<string>(paths[currentRegion]);
                    newPath.Add(neighbor);
                    paths[neighbor] = newPath;
                    
                    // Calculate physical distance
                    float physicalDistance;
                    
                    // If this is the first step from the start region and we have a valid agent position, use it
                    if (currentRegion == startRegionId && agentPosition != default)
                    {
                        Region neighborRegion = FindRegionById(neighbor);
                        // Use agent's position to the neighbor's center, ignoring height
                        physicalDistance = NavigationManager.CalculateDistanceIgnoringHeight(
                            startPosition, 
                            neighborRegion.GetComponent<BoxCollider>().bounds.center
                        );
                    }
                    else
                    {
                        // For subsequent steps, calculate between region centers
                        physicalDistance = CalculatePhysicalDistanceIgnoringHeight(currentRegion, neighbor);
                    }
                    
                    // Store the intermediate distance between these two regions
                    string key = $"{currentRegion}->{neighbor}";
                    intermediateDistances[key] = physicalDistance;
                    
                    // Update the distance
                    distances[neighbor] = distances[currentRegion] + physicalDistance;
                    
                    // Add the neighbor to the queue and mark it as visited
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        // If the exit is not reachable, return a message
        if (!paths.ContainsKey(exitRegionId))
            return $"Exit {exitRegionId} is not reachable from {startRegionId}";

        // Format the path as a string with intermediate distances
        StringBuilder pathBuilder = new StringBuilder();
        pathBuilder.Append($"Route to {exitRegionId}: {paths[exitRegionId][0]}");
        
        for (int i = 1; i < paths[exitRegionId].Count; i++)
        {
            string prevRegion = paths[exitRegionId][i-1];
            string currRegion = paths[exitRegionId][i];
            string key = $"{prevRegion}->{currRegion}";
            
            float distance = 0;
            if (intermediateDistances.ContainsKey(key))
            {
                distance = intermediateDistances[key];
            }
            
            pathBuilder.Append($"->{currRegion} ({distance:F1}m)");
        }
        
        pathBuilder.Append($"->exit_point (total distance: {distances[exitRegionId]:F1} m)");
        return pathBuilder.ToString();
    }

    // Helper method to calculate physical distance between two regions, ignoring height
    private static float CalculatePhysicalDistanceIgnoringHeight(string regionId1, string regionId2)
    {
        // Try to find the regions in the scene
        Region region1 = FindRegionById(regionId1);
        Region region2 = FindRegionById(regionId2);
        
        // If both regions are found, calculate the actual distance ignoring height
        if (region1 != null && region2 != null)
        {
            BoxCollider box1 = region1.GetComponent<BoxCollider>();
            BoxCollider box2 = region2.GetComponent<BoxCollider>();
            
            if (box1 != null && box2 != null)
            {
                // Use the centers of the bounding boxes and ignore height
                return NavigationManager.CalculateDistanceIgnoringHeight(
                    box1.bounds.center, 
                    box2.bounds.center
                );
            }
        }
        
        // Default distance if regions can't be found (approximate average room size)
        return 10f;
    }

    // Helper method to find a region by ID
    private static Region FindRegionById(string regionId)
    {
        Region[] allRegions = UnityEngine.Object.FindObjectsByType<Region>(FindObjectsSortMode.None);
        
        foreach (Region region in allRegions)
        {
            if (region.regionId == regionId)
            {
                return region;
            }
        }
        
        return null;
    }

    // Update the BUILDING_MAP to use the dynamic path finding
    public static string GetBuildingMap(string startRegionId, Vector3 agentPosition)
    {
        // Get all paths to exits instead of just the closest one
        return GetAllExitPaths(startRegionId, agentPosition);
    }

    public string camelCaseToSnakeCase(string name)
    {
        string regionId = "";
        
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                regionId += "_" + char.ToLower(c);  
            }
            else
            {
                regionId += char.ToLower(c);
            }
        }
        
        return regionId;
    }

    void Awake()
    {
        // Set regionId based on the GameObject name if not already set
        if (string.IsNullOrEmpty(regionId))
        {
            // Convert from camelCase to snake_case
            string name = gameObject.name;
            regionId = camelCaseToSnakeCase(name);
        }
        
        // Set regionDescription from the dictionary if available
        if (regionDescriptions.ContainsKey(regionId))
        {
            regionDescription = regionDescriptions[regionId];
        }
        else
        {
            Debug.LogWarning("No description found for region: " + regionId);
            regionDescription = "No description available";
        }
    }

    void Start()
    {
        // Get the region's box collider
        BoxCollider regionBounds = GetComponent<BoxCollider>();
        if (regionBounds == null)
        {
            Debug.LogError("Region " + regionId + " is missing a BoxCollider component!");
            return;
        }

        GameObject[] hidingPlaces = GameObject.FindGameObjectsWithTag("HidingPlace");
        
        foreach (GameObject hidingPlace in hidingPlaces)
        {
            // Check if hiding place position is within region bounds
            Vector3 hidingPlacePos = hidingPlace.transform.position;
            Bounds bounds = regionBounds.bounds;
        }
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Debug.Log("Player entered region: " + regionId);
        }
    }

    
}
