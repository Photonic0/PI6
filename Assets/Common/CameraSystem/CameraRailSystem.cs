using Assets.Helpers;
using Assets.Systems;
using UnityEngine;
public class CameraRailSystem : MonoBehaviour
{
    public enum PositionSamplingMode
    {
        Horizontal,
        Vertical,
        NearestPointOnLine
    }
    public static CameraRailSystem instance;
    public CameraRailPointData[] railPoints;
    [SerializeField] CameraRailPointData secondPoint;
    [SerializeField] CameraRailPointData closest;
    [SerializeField] bool debug_clickToUpdatePositionsArray;
    [SerializeField] Transform parentTransform;
    [SerializeField] Transform cameraTransform;
    [SerializeField] PositionSamplingMode positionSamplingMode;
    static bool shouldSnapNextUpdate;
    public float cameraMoveSpeed;
    private static CameraRailPointData dummyPoint;
    private void Awake()
    {
        dummyPoint = new()
        {
            position = new Vector2(9999999, 9999999)//can't be infinity because
                                                    //that causes NaN issues.
        };
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        UpdatePositionsArray();
    }

    private void UpdatePositionsArray()
    {
        for (int i = 0; i < railPoints.Length; i++)
        {
            railPoints[i].position = railPoints[i].transform.position;
        }
    }
    //find closest
    //dot product comparison between previous and next point
    //choose based on that
    private void FixedUpdate()
    {
        if (debug_clickToUpdatePositionsArray)
        {
            debug_clickToUpdatePositionsArray = false;
            UpdatePositionsArray();
        }
        GetFinalPosition(out Vector3 currentPos, out Vector3 finalPos);
        if (shouldSnapNextUpdate)
        {
            parentTransform.position = finalPos;
        }
        else
        {
            float moveSpeedMult = Mathf.InverseLerp(0, .2f, Vector2.Distance(currentPos, finalPos));
            parentTransform.position = Vector3.MoveTowards(currentPos, finalPos, Time.fixedDeltaTime * cameraMoveSpeed * moveSpeedMult);
        }
        //float t = 0;
        //if(Mathf.Abs(secondPoint.position.x - closest.position.x) > 0.00001f)
        //{
        //    t = Mathf.InverseLerp(secondPoint.position.x, closest.position.x, finalPos.x);
        //}
        //else if(Mathf.Abs(secondPoint.position.y - closest.position.y) > 0.00001f)
        //{
        //    t = Mathf.InverseLerp(secondPoint.position.y, closest.position.y, finalPos.y);  
        //}
        //Vector3 lerpedOffsetPositions = Vector3.Lerp(secondPoint.OffsetPosition, closest.OffsetPosition, t);
        //lerpedOffsetPositions.z = cameraTransform.position.z;
        //cameraTransform.position = lerpedOffsetPositions;
    }

    private void GetFinalPosition(out Vector3 currentPos, out Vector3 finalPos)
    {
        Vector2 targetPos = GameManager.PlayerPosition;
        secondPoint = railPoints[0];
        closest = dummyPoint;
        int indexOfClosest = 0;
        for (int i = 0; i < railPoints.Length; i++)
        {
            CameraRailPointData possiblyClosest = railPoints[i];
            if (ACloserThanB(possiblyClosest.position, closest.position, targetPos))
            {
                closest = possiblyClosest;
                indexOfClosest = i;
            }
        }
        float dotDifference = -2;
        if (indexOfClosest == 0)
        {
            secondPoint = railPoints[1];
        }
        else if (indexOfClosest == railPoints.Length - 1)
        {
            secondPoint = railPoints[^2];
        }
        else
        {
            CameraRailPointData prevPoint = railPoints[indexOfClosest - 1];
            CameraRailPointData nextPoint = railPoints[indexOfClosest + 1];
            ChooseSecondPoint(prevPoint, nextPoint, targetPos, ref dotDifference);
        }
        currentPos = parentTransform.position;
        finalPos = SampleNearestPoint(secondPoint.OffsetPosition, closest.OffsetPosition, GameManager.PlayerPosition);
        dotDifference = Mathf.Abs(dotDifference);
        if (positionSamplingMode == PositionSamplingMode.NearestPointOnLine)
        {
            finalPos = Vector3.Lerp(finalPos, closest.OffsetPosition, Mathf.InverseLerp(0.2f, 0, dotDifference));
        }
        finalPos.z = currentPos.z;
    }

