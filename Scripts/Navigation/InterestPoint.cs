using UnityEngine;

public enum InterestPointType
{
    HideSpot,
    ExitPoint
}

public class InterestPoint: MonoBehaviour
{
    public string id;
    public InterestPointType type;
    public string description;
    public string occupant = "None";
    
    private void Start()
    {
        // Set sphere collider radius to 1 for GameObjects tagged as "HidingPlace"
        if (gameObject.CompareTag("HidingPlace"))
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.radius = 0.7f;
            }
            else
            {
                Debug.LogWarning("No SphereCollider found on HidingPlace: " + gameObject.name);
            }
        }
    }
}

