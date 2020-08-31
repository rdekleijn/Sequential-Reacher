using UnityEngine;
using Unity.MLAgents;


public class ReacherGoal : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;



    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(1f, 1f, 1f);

            if (agent.GetComponent<ReacherAgent>().justTouchedTarget == false)
            {
                agent.GetComponent<ReacherAgent>().justTouchedTarget = true; // record target touched
                agent.GetComponent<ReacherAgent>().timeTargetTouched = Time.frameCount; // record time of target touch
                agent.GetComponent<ReacherAgent>().AddReward(1.0f);

                //Debug.Log("RT " + (agent.GetComponent<ReacherAgent>().timeTargetTouched - agent.GetComponent<ReacherAgent>().timeTargetActive));

                var statsRecorder = Academy.Instance.StatsRecorder;
                statsRecorder.Add("RT", (agent.GetComponent<ReacherAgent>().timeTargetTouched - agent.GetComponent<ReacherAgent>().timeTargetActive));


            }

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == hand)
        {
            //agent.GetComponent<ReacherAgent>().AddReward(0.01f);  // sparse reward!
        }
    }
}
