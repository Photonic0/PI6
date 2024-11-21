using UnityEngine;

public class SetTransformsSimmetric : MonoBehaviour
{
    [SerializeField] Transform[] source;
    [SerializeField] Transform symmetryPoint;
    [SerializeField] Transform[] transformsToSet;
    [SerializeField] bool symmetryX;
    [SerializeField] bool symmetryY;
    [SerializeField] bool copyX;
    [SerializeField] bool copyY;
    [SerializeField] bool checkToSet;
    private void OnDrawGizmos()
    {
        if (checkToSet)
        {
            Vector3 center = this.symmetryPoint.position;
            for (int i = 0; i < source.Length; i++)
            {
                Vector3 source = this.source[i].position;
                Vector3 result = transformsToSet[i].position;

                if (symmetryX)
                {
                    result.x = center.x + (center.x - source.x);
                }
                else if (copyX)
                {
                    result.x = source.x;
                }
                if (symmetryY)
                {
                    result.y = center.y + (center.y - source.y);
                }
                else if (copyY)
                {
                    result.y = source.y;
                }
                transformsToSet[i].position = result;
            }
            checkToSet = false;
        }
    }
}
