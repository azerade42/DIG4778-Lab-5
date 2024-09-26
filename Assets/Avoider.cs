using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class Avoider : MonoBehaviour
{
    [SerializeField] private Avoidee objToAvoid;
    [SerializeField] private float visionRange;
    [SerializeField] private float agentSpeed;
    [SerializeField] private bool showGizmos;
    [SerializeField] private float sampleTickRate = 2f;
    
    private NavMeshAgent agent;
    private AvoiderState avoiderState;
    private List<Vector2> samplesSeen = new List<Vector2>();
    private List<Vector2> samplesHidden = new List<Vector2>();
    private int sampleCount;
    private int sampleCountMax = 20;
    private float timeSinceLastSeen;
    private bool stoppedOnce;
    Vector3 furthestHiddenPos;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = agentSpeed;
        agent.acceleration = 20f;
        agent.stoppingDistance = 1f;
    }

    private void FixedUpdate()
    {
        DetermineAvoiderState();

        if (!stoppedOnce && avoiderState == AvoiderState.Stopped)
        {
            Debug.Log("Stopped since Avoider has not seen the avoidee for over two seconds");
            stoppedOnce = true;
            agent.velocity = Vector3.zero;
        }

        // Resample to a new point if the agent is close to its destination
        if (avoiderState == AvoiderState.Running)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                sampleCount = 0;
                samplesSeen = new List<Vector2>();
                samplesHidden = new List<Vector2>();

                SampleValidMoves();
                MoveToFurthestHiddenPoint();
            }
        }

    }

    // Determines if the avoider needs to run or be stopped
    private void DetermineAvoiderState()
    {
        // Check if there is an obstruction between the avoider and the avoidee
        if (Physics.Raycast(objToAvoid.transform.position, transform.position - objToAvoid.transform.position, out RaycastHit hitInfo, Vector3.Distance(objToAvoid.transform.position, transform.position)))
        {
            // If there is a direct line of sight to the avoider, start making the avoider run away
            if (hitInfo.collider.gameObject.GetComponent<Avoider>())
            {
                timeSinceLastSeen = 0;
                stoppedOnce = false;
                avoiderState = AvoiderState.Running;
            }
            else
            {
                timeSinceLastSeen += agent.remainingDistance <= agent.stoppingDistance ? Time.deltaTime : 0f;

                // Stop the avoider if it has been more than two seconds since there has been a direct line of sight with the avoidee
                if (timeSinceLastSeen > 2f)
                {
                    avoiderState = AvoiderState.Stopped;
                }
            }
        }
    }

    // Uses the PoissonDiscSampler to create and store seen and hidden spots (from the avoidee) for the avoider to move to
    private void SampleValidMoves()
    {
        if (++sampleCount > sampleCountMax) return;

        PoissonDiscSampler sampler = new PoissonDiscSampler(visionRange*2, visionRange*2, 1f);
        foreach (Vector2 sample in sampler.Samples())
        {
            Vector2 offsetSample = new Vector2(transform.position.x + sample.x - visionRange, transform.position.z + sample.y - visionRange);
            
            if (IsSampleHidden(offsetSample))
                samplesHidden.Add(offsetSample);
            else
                samplesSeen.Add(offsetSample);
        }
    }

    // Performs raycasts to see if the sample point is hidden from the avoidee
    private bool IsSampleHidden(Vector2 offsetSample)
    {
        bool isHidden;
        Vector3 offsetSampleCoverted = new Vector3(offsetSample.x, 1, offsetSample.y);
        float offsetDistFromAvoidee = Vector3.Distance(objToAvoid.transform.position, offsetSampleCoverted);

        // Check that the avoidee does not have a line of sight to the sample position
        if (Physics.Raycast(objToAvoid.transform.position, offsetSampleCoverted - objToAvoid.transform.position, out RaycastHit hitInfo, offsetDistFromAvoidee))
        {
            // Sample position is behind the Avoider and therefore seen by the avoidee
            if (hitInfo.collider.gameObject.GetComponent<Avoider>())
            {
                isHidden = false;
            }
            // Sample position is behind anything that isn't the avoider
            else
            {
                float offsetDistFromAvoider = Vector3.Distance(transform.position, offsetSampleCoverted);
                // Check that the sample position isn't obstructed by the avoidee
                if (Physics.Raycast(transform.position, offsetSampleCoverted - transform.position, out RaycastHit hitInfo2, offsetDistFromAvoider))
                {
                    // The point is obstructed by the avoidee
                    if (hitInfo2.collider.gameObject.GetComponent<Avoidee>())
                    {
                        isHidden = false;
                    }
                    // The point is obstructed by anything other than the avoidee
                    else
                    {
                        isHidden = true;
                    }
                }
                // Raycast from avoider to point is not obstructed & hidden
                else
                {
                    isHidden = true;
                }
            }
        }
        //Sample position does not have an obstruction from avoidee
        else
        {
            isHidden = false;
        }

        return isHidden;
    }

    // Determines the furthest hidden point generated by the sampler and sets the avoider's destination to it
    private void MoveToFurthestHiddenPoint()
    {
        float furthestDistance = 0f;
        furthestHiddenPos = transform.position;
        Vector2 furthestPoint = Vector2.zero;

        for (int i = 0; i < samplesHidden.Count(); i++)
        {
            Vector2 convertedPos = new Vector2(transform.position.x, transform.position.z);
            float dist = Vector2.Distance(convertedPos, samplesHidden[i]);

            if (dist > furthestDistance)
            {
                furthestDistance = dist;
                furthestPoint = samplesHidden[i];
                furthestHiddenPos = new Vector3(furthestPoint.x, 1, furthestPoint.y);
            }
        }
        agent.SetDestination(furthestHiddenPos);
        
        // removed so it can be visualized by gizmos
        samplesHidden.Remove(furthestPoint);
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Visualize the line of sight between the avoider and the avoidee in white
        if (objToAvoid != null)
        {
            // Visualize the line of sight between the avoider and the avoidee
            Gizmos.color = Color.white;
            Gizmos.DrawRay(objToAvoid.transform.position, transform.position - objToAvoid.transform.position);
        }
        
        // Visualize the furthest point in yellow
        if (furthestHiddenPos != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, furthestHiddenPos - transform.position);
        }

        // Visualize the samples seen by the avoidee in red and the samples hidden in green
        if (samplesSeen != null)
        {   
            DrawSamples(samplesSeen, Color.red);
        }
        if (samplesHidden != null)
        {
            DrawSamples(samplesHidden, Color.green);
        }
    }

    // Visualizes the sample points that the avoider tries to move to every tick
    private void DrawSamples(List<Vector2> samplePoints, Color lineColor)
    {
        List<Vector3> pointsToDraw = new List<Vector3>();
        foreach (Vector2 point in samplePoints)
        {
            pointsToDraw.Add(transform.position);
            pointsToDraw.Add(new Vector3(point.x, 1, point.y));

            // debugging raycast from Avoidee
            // Gizmos.color = Color.blue;
            // Vector3 raycastDirection = new Vector3(point.x, 1, point.y) - objToAvoid.transform.position;
            // Gizmos.DrawRay(objToAvoid.transform.position, raycastDirection);
        }
        Gizmos.color = lineColor;
        Gizmos.DrawLineList(pointsToDraw.ToArray());
    }
}
