using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.iOS;

public class OurAgent : Agent
{
    private Rigidbody sphereRb;
    public Transform destination;

    public float multiplier = 5f;

    void Start()
    {
        sphereRb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if(transform.localPosition.y < 0)
        {
            sphereRb.angularVelocity = Vector3.zero;
            sphereRb.linearVelocity = Vector3.zero;
            transform.localPosition = new Vector3(0f, 0.5f, 0f);
        }

        destination.localPosition = new Vector3(Random.value * 8.5f - 4, 0.5f, Random.value * 8.5f - 4);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(sphereRb.linearVelocity.x);
        sensor.AddObservation(sphereRb.linearVelocity.y);

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(destination.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;

        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];

        sphereRb.AddForce(controlSignal * multiplier);

        float distanceToDestination = Vector3.Distance(transform.localPosition, destination.localPosition);
        if(distanceToDestination < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        if(transform.localPosition.y < 0)
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");

    }
}
