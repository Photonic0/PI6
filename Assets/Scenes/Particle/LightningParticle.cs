using Assets.Helpers;
using UnityEngine;

public class LightningParticle : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;
    [SerializeField] float timer;
    [SerializeField] float lightningRate;
    [SerializeField] int jointCount;
    [SerializeField] Transform target;
    [SerializeField] Transform origin;
    [SerializeField] float maxDisjoint;
    [SerializeField] Vector3[] jointPositions;
    [SerializeField] float sameDirectionTolerance;
    [SerializeField] int maxAttempts = 15;
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > lightningRate)
        {
            timer %= lightningRate;
            if(target == null || origin == null)
            {
                return;
            }
            float[] jointValues = new float[jointCount];
            for (int i = 0; i < jointCount; i++)
            {
                jointValues[i] = Random.value;
            }
            System.Array.Sort(jointValues);
            jointPositions = new Vector3[jointCount + 2];
            jointPositions[0] = origin.position;
            jointPositions[^1] = target.position;
            for (int i = 1; i < jointCount + 1; i++)
            {
                float lerpVal = jointValues[i - 1];
                Vector3 position = Vector3.Lerp(origin.position, target.position, lerpVal);
                for (int j = 0; j < maxAttempts; j++)
                {
                    Vector3 offset = (Vector3)Random2.Circular(Mathf.Min(maxDisjoint, Vector3.Distance(position, origin.position), Vector3.Distance(position, target.position)));
                    //maybe remap the offset instead of an iterative check?
                    if(Vector3.Dot(offset, jointPositions[i - 1] - position) > sameDirectionTolerance)
                    {
                        position += offset;
                        break; 
                    }
                }
                jointPositions[i] = position;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Update();
        for (int i = 0;i < jointPositions.Length - 1;i++)
        {
            Gizmos.DrawLine(jointPositions[i], jointPositions[i + 1]);
        }
    }
}