    public void ChooseSecondPoint(CameraRailPointData prevPoint, CameraRailPointData nextPoint, Vector2 targetPos, ref float dotDifference)
    {
        if (positionSamplingMode == PositionSamplingMode.NearestPointOnLine)
        {
            Vector2 dirToClosest = (closest.position - targetPos).normalized;
            Vector2 dirToNext = (closest.position - nextPoint.position).normalized;
            Vector2 dirToPrev = (closest.position - prevPoint.position).normalized;
            dotDifference = Vector2.Dot(dirToClosest, dirToPrev) - Vector2.Dot(dirToClosest, dirToNext);
            if (dotDifference > 0)
            {
                secondPoint = prevPoint;
            }
            else
            {
                secondPoint = nextPoint;
            }
        }
        else if (positionSamplingMode == PositionSamplingMode.Vertical)
        {
            //determine if the closest point is between the prev point and target pos
            //if it is, choose the next point.
            if (closest.position.y > prevPoint.position.y && closest.position.y < targetPos.y)
            {
                secondPoint = nextPoint;
            }
            else
            {
                secondPoint = prevPoint;
            }
        }
        else//positionSamplingMode == horizontal
        {
            //determine if the closest point is between the prev point and target pos
            //if it is, choose the next point.
            if (closest.position.x > prevPoint.position.x && closest.position.x < targetPos.x)
            {
                secondPoint = nextPoint;
            }
            else
            {
                secondPoint = prevPoint;
            }
        }
    }
    public bool ACloserThanB(Vector2 pointA, Vector2 pointB, Vector2 target) => positionSamplingMode switch
    {
        PositionSamplingMode.Horizontal => Mathf.Abs(pointA.x - target.x) < Mathf.Abs(pointB.x - target.x),
        PositionSamplingMode.Vertical => Mathf.Abs(pointA.y - target.y) < Mathf.Abs(pointB.y - target.y),
        _ => (pointA - target).sqrMagnitude < (pointB - target).sqrMagnitude,
    };
    public Vector2 SampleNearestPoint(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        
        switch (positionSamplingMode)
        {
            case PositionSamplingMode.Horizontal:
                point.x = Mathf.Clamp(point.x, Mathf.Min(lineStart.x, lineEnd.x), Mathf.Max(lineStart.x, lineEnd.x));
                point.y = Helper.Remap(point.x, lineStart.x, lineEnd.x, lineStart.y, lineEnd.y, Easings.InOutSineClamped);
                return point;
            case PositionSamplingMode.Vertical:
                point.y = Mathf.Clamp(point.y, Mathf.Min(lineStart.y, lineEnd.y), Mathf.Max(lineStart.y, lineEnd.y));
                point.x = Helper.Remap(point.y, lineStart.y, lineEnd.y, lineStart.x, lineEnd.x, Easings.InOutSineClamped);
                return point;
            case PositionSamplingMode.NearestPointOnLine:
                return NearestPointOnFiniteLine(lineStart, lineEnd, point);
            default:
                return point;
        }
    }
    public static Vector2 NearestPointOnFiniteLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 line = (lineEnd - lineStart);
        float len = line.magnitude;
        if (len > 0)
        {
            line.Normalize();
        }
        Vector2 v = point - lineStart;
        float d = Vector2.Dot(v, line);
        d = Mathf.Clamp(d, 0f, len);
        return lineStart + line * d;
    }
    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num, offset;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//

        // alpha tests//
        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }

        // check if they are parallel
        if (f == 0) return false;

        // compute intersection coordinates //
        num = d * Ax;   // numerator //
        offset = SameSign(num, f) ? f * 0.5f : -f * 0.5f;  // round direction //
        intersection.x = p1.x + (num + offset) / f;

        num = d * Ay;
        offset = SameSign(num, f) ? f * 0.5f : -f * 0.5f;
        intersection.y = p1.y + (num + offset) / f;

        return true;
    }
    private static bool SameSign(float a, float b) => (a * b) >= 0f;
    public static void QueueCameraPositionSnap()
    {
        //execution order issues -.-
        shouldSnapNextUpdate = true;
    }
    private void OnDrawGizmos()
    {
        for (int i = 1; i < railPoints.Length; i++)
        {
            Gizmos.DrawLine(railPoints[i - 1].position, railPoints[i].position);
        }
        Gizmos.color = Color.blue;
        for (int i = 1; i < railPoints.Length; i++)
        {
            Gizmos.DrawLine(railPoints[i - 1].OffsetPosition, railPoints[i].OffsetPosition);
        }
        Gizmos.color = Color.green;
        for (int i = 0; i < railPoints.Length; i++)
        {
            if (railPoints[i].offset != Vector2.zero)
            {
                Gizmos.DrawLine(railPoints[i].position, railPoints[i].OffsetPosition);
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(closest.position, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(secondPoint.position, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(closest.OffsetPosition, secondPoint.OffsetPosition);
        if (!Application.isPlaying)
        {
            debug_clickToUpdatePositionsArray = false;
            UpdatePositionsArray();
        }
    }
}
