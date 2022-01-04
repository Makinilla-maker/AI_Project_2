using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class RollerAgent : Agent
{
    Rigidbody rBody;
    public Transform raycast;
    public float raycastDistance;
    public Transform[] waypointsBall;
    public Transform[] waypointsTarget;
    [SerializeField] private float distance;
    [SerializeField] private float distanceN;
    [SerializeField] private float distanceS;
    [SerializeField] private float distanceE;
    [SerializeField] private float distanceW;
    public Vector3 startPlayerPos;
    void Start()
    {
        startPlayerPos = transform.position;
        rBody = GetComponent<Rigidbody>();
    }
    public Transform target;
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        int i = Random.Range(0, 8);
        transform.position = waypointsBall[i].position;
        // Move the target to a new spot
        //target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);



    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        sensor.AddObservation(distanceN);
        sensor.AddObservation(distanceS);
        sensor.AddObservation(distanceW);
        sensor.AddObservation(distanceE);

    }
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);
        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);
        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            RecalculatePosition();
            EndEpisode();
        }
        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
        
        distanceN = WhereAmI(raycast.forward);
        distanceE = WhereAmI(raycast.right);
        distanceS = WhereAmI(-raycast.forward);
        distanceW = WhereAmI(-raycast.right);
    }
    void RecalculatePosition()
    {
        //if (target.position == new Vector3(-7.61999989f, 0.519999981f, -14.1199999f)) target.position = new Vector3(-7.84000015f, 0.519999981f, 13.5100002f);
        //else if (target.position == new Vector3(-7.84000015f, 0.519999981f, 13.5100002f)) target.position = new Vector3(11.6099997f, 0.519999981f, 0.119999997f);
        //else if (target.position == new Vector3(11.6099997f, 0.519999981f, 0.119999997f)) target.position = new Vector3(-7.61999989f, 0.519999981f, -14.1199999f);
        //else
        //{
        //    target.localPosition = new Vector3(-7.61999989f, 0.519999981f, -14.1199999f);
        //}
        int y = Random.Range(0, 8);
        target.position = waypointsTarget[y].position;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "wall")
        {
            //SetReward(-0.25f);
            EndEpisode();
        }
    }
    float WhereAmI(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(raycast.position, direction, out hit, raycastDistance))
        {
            Debug.Log(hit.transform.tag);
            if (hit.transform.tag == "wall")
            {
                distance = hit.distance;
                Debug.DrawLine(raycast.position, direction * hit.distance, Color.blue);
                return distance;
            }
        }
        return hit.distance;
    }
}