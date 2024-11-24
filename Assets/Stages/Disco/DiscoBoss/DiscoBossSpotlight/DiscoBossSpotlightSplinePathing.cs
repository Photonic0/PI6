using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class DiscoBossSpotlightSplinePathing : MonoBehaviour
{
    public Transform[] pathTransforms;
    public bool c2Continuous;
    public Vector2[][] path;
    public int SplineCount => path == null ? 0 : path.Length;
    public void Start()
    {
#if UNITY_EDITOR
        SplinePreviewGUI.AddToDisplay(this);
#endif
        if (pathTransforms == null)
            return;
        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] == null)
            {
                System.Array.Resize(ref pathTransforms, i);
                break;
            }
        }
        int bezierCount = (pathTransforms.Length - 2) / 2;
        path = new Vector2[bezierCount][];
        path[0] = new Vector2[] { pathTransforms[0].position, pathTransforms[1].position, pathTransforms[2].position, pathTransforms[3].position };
#if UNITY_EDITOR
        pathTransforms[0].gameObject.name = "key point 0";
        pathTransforms[1].name = "influence A 0";
        pathTransforms[2].name = "influence B 0";
        if ((pathTransforms.Length - 4) % 2 == 0)
        {
            pathTransforms[^1].name = "key point " + bezierCount;
        }
#endif
        for (int i = 1; i < bezierCount; i++)
        {
#if UNITY_EDITOR
            pathTransforms[i * 2 + 1].gameObject.name = "key point " + i.ToString();
            pathTransforms[i * 2 + 2].name = "influence B " + i;
#endif
            Vector2 start = pathTransforms[i * 2 + 1].position;
            Vector2 previous2ndInfluence = path[i - 1][2]; //pathTransforms[i * 2].position;
            Vector2 end = pathTransforms[i * 2 + 3].position;
            Vector2 influence1 = start + (start - previous2ndInfluence);
            Vector2 influence2;

            if (c2Continuous)
            {
                Vector2 previous1stInfluence = path[i - 1][1];
                //p3 (end of previous bezier) is start
                influence2 = previous1stInfluence + 4 * (start - previous2ndInfluence);
                pathTransforms[i * 2 + 2].position = influence2;
            }
            else
            {

                influence2 = pathTransforms[i * 2 + 2].position;
            }
            path[i] = new Vector2[]
            {
                start,
                influence1,
                influence2,
                end
            };
        }
        bool deleteTransforms = true;
#if UNITY_EDITOR
        deleteTransforms = false;
#endif
        if (deleteTransforms)
        {
            for (int i = 0; i < pathTransforms.Length; i++)
            {
                Destroy(pathTransforms[i].gameObject);
            }
            pathTransforms = null;
        }
    }
    static Vector2 CubicBezier(Vector2 influence2, Vector2 start, Vector2 end, Vector2 influence1, float t)
    {
        float tSqr = t * t;
        float tCube = tSqr * t;

        return start * (-tCube + 3 * tSqr - 3 * t + 1) +
            influence1 * (3 * tCube - 6 * tSqr + 3 * t) +
            influence2 * (-3 * tCube + 3 * tSqr) +
            end * tCube;
    }
    public Vector2 SampleClamped(float t)
    {
        if(path == null)
        {
            Start();
        }
        t = Mathf.Clamp(t, 0, path.Length);
        int index = (int)t;
        t %= 1;
        if (index > path.Length - 1)
        {
            t = 1;
            index = path.Length - 1;
        }
        Vector2 start = path[index][0];
        Vector2 inf1 = path[index][1];
        Vector2 inf2 = path[index][2];
        Vector2 end = path[index][3];
        return CubicBezier(inf2, start, end, inf1, t);
    }
#if UNITY_EDITOR

    private void Update()
    {
        if (populateBezierPointList || constantLinePointUpdate)
        {
            Start();
            populateBezierPointList = false;
        }
    }
    [Header("debug fields")]
    [SerializeField] bool populateBezierPointList;
    [SerializeField] int bezierGizmoLineSegmentsForApproximation;
    [SerializeField] bool constantLinePointUpdate;
    public bool showHandles;
    [Range(0f, 1f)]
    [SerializeField] float debug_controlPointIndicatorTransparency = .5f;
    [SerializeField] bool theObjToTriggerEditorUpdate;
    private void OnDrawGizmos()
    {

        if (bezierGizmoLineSegmentsForApproximation == 0)
            return;

        if (path == null || path.Length == 0)
        {
            return;
        }

        if (!Application.isPlaying && theObjToTriggerEditorUpdate)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
        float step = 1f / bezierGizmoLineSegmentsForApproximation;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(path[0][1], .1f);//this is the exception to can't drag inf1
        if (c2Continuous)
        {
            Gizmos.DrawWireSphere(path[0][2], .1f);
        }
        float timer = (float)(Time.timeAsDouble % 1);
        for (int i = 0; i < path.Length; i++)
        {
            Gizmos.color = i == 0 ? Color.green : Color.white;
            Vector2 start = path[i][0];
            Vector2 inf1 = path[i][1];
            Vector2 inf2 = path[i][2];
            Vector2 end = path[i][3];
            //don't draw for start or else we'd be drawing the start of every point twice, as the end of one is the start of the next
            //don't draw for inf1 since you can't drag around point inf1
            if (!c2Continuous)//if it's c2 continuous then can't even control influence 2
            {
                Gizmos.DrawWireSphere(inf2, .1f);
            }
            Gizmos.DrawWireSphere(end, .1f);

            for (float t = 0; t < 1; t += step)
            {
                Gizmos.DrawLine(CubicBezier(inf2, start, end, inf1, t), CubicBezier(inf2, start, end, inf1, t + step));
            }
            Gizmos.color = new Color(1, 1, 1, debug_controlPointIndicatorTransparency);
            Gizmos.DrawLine(start, inf1);
            Gizmos.DrawLine(end, inf2);

            Gizmos.color = Color.red;
            //Gizmos.DrawSphere(CubicBezier(inf2, start, end, inf1, timer), .1f);
        }
    }
    public void AddTransform(Transform transform)
    {
        System.Array.Resize(ref pathTransforms, pathTransforms.Length + 1);
        pathTransforms[^1] = transform;
    }
}
[CustomEditor(typeof(DiscoBossSpotlightSplinePathing))]
class SplinePreviewGUI : Editor
{
    static List<DiscoBossSpotlightSplinePathing> paths;
    public static void AddToDisplay(DiscoBossSpotlightSplinePathing displayToAdd)
    {
        if (paths == null)
        {
            paths = new(1);
        }
        if (paths.Contains(displayToAdd))
        {
            return;
        }
        paths.Add(displayToAdd);
    }

    private void OnSceneGUI()
    {

        for (int j = 0; j < paths.Count; j++)
        {
            DiscoBossSpotlightSplinePathing spline = paths[j];
            if (Selection.activeGameObject == spline.gameObject)
            {
                for (int i = 0; i < spline.pathTransforms.Length; i++)
                {
                    Transform transform = spline.pathTransforms[i];
                    if ((i < 4 || i % 2 != 0 || !spline.c2Continuous) && spline.showHandles)
                    {
                        transform.position = Handles.DoPositionHandle(transform.position, Quaternion.identity);
                       
                    }
                }
            }
        }
    }
#endif
}

