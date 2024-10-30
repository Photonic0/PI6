using Assets.Helpers;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TyphoonTilesCloudsEffectSpawner : MonoBehaviour
{
    float secsPerCloudSpawn;
    [SerializeField] Tilemap tilemap;
    [SerializeField] TyphoonTilesCloudParticle[] cloudsPool;
    byte[,] tileExistsAndNeighbourCount;//"there are only 2 hard things in computer science"....
    Vector3[] randomSpawnPositions;
    float timer;
    Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        //remove any layering issues
        for (int i = 0; i < cloudsPool.Length; i++)
        {
            Vector3 pos = cloudsPool[i].transform.position;
            pos.z = 0.01f * i;
            cloudsPool[i].transform.position = pos;
        }
        //~10k fps, 0.17ms to run.
        InitializeRandomSpawnPositionsAndSpawnRate();

    }
    private void InitializeRandomSpawnPositionsAndSpawnRate()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int minX = bounds.xMin;
        int minY = bounds.yMin;
        int maxX = bounds.xMax;
        int maxY = bounds.yMax;
        Vector3Int boundsSize = bounds.size;
        int tileCount = 0;
        tileExistsAndNeighbourCount = new byte[boundsSize.x, boundsSize.y];
        for (int n = minX; n < maxX; n++)
        {
            for (int p = minY; p < maxY; p++)
            {
                Vector3Int localPlace = new(n, p);
                if (tilemap.HasTile(localPlace))
                {
                    tileExistsAndNeighbourCount[n - minX, p - minY] = 1;
                    tileCount++;
                }
            }
        }
        randomSpawnPositions = new Vector3[tileCount];
        int lengthX = tileExistsAndNeighbourCount.GetLength(0);
        int lengthY = tileExistsAndNeighbourCount.GetLength(1);
        int index = 0;
        for (int n = minX; n < maxX; n++)
        {
            for (int p = minY; p < maxY; p++)
            {
                int x = n - minX;
                int y = p - minY;
                if ((tileExistsAndNeighbourCount[x, y] & 1) != 0)
                {
                    continue;
                }
                uint neighbourCount = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if ((i == 0 && j == 0) || (x + i) < 0 || (y + j) < 0 || (x + i) >= lengthX || (y + j) >= lengthY)
                            continue;
                        if ((tileExistsAndNeighbourCount[x + i, y + j] & 1) == 0)
                        {
                            neighbourCount++;
                        }
                    }
                }
                //tileExistsAndNeighbourCount[x,y] = ((byte)neighbourCount) << 1;
                if (neighbourCount > 6)
                {
                    Vector3Int localPlace = new(n, p);
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    place.x += .5f;//center the X
                    place.y += .5f;//center the Y
                    if (index < randomSpawnPositions.Length)
                    {
                        randomSpawnPositions[index] = place;
                    }
                    index++;
                }
            }
        }
        int testVal = -1;
        Debug.Log(System.Convert.ToString(testVal, 2));
        System.Array.Resize(ref randomSpawnPositions, index);
        secsPerCloudSpawn = 40f / randomSpawnPositions.Length;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > secsPerCloudSpawn)
        {
            timer %= secsPerCloudSpawn;

            if (Helper.TryFindFreeIndex(cloudsPool, out int index))
            {
                float size = mainCam.orthographicSize * 2 + 5;
                Vector2 cameraPos = mainCam.transform.position;
                for (int i = 0; i < 100; i++)//try 100 times to get a spawn position within range
                {
                    //maybe try an approach based on checking tileExistences so no repeat tries required?
                    Vector3 cloudPos = randomSpawnPositions[Random.Range(0, randomSpawnPositions.Length)];
                    float minX = cameraPos.x - size;
                    float minY = cameraPos.y - size;
                    float maxX = cameraPos.x + size;
                    float maxY = cameraPos.y + size;
                    if (!(cloudPos.x < minX || cloudPos.x > maxX || cloudPos.y > maxY || cloudPos.y < minY))
                    {
                        cloudPos.x += Random.value - 0.5f;
                        cloudPos.y += Random.value - 0.5f;
                        TyphoonTilesCloudParticle cloud = cloudsPool[index];
                        cloud.velocity.x = Random.Range(-1f, 1f);
                        cloudPos.z = cloud.transform.position.z;//keep z position so layering doesn't get messed up
                        cloud.transform.position = cloudPos;
                        cloud.SetFlipX();
                        cloud.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
    //bool PointInsideMainCameraBounds(Vector2 pointToCheck, float extraSize)//idk what to call this last parameter
    //{
        
    //}
}
