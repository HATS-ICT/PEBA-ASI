using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HealthSystem : MonoBehaviour
{
    public bool isImmune = false;
    public bool isDead = false;
    public bool isInjured = false;
    public int health;
    public HealthStatus healthStatus = HealthStatus.Alive;
    
    private VictimController controller;
    private CapsuleCollider capsule;
    
    public void Initialize(VictimController controller)
    {
        this.controller = controller;
        this.capsule = controller.GetComponent<CapsuleCollider>();
        health = Random.Range(1, SimConfig.DefaultAgentHealth + 1);
        healthStatus = HealthStatus.Alive;
    }
    
    public void TakeDamage()
    {
        health--;
        healthStatus = HealthStatus.Injured;
        
        // Update health status in PersonDataManager
        if (controller.personDataManager != null)
        {
            controller.personDataManager.health = health;
            controller.personDataManager.healthStatus = healthStatus.ToString();
        }
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        healthStatus = HealthStatus.Dead;
        controller.personDataManager.logger.SetFinalStatus("Dead");
        // Update health status in PersonDataManager
        if (controller.personDataManager != null)
        {
            controller.personDataManager.health = 0;
            controller.personDataManager.healthStatus = "Dead";
        }
        
        controller.GetComponent<Animator>().SetBool("death", true);
        controller.GetComponent<NavMeshAgent>().enabled = false;
        capsule.isTrigger = true;
        capsule.center = new Vector3(0, 0, 0);
        isDead = true;
    }
}