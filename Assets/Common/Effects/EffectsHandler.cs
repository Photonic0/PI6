using Assets.Common.Consts;
using UnityEngine;

public static class EffectsHandler
{
    public static void SpawnSmallExplosion(FlipnoteColors.ColorID id, Vector2 position, float duration = 0.15f)
    {
        Color col = FlipnoteColors.GetColor(id);
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionSmall, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnMediumExplosion(FlipnoteColors.ColorID id, Vector2 position, float duration = 0.25f)
    {
        Color col = FlipnoteColors.GetColor(id);
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionMedium, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnBigExplosion(FlipnoteColors.ColorID id, Vector2 position, float duration = .4f)
    {
        Color col = FlipnoteColors.GetColor(id);
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionBig, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnSmallExplosion(Color col, Vector2 position, float duration = 0.15f)
    {
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionSmall, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnMediumExplosion(Color col, Vector2 position, float duration = 0.25f)
    {
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionMedium, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnBigExplosion(Color col, Vector2 position, float duration = .4f)
    {
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionBig, position, Quaternion.identity);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
    public static void SpawnSmallExplosion(Transform parent, Color col, Vector2 position, float duration = 0.15f)
    {
        GameObject obj = Object.Instantiate(CommonPrefabs.ExplosionSmall, position, Quaternion.identity, parent);
        obj.transform.Rotate(0, 0, 90 * Random.Range(0, 4));//for a bit more visual variety
        obj.GetComponent<SpriteRenderer>().color = col;
        Object.Destroy(obj, duration);
    }
}
