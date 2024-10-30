using Assets.Helpers;
using System;
using UnityEngine;

namespace Assets.Common.Effects.Lightning
{
    [Serializable]
    public class SimpleLightningRenderer : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        float timer;
        float lifetimeTimer;
        float appearanceChangeCooldown;
        float duration;
        float maxPointDeviation;
        Vector2 start;
        Vector2 end;
        public static (GameObject, SimpleLightningRenderer) Create(float appearanceChangeCooldown, Vector2 start, Vector2 end, float duration, bool destroyWhenExpire = true, float maxPointDeviation = 0.8f)
        {
            GameObject obj = Instantiate(CommonPrefabs.SimpleLightningLineRenderer, Vector3.zero, Quaternion.identity);
            SimpleLightningRenderer renderer = obj.GetComponent<SimpleLightningRenderer>();
            renderer.appearanceChangeCooldown = appearanceChangeCooldown;
            renderer.start = start;
            renderer.end = end;
            renderer.maxPointDeviation = maxPointDeviation;
            renderer.ChangeLinePositions();
            renderer.duration = duration;
            if (destroyWhenExpire)
            {
                Destroy(obj, duration);
            }
            return (obj, renderer);
        }
        private void Update()
        {
            lifetimeTimer += Time.deltaTime;
            timer += Time.deltaTime;
            if(lifetimeTimer > duration)
            {
                gameObject.SetActive(false);
                return;
            }
            if(timer > appearanceChangeCooldown)
            {
                timer %= appearanceChangeCooldown;
                ChangeLinePositions();
            }
        }
        public void ActivateAndSetAttributes(float appearanceChangeCooldown, Vector2 start, Vector2 end, float duration, float maxPointDeviation = .8f)
        {
            this.appearanceChangeCooldown = appearanceChangeCooldown;
            this.start = start;
            this.end = end;
            this.maxPointDeviation = maxPointDeviation;
            timer = 0;
            lifetimeTimer = 0;
            ChangeLinePositions();
            this.duration = duration;
            gameObject.SetActive(true);
        }
        private void ChangeLinePositions()
        {
            Vector2 deltaPos = end - start;
            int pointsAmount = (int)Mathf.Max(deltaPos.magnitude, 2) + 2;
            Vector2 direction = deltaPos.normalized;
            Vector2 normal = new(-direction.y, direction.x);
            //confusing name
            float[] pointsTs = new float[pointsAmount];
            pointsTs[0] = 0;
            pointsTs[^1] = 1;
            for (int i = 1; i < pointsAmount - 1; i++)
            {
                pointsTs[i] = UnityEngine.Random.value;
            }
            Array.Sort(pointsTs);
            Vector3[] positions = new Vector3[pointsAmount];
            float prevDeviation = 0;//so the first one is not near the middle
            for (int i = 0; i < pointsAmount; i++)
            {
                float t = pointsTs[i];
                Vector2 position = Vector2.Lerp(start, end, t);
                float dist = Mathf.Min((end - position).magnitude, (start - position).magnitude);
                float deviationMult = Random2.FloatWithExcludedRange(-maxPointDeviation, maxPointDeviation, prevDeviation - (maxPointDeviation / 2f), prevDeviation + (maxPointDeviation / 2f));
                prevDeviation = deviationMult;
                deviationMult *= Mathf.InverseLerp(0, 1, dist);
                position += normal * deviationMult;
                positions[i] = position;
            }
            if (pointsAmount != lineRenderer.positionCount)
            {
                lineRenderer.positionCount = pointsAmount;
            }
            lineRenderer.SetPositions(positions);
        }
    }
}
