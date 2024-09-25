using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class Avoider : MonoBehaviour
{
    //Require the object I need to avoid (avoidee)
    [SerializeField] private Avoidee objToAvoid;
//Require the range that I do not allow the avoidee to approach
    [SerializeField] private float visionRange;
//Require if I need to visualize
    [SerializeField] private float agentSpeed;
    [SerializeField] private bool showGizmos;

    private List<Vector2> movesToDraw = new List<Vector2>();
    private int moveCount;
    private int moveCountMax = 50;

    private float timeElapsed;

/* GENERAL LOOP */
//Do this forever
    private void Update()
    {
         //Can the avoidee see me?
   //No: Wait a bit and check again
   //Yes: Is there a place to run? (is the candidate list empty?)  
       //No: Wait a bit and check again
       //Yes: Tell the agent to move there (which point in the candidate list is closest?)
        if (Physics.Raycast(objToAvoid.transform.position, transform.position - objToAvoid.transform.position, out RaycastHit hitInfo))
        {
            if (hitInfo.collider.gameObject.GetComponent<Avoider>())
            {
                SampleValidMoves();
            }
            else
            {
                print("Avoidee cannot see avoider!");
            }
        }

        if (timeElapsed > 1f)
        {
            timeElapsed = 0f;
            movesToDraw = new List<Vector2>();
        }

        timeElapsed += Time.deltaTime;
    }

    /* FIND A SPOT */
//Create a PoissonDiscSampler
//Create a collection to store candidate hiding spots
//Foreach point visualize a line to it
//Foreach point in the PoissonDiscSampler, can the avoidee see it? (check visibility to point)
   //Yes: ignore that point
   //No: add the point to the candidate list
    private void SampleValidMoves()
    {
        if (movesToDraw.Count() >= moveCountMax) return;

        PoissonDiscSampler sampler = new PoissonDiscSampler(visionRange*2, visionRange*2, 1f);
        foreach (Vector2 sample in sampler.Samples())
        {
            Vector2 offsetSample = new Vector2(sample.x - visionRange, sample.y - visionRange);
            movesToDraw.Add(offsetSample);
        }

        // foreach (Vector2 point in sampler.Samples())
        // {
        //     print(point);
        // }
    }


/* CHECK VISIBILITY TO POINT */
//Create a ray from one point to another
//Check if the ray hits something that is not the player (avoidee)
   //NO: The point is visible
   //YES: The point is not visible
   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (showGizmos)
        {
            Gizmos.DrawRay(objToAvoid.transform.position, transform.position - objToAvoid.transform.position);

            List<Vector3> validMoves = new List<Vector3>();
            foreach (Vector2 point in movesToDraw)
            {
                validMoves.Add(transform.position);
                validMoves.Add(new Vector3(point.x, 0, point.y));
            }
            Gizmos.DrawLineList(validMoves.ToArray());
        }
    }
}
