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
        public bool LightningActive => lifetimeTimer < duration;
        public Vector2 CenterPoint => (start + end) * .5f;
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
            float[] pointsTs = new float[pointsAmount];            //confusing name

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
        public void Stop()
        {
            lifetimeTimer = duration + 1;
            timer = 0;
            gameObject.SetActive(false);
        }
        public void Move(float dy)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            start.y += dy;
            end.y += dy;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i].y += dy;
            }
            lineRenderer.SetPositions(positions);
        }
        //creating a function to adapt all the points of the lightning to a new start and end position
        public void Move(Vector2 newStart, Vector2 newEnd)
        {
            Vector3[] vertexPositions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(vertexPositions);
            for (int i = 1; i < vertexPositions.Length - 1; i++)
            {
                Vector3 currentVertexPos = vertexPositions[i];
                float newX = Helper.Remap(currentVertexPos.x, start.x, end.x, newStart.x, newEnd.x);
                float newY = Helper.Remap(currentVertexPos.y, start.y, end.y, newStart.y, newEnd.y);
                Vector3 newVertexPos = new Vector3(newX, newY);
                vertexPositions[i] = newVertexPos;
            }
            vertexPositions[0] = newStart;
            vertexPositions[^1] = newEnd;
            start = newStart;
            end = newEnd;
            lineRenderer.SetPositions(vertexPositions);
        }
    }
}
