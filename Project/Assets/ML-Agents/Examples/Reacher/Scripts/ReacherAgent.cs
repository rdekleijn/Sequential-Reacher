using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class ReacherAgent : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject hand;
    public GameObject goal;
    public GameObject sphere;
    public int activeTarget;
    public int timeTargetTouched;
    public int timeTargetActive;
    public bool justTouchedTarget = false;
    public float moveSpeed = 0f;
    float m_GoalDegree;
    Rigidbody m_RbA;
    Rigidbody m_RbB;
    // speed of the goal zone around the arm (in radians)
    float m_GoalSpeed;
    // radius of the goal zone
    float m_GoalSize;
    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
    float m_Deviation;
    // Frequency of the cosine deviation of the goal along the vertical dimension
    float m_DeviationFreq;

    StatsRecorder m_Recorder;


    EnvironmentParameters m_ResetParams;

    /// <summary>
    /// Collect the rigidbodies of the reacher in order to resue them for
    /// observations and actions.
    /// </summary>
    public override void Initialize()
    {
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    /// <summary>
    /// We collect the normalized rotations, angularal velocities, and velocities of both
    /// limbs of the reacher as well as the relative position of the target and hand.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.angularVelocity);
        sensor.AddObservation(m_RbA.velocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.angularVelocity);
        sensor.AddObservation(m_RbB.velocity);

        sensor.AddObservation(goal.transform.localPosition);
        sensor.AddObservation(hand.transform.localPosition);


        // we will use this to determine if target touched
        if (justTouchedTarget)
        {
            sensor.AddObservation(1.0f);
        } else
        {
            sensor.AddObservation(0.0f);
        }

        moveSpeed = m_RbA.velocity.magnitude + m_RbB.velocity.magnitude;

        //Debug.Log("Dist " + Vector3.Distance(sphere.transform.position, hand.transform.position));
        Debug.Log("Dist " + (goal.transform.position - transform.position));


        m_Recorder.Add("Distance to base", Vector3.Distance(sphere.transform.position, hand.transform.position));



    }

    /// <summary>
    /// The agent's four actions correspond to torques on each of the two joints.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        m_GoalDegree += m_GoalSpeed;
        UpdateGoalPosition();

        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 150f;
        var torqueZ = Mathf.Clamp(vectorAction[1], -1f, 1f) * 150f;
        m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 150f;
        torqueZ = Mathf.Clamp(vectorAction[3], -1f, 1f) * 150f;
        m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));
    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    void UpdateGoalPosition()
    {

        if (justTouchedTarget && Time.frameCount - timeTargetTouched >= 50)
        {
            bool targetChosen = false;
            while (targetChosen == false)
            {
                int newTarget = Random.Range(0, 4);
                if (newTarget != activeTarget)
                {
                    targetChosen = true;
                    activeTarget = newTarget;
                }
            }

            m_GoalDegree = activeTarget * 90;
            justTouchedTarget = false;
            timeTargetActive = Time.frameCount;
        }


        var radians = m_GoalDegree * Mathf.PI / 180f;
        var goalX = 8f * Mathf.Cos(radians);
        var goalY = 8f * Mathf.Sin(radians);
        var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
        goal.transform.position = new Vector3(goalY, goalZ, goalX) + transform.position;

        GetComponent<ReacherAgent>().AddReward(-0.0001f * moveSpeed); //was 0.00001

        //var statsRecorder = Academy.Instance.StatsRecorder;
        //statsRecorder.Add("Distance to center", Vector3.Distance(new Vector3(0, 0, 0), hand.transform.localPosition));

        //Debug.Log("Dist" + Vector3.Distance(new Vector3(0.1f, 0, 0), hand.transform.localPosition));
    }

    /// <summary>
    /// Resets the position and velocity of the agent and the goal.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        pendulumA.transform.position = new Vector3(0f, -4f, 0f) + transform.position;
        pendulumA.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbA.velocity = Vector3.zero;
        m_RbA.angularVelocity = Vector3.zero;

        pendulumB.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
        pendulumB.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbB.velocity = Vector3.zero;
        m_RbB.angularVelocity = Vector3.zero;

        // m_GoalDegree = Random.Range(0, 360); // old value

        // Here we choose target 0-3
        activeTarget = Random.Range(0, 4);
        m_GoalDegree = activeTarget * 90;
        timeTargetActive = Time.frameCount;

        UpdateGoalPosition();

        SetResetParameters();

        m_Recorder = Academy.Instance.StatsRecorder;

        goal.transform.localScale = new Vector3(m_GoalSize, m_GoalSize, m_GoalSize);
    }


    public void SetResetParameters()
    {
        m_GoalSize = m_ResetParams.GetWithDefault("goal_size", 5);
        m_GoalSpeed = Random.Range(-1f, 1f) * m_ResetParams.GetWithDefault("goal_speed", 0); // was 1
        m_Deviation = m_ResetParams.GetWithDefault("deviation", 0);
        m_DeviationFreq = m_ResetParams.GetWithDefault("deviation_freq", 0);
    }
}
