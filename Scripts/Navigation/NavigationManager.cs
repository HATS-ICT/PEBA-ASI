using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;  


public class NavigationManager : MonoBehaviour
{
    public Region currentRegion;
    private Region[] cachedRegions;
    
    private void Awake()
    {
        // Initialize regions cache and current region on startup
        RefreshRegionsCache();
        GetCurrentLocation();
    }
    
    private void RefreshRegionsCache()
    {
        cachedRegions = Object.FindObjectsByType<Region>(FindObjectsSortMode.None);
    }

    public Region GetRegionByCoordinate(Vector3 coordinate)
    {
        if (cachedRegions == null || cachedRegions.Length == 0)
        {
            RefreshRegionsCache();
        }
        
        foreach (Region region in cachedRegions)
        {
            BoxCollider boxCollider = region.GetComponent<BoxCollider>();
            if (boxCollider != null && boxCollider.bounds.Contains(coordinate))
            {
                return region;
            }
        }
        return null;
    }
    

    public Region GetCurrentLocation()
    {
        if (cachedRegions == null || cachedRegions.Length == 0)
        {
            RefreshRegionsCache();
        }
        
        Vector3 playerPosition = transform.position;

        foreach (Region region in cachedRegions)
        {
            BoxCollider boxCollider = region.GetComponent<BoxCollider>();
            if (boxCollider != null && boxCollider.bounds.Contains(playerPosition))
            {
                currentRegion = region;
                return region;
            }
        }
        
        // If we didn't find a region containing the player, log a warning
        if (currentRegion == null)
        {
            Debug.LogWarning($"Player at {playerPosition} is not in any region, using default");
        }
        
        return currentRegion;
    }

