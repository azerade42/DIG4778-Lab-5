using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Avoidee : MonoBehaviour
{
    [SerializeField] private Avoider target;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 1f;
    }
    private void Update()
    {
        agent.SetDestination(target.transform.position);
    }
}
