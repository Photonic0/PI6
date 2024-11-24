using Assets.Helpers;
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
    [SerializeField] bool setRotations;
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
                Vector2 sourceRotation = (transform.rotation.eulerAngles.z * Mathf.Deg2Rad).PolarVector();

                if (symmetryX)
                {
                    result.x = center.x + (center.x - source.x);
                    sourceRotation.x = -sourceRotation.x;
                }
                else if (copyX)
                {
                    result.x = source.x;
                }
                if (symmetryY)
                {
                    result.y = center.y + (center.y - source.y);
                    sourceRotation.y = -sourceRotation.y;
                }
                else if (copyY)
                {
                    result.y = source.y;
                }
                transformsToSet[i].position = result;
                if (setRotations)
                {
                    transformsToSet[i].rotation = sourceRotation.ToRotation();
                }
            }
            checkToSet = false;
        }
    }
}
