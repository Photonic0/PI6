using UnityEngine;

namespace Assets.Helpers
{
    public static class Random2
    {
        public static bool Percent(float chance)
        {
            return chance >= Random.value;
        }
        public static bool Bool => Random.Range(0, 2) == 0;
        public static Vector2 Direction
        {
            get
            {
                float angle = Random.value * Mathf.PI * 2;
                return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            }
        }
        public static bool XInY(int x, int y)
        {
            //example
            //x = 2, y = 5
            //0 true
            //1 true
            //2 false
            //3 false
            //4 false
            //5 false
            return Random.Range(0, y) < x;
        }
        public static bool OneIn(int x)
        {
            return Random.Range(0, x) == 0;
        }
        public static Vector2 CenteredRect(float width, float height)
        {
            float x = width * Random.value;
            float y = height * Random.value;
            x -= width / 2;
            y -= height / 2;
            return new Vector2(x, y);
        }
        public static Vector2 CircleEdge(float radius)
        {
            float angle = Random.value * Mathf.PI * 2;
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }
        public static Vector2 Circular(float radius)
        {
            float dist = Random.value;
            dist = Mathf.Sqrt(dist);//even out distribution of points
            dist *= radius;
            float angle = Random.value * Mathf.PI * 2;
            return new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
        }
        public static Vector2 Ring(float minDist, float maxDist)
        {
            float minDistNormalized = minDist / maxDist;
            minDistNormalized *= minDistNormalized;
            float dist = Random.value * (1 - minDistNormalized) + minDistNormalized;
            dist = Mathf.Sqrt(dist);//even out distribution of points
            dist *= maxDist;
            float angle = Random.value * Mathf.PI * 2;
            return new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
        }

        public static float FloatWithExcludedRange(float min, float max, float excludedRangeMin, float excludedRangeMax)
        {
            excludedRangeMax = Mathf.Clamp(excludedRangeMax, min, max);
            excludedRangeMin = Mathf.Clamp(excludedRangeMin, min, excludedRangeMax);
            float lowerRange = excludedRangeMin - min;
            float upperRange = max - excludedRangeMax;
            float totalRange = lowerRange + upperRange;
            float randomValue = Random.value * totalRange;
            if (randomValue < lowerRange)
            {
                return min + randomValue;
            }
            else
            {
                return excludedRangeMax + (randomValue - lowerRange);
            }
        }

        public static float Float(float min, float max)
        {
            return Random.value * (max - min) + min;
        }
        public static float Float(float max)
        {
            return Random.value * max;
        }
    }
}
