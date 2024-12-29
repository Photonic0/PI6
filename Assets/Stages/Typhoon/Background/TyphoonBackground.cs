using Assets.Helpers;
using UnityEngine;

public class TyphoonBackground : MonoBehaviour
{
    [SerializeField] TyphoonBackgroundCloudParticle[] cloudParticles;
    float spawnCooldown;
    int nextCloudIndex;//keep things varied
    Camera cam;
    void Start()
    {
        cam = Camera.main;
    }

    void FixedUpdate()
    {
        spawnCooldown -= Time.fixedDeltaTime;
        if (spawnCooldown < 0)
        {
            for (int i = 0; i < 10; i++)
            {
                TyphoonBackgroundCloudParticle cloudParticle = cloudParticles[nextCloudIndex];
                nextCloudIndex = (int)Mathf.Repeat(nextCloudIndex + Random.Range(1, 4), cloudParticles.Length);
                if (cloudParticle.gameObject.activeInHierarchy)
                {
                    continue;
                }
                spawnCooldown = .15f;
                cloudParticle.velocity.x = Random2.Float(2, 6) * (Random.Range(0,2) * 2 - 1);
                Vector3 pos = cloudParticle.transform.position;
                float z = Random2.Float(5, 16);
                GetCameraBounds(out float minX, out float maxX, out float maxY, out float minY, 3, z);
                pos.x = Random2.Float(minX, maxX);
                pos.y = Random2.Float(minY, maxY);
                pos.z = z;
                cloudParticle.transform.position = pos;
                cloudParticle.SetFlipX();
                cloudParticle.gameObject.SetActive(true);
                break;
            }
        }
    }
    void GetCameraBounds(out float minX, out float maxX, out float maxY, out float minY, float padding = 0, float targetZPos = 0)
    {
        Vector3 cameraPos = cam.transform.position;
        float viewHeight = 2f * Mathf.Abs(targetZPos - cameraPos.z) * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * .5f);
        float viewWidth = viewHeight * cam.aspect;
        float cameraX = cameraPos.x;
        float cameraY = cameraPos.y;
        minX = cameraX - viewWidth * .5f - padding;
        maxX = cameraX + viewWidth * .5f + padding;
        minY = cameraY - viewHeight * .5f - padding;
        maxY = cameraY + viewHeight * .5f + padding;
    }
}