    public List<NeighborRegions> GetNeighborRegions()
    {
        if (currentRegion == null)
        {
            Debug.LogError("No current region set!");
            return new List<NeighborRegions>();
        }

        if (Region.regionConnectivity.TryGetValue(currentRegion.regionId, out string[] neighbors))
        {
            return neighbors.Select(neighborId => {
                Region neighborRegion = GetRegionById(neighborId);
                float distance = 0f;
                string description = "Unknown";
                string direction = "unknown";
                
                if (neighborRegion != null)
                {
                    Vector3 currentPos = currentRegion.transform.position;
                    Vector3 neighborPos = neighborRegion.transform.position;
                    distance = Vector3.Distance(currentPos, neighborPos);
                    description = neighborRegion.regionDescription;
                    
                    // Calculate direction from current region to neighbor region
                    Vector3 directionVector = neighborPos - currentPos;
                    direction = GetCardinalDirection(directionVector);
                }

                return new NeighborRegions { 
                    id = neighborId, 
                    distance = (int) distance, 
                    description = description,
                    direction = direction
                };
            }).ToList();
        }
        Debug.LogError("No neighbor regions found for region: " + currentRegion.regionId);
        return new List<NeighborRegions>();
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

    public List<SurroundingInterestPoints> GetInterestPointsInCurrentRegion(int hide_limit = 3, bool randomSelection = true)
    {
        List<SurroundingInterestPoints> pointsInRegion = new List<SurroundingInterestPoints>();
        
        if (currentRegion == null)
        {
            Debug.LogError("No current region set!");
            return pointsInRegion;
        }

        BoxCollider regionBounds = currentRegion.GetComponent<BoxCollider>();
        if (regionBounds == null)
        {
            Debug.LogError("Current region is missing a BoxCollider component!");
            return pointsInRegion;
        }

        // Find all hiding spots and exits
        GameObject[] hidingPlaces = GameObject.FindGameObjectsWithTag("HidingPlace");
        GameObject[] exitPlaces = GameObject.FindGameObjectsWithTag("ExitPlace");
        
        // Temporary list to store hiding places before limiting
        List<SurroundingInterestPoints> hidingPointsList = new List<SurroundingInterestPoints>();
        
        // Check hiding spots
        foreach (GameObject hidingPlace in hidingPlaces)
        {
            if (regionBounds.bounds.Contains(hidingPlace.transform.position))
            {
                InterestPoint point = hidingPlace.GetComponent<InterestPoint>();
                if (point != null)
                {
                    float distance = Vector3.Distance(transform.position, hidingPlace.transform.position);
                    hidingPointsList.Add(new SurroundingInterestPoints {
                        id = point.id,
                        type = point.type,
                        description = point.description,
                        distance = (int)distance,
                        occupant = point.occupant
                    });
                }
            }
        }
        
        // Select hiding places based on the selection method
        if (randomSelection)
        {
            hidingPointsList = hidingPointsList.OrderBy(p => UnityEngine.Random.value).Take(hide_limit).ToList();
        }
        else
        {
            hidingPointsList = hidingPointsList.OrderBy(p => p.distance).Take(hide_limit).ToList();
        }
        
        pointsInRegion.AddRange(hidingPointsList);
        
        // Check exit points
        foreach (GameObject exitPlace in exitPlaces)
        {
            if (regionBounds.bounds.Contains(exitPlace.transform.position))
            {
                InterestPoint point = exitPlace.GetComponent<InterestPoint>();
                if (point != null)
                {
                    float distance = Vector3.Distance(transform.position, exitPlace.transform.position);
                    pointsInRegion.Add(new SurroundingInterestPoints {
                        id = point.id,
                        type = point.type,
                        description = point.description,
                        distance = (int)distance
                    });
                }
            }
        }

        return pointsInRegion;
    }

    public Vector3? GetInterestPointLocationByID(string interestPointId)
    {
        // Find all hiding spots and exits
        GameObject[] hidingPlaces = GameObject.FindGameObjectsWithTag("HidingPlace");
        GameObject[] exitPlaces = GameObject.FindGameObjectsWithTag("ExitPlace");
        
        // Check hiding spots
        foreach (GameObject hidingPlace in hidingPlaces)
        {
            InterestPoint point = hidingPlace.GetComponent<InterestPoint>();
            if (point != null && point.id == interestPointId)
            {
                return hidingPlace.transform.position;
            }
        }
        
        // Check exit points
        foreach (GameObject exitPlace in exitPlaces)
        {
            InterestPoint point = exitPlace.GetComponent<InterestPoint>();
            if (point != null && point.id == interestPointId)
            {
                return exitPlace.transform.position;
            }
        }

        Debug.LogWarning($"Could not find InterestPoint with id: {interestPointId}");
        return null;
    }

    public Vector3? GetRegionLocation(bool randomPoint = false, float padding = 0.7f)
    {
        if (currentRegion == null)
        {
            Debug.LogError("No current region set!");
            return null;
        }

        BoxCollider regionBounds = currentRegion.GetComponent<BoxCollider>();
        if (regionBounds != null)
        {
            if (randomPoint)
            {
                return GetRandomPointInBounds(regionBounds.bounds, padding);
            }
            return regionBounds.bounds.center;
        }
        
        Debug.LogError("Current region is missing a BoxCollider component!");
        return null;
    }

    public Vector3? GetRegionLocationByID(string regionId, bool randomPoint = false, float padding = 0.7f)
    {
        Region region = GetRegionById(regionId);
        if (region == null)
        {
            Debug.LogError("No region found with id: " + regionId);
            return null;
        }

        BoxCollider regionBounds = region.GetComponent<BoxCollider>();
        if (regionBounds != null)
        {
            if (randomPoint)
            {
                return GetRandomPointInBounds(regionBounds.bounds, padding);
            }
            return regionBounds.bounds.center;
        }
        
        Debug.LogError("Region with id: " + regionId + " is missing a BoxCollider component!");
        return null;
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds, float padding)
    {
        // Calculate the padded bounds
        Vector3 paddedMin = bounds.min + new Vector3(padding, padding, padding);
        Vector3 paddedMax = bounds.max - new Vector3(padding, padding, padding);
        
        // Generate a random point within the padded bounds
        return new Vector3(
            Random.Range(paddedMin.x, paddedMax.x),
            Random.Range(paddedMin.y, paddedMax.y),
            Random.Range(paddedMin.z, paddedMax.z)
        );
    }

    public Vector3? GetPersonLocationByNameID(string personId)
    {
        string personName = GetPersonNameByID(personId);
        
        VictimController[] allPeople = Object.FindObjectsByType<VictimController>(FindObjectsSortMode.None);
        foreach (VictimController person in allPeople)
        {
            if (person.personDataManager.persona.name == personName)
            {
                return person.transform.position;
            }
        }
        Debug.LogWarning($"Could not find Person with name: {personName}");
        return null;
    }

    public string GetPersonNameByID(string personId)
    {
        string[] parts = personId.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }
        }
        return string.Join(" ", parts);
    }

    public Region GetRegionById(string regionId)
    {
        if (cachedRegions == null || cachedRegions.Length == 0)
        {
            RefreshRegionsCache();
        }
        
        foreach (Region region in cachedRegions)
        {
            if (region.regionId == regionId)
            {
                return region;
            }
        }
        
        Debug.LogWarning($"Could not find Region with id: {regionId}");
        return null;
    }

    public Vector3 GetShooterLocation()
    {
        GameObject shooter = GameObject.FindGameObjectWithTag("Shooter");
        if (shooter != null)
        {
            return shooter.transform.position;
        }
        Debug.LogWarning("Could not find Shooter GameObject");
        return Vector3.zero;
    }

    // Add a method to update the current region manually if needed
    public void SetCurrentRegion(Region region)
    {
        if (region != null)
        {
            currentRegion = region;
        }
        else
        {
            Debug.LogWarning("Attempted to set current region to null");
        }
    }

    // Add this new method to calculate distance ignoring height
    public static float CalculateDistanceIgnoringHeight(Vector3 point1, Vector3 point2)
    {
        // Create new vectors with the same x and z coordinates but ignoring y (height)
        Vector3 flatPoint1 = new Vector3(point1.x, 0, point1.z);
        Vector3 flatPoint2 = new Vector3(point2.x, 0, point2.z);
        
        // Calculate distance between the flattened points
        return Vector3.Distance(flatPoint1, flatPoint2);
    }
}
