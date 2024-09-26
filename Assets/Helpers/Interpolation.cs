using Assets.DataStructures;
using UnityEngine;

namespace Assets.Helpers
{
    public static class Interpolation
    {
        public static Color HSV(Color c1, Color c2, float t)
        {
            float a = Mathf.Lerp(c1.a, c2.a, t);
            Color.RGBToHSV(c1, out float h1, out float s1, out float v1);
            Color.RGBToHSV(c2, out float h2, out float s2, out float v2);
            h1 *= 360;
            h2 *= 360;
            h1 = Mathf.LerpAngle(h1, h2, t);
            s1 = Mathf.Lerp(s1, s2, t);
            v1 = Mathf.Lerp(v1, v2, t);
            Color result =  Color.HSVToRGB(h1 / 360f, s1, v1);
            result.a = a;
            return result;
        }
        public static float Multi(float t, params float[] points)
        {
            int index = (int)t;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Mathf.LerpUnclamped(points[index], points[index + 1], t % 1);
        }
        public static Vector3 Multi(float t, params Vector3[] points)
        {
            int index = (int)t;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Vector3.LerpUnclamped(points[index], points[index + 1], t % 1);
        }
        public static Vector2 Multi(float t, params Vector2[] points)
        {
            int index = (int)t;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Vector2.LerpUnclamped(points[index], points[index + 1], t % 1);
        }
        public static Color Multi(float t, params Color[] points)
        {
            int index = (int)t;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Color.LerpUnclamped(points[index], points[index + 1], t % 1);
        }
        public static Color HSVMulti(float t, params Color[] points)
        {
            int index = (int)t;
            Debug.Log(t);
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }

            return HSV(points[index], points[index + 1], t % 1);
        }

        public static float Multi(Counter t, params float[] points)
        {
            int index = (int)t.Progress;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Mathf.LerpUnclamped(points[index], points[index + 1], t.Time % 1);
        }
        public static Vector3 Multi(Counter t, params Vector3[] points)
        {
            int index = (int)t.Progress;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Vector3.LerpUnclamped(points[index], points[index + 1], t.Time % 1);
        }
        public static Vector2 Multi(Counter t, params Vector2[] points)
        {
            int index = (int)t.Progress;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Vector2.LerpUnclamped(points[index], points[index + 1], t.Time % 1);
        }
        public static Color Multi(Counter t, params Color[] points)
        {
            int index = (int)t.Progress;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return Color.LerpUnclamped(points[index], points[index + 1], t.Time % 1);
        }
        public static Color HSVMulti(Counter t, params Color[] points)
        {
            int index = (int)t.Progress;
            if (index < 0)
            {
                return points[0];
            }
            if (index + 1 >= points.Length)
            {
                return points[^1];
            }
            return HSV(points[index], points[index + 1], t.Time % 1);
        }
    }
}
