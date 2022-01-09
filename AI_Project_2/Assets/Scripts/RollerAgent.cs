using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class RollerAgent : Agent
{
    Rigidbody rBody;
    public float time = 60;
    private float time2;
    private Vector3 startPosition;
    public GameObject posRewards;
    public GameObject negRewards;

    [SerializeField] private bool check;
    void Start()
    {
        startPosition = transform.position;
        rBody = GetComponent<Rigidbody>();
        time2 = time;
        check = false;
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
        transform.position = startPosition;
        time = time2;
        for (int i = 0; i < posRewards.transform.childCount; i++)
        {
            posRewards.transform.GetChild(i).gameObject.SetActive(true); 
        }
        for (int i = 0; i < negRewards.transform.childCount; i++)
        {
            negRewards.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        sensor.AddObservation(check);
        if (check) check = false;
    }

    public float forceMultiplier = 5;

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
            EndEpisode();
        }

        // Countdown
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            EndEpisode();
        }
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
            SetReward(-0.1f);
            check = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "neg")
        {
            SetReward(-0.7f);
            other.gameObject.SetActive(false);
        }
        if(other.tag == "pos")
        {
            SetReward(0.5f);
            other.gameObject.SetActive(false);
        }
        if (other.tag == "trap")
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
}