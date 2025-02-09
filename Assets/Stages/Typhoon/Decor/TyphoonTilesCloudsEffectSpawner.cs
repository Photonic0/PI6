using Assets.Helpers;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ALSO HANDLES SPAWNING LIGHTNING NODES
/// </summary>
public class TyphoonTilesCloudsEffectSpawner : MonoBehaviour
{
    [SerializeField] bool debug_disableClouds;
    [SerializeField] bool debug_disableLightning;
    [SerializeField] bool debug_spawnLightning;
    [SerializeField] bool debug_spawnCloud;

    [SerializeField] Camera mainCam;
    [SerializeField] Tilemap tilemap;
    [SerializeField] TyphoonTilesCloudParticle[] cloudsPool;
    [SerializeField] TyphoonTilesLightningNodePair[] lightningNodeGroupPool;
    byte[,] tileExistsAndNeighbourCount;//"there are only 2 hard things in computer science"....
    Vector3[] randomSpawnPositions;
    BoundsInt tilemapBounds;
    float cloudSpawnTimer;
    float secsPerCloudSpawn;
    float lightningSpawnTimer;
    [SerializeField] float node1Rotation;
    [SerializeField] float node2Rotation;
    [SerializeField] Vector3 debug_endPointIndicator;
    [SerializeField] Vector3 debug_endPointIndicatorWithBounds;
    private void Start()
    {
        mainCam = Camera.main;
        //remove any layering issues
        for (int i = 0; i < cloudsPool.Length; i++)
        {
            Vector3 pos = cloudsPool[i].transform.position;
            pos.z = 0.001f * i + 0.07f;
            cloudsPool[i].transform.position = pos;
        }
        for (int i = 0; i < lightningNodeGroupPool.Length; i++)
        {
            //ended up having to fix it with layers and not Z position...
            TyphoonTilesLightningNodePair node = lightningNodeGroupPool[i];
            Vector3 pos = new(0, 0, .05f);

            node.transform1.position = pos;
            node.transform2.position = pos;
        }
        //~10k fps, 0.17ms to run.
        InitializeRandomSpawnPositionsAndSpawnRate();

    }

    private void InitializeRandomSpawnPositionsAndSpawnRate()
    {
        BoundsInt bounds = tilemap.cellBounds;
        tilemapBounds = bounds;
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

                int neighbourCount = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if ((i == 0 && j == 0) || (x + i) < 0 || (y + j) < 0 || (x + i) >= lengthX || (y + j) >= lengthY)
                            continue;
                        if ((tileExistsAndNeighbourCount[x + i, y + j] & 1) == 1)
                        {
                            neighbourCount++;
                        }
                    }
                }
                tileExistsAndNeighbourCount[x, y] |= (byte)(neighbourCount << 1);
                if ((tileExistsAndNeighbourCount[x, y] & 1) == 1 && neighbourCount > 6)
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
        System.Array.Resize(ref randomSpawnPositions, index);
        secsPerCloudSpawn = .5f;
    }

    void Update()
    {
        cloudSpawnTimer += Time.deltaTime;
        lightningSpawnTimer += Time.deltaTime;
        if (debug_spawnCloud || (cloudSpawnTimer > secsPerCloudSpawn && !debug_disableClouds))
        {
            cloudSpawnTimer %= secsPerCloudSpawn;
            if (debug_spawnCloud)
            {
                cloudSpawnTimer = 0;
                debug_spawnCloud = false;
            }
            SpawnCloud();
        }
        if (debug_spawnLightning || (lightningSpawnTimer > secsPerCloudSpawn * .7f && !debug_disableLightning))
        {
            lightningSpawnTimer %= secsPerCloudSpawn * .7f;
            if (debug_spawnLightning)
            {
                debug_spawnLightning = false;
                lightningSpawnTimer = 0;
            }
            SpawnLightning();
        }
    }

    private void SpawnCloud()
    {
        if (!Helper.TryFindFreeIndex(cloudsPool, out int index))
        {
            return;
        }
        if (!GetRandomTilePositionInsideCameraBounds(out Vector3 cloudPos))
        {
            return;
        }
        cloudPos.x += Random.value - 0.5f;
        cloudPos.y += Random.value - 0.5f;
        TyphoonTilesCloudParticle cloud = cloudsPool[index];
        //important to set position before sprite bound dependent calculations!!
        cloudPos.z = cloud.transform.position.z;//keep z position so layering doesn't get messed up
        cloud.transform.position = cloudPos;

        float velX = Random.Range(-1f, 1f);
        float distTravelled = velX * TyphoonTilesCloudParticle.ParticleDuration;
        Bounds spriteBounds = cloud.sprite.bounds;
        float spriteHalfWidth = spriteBounds.extents.x;
        float endPointX = 999999;// spriteBounds.center.x + spriteHalfWidth * Mathf.Sign(velX);
        float velXSign = Mathf.Sign(velX);
        //supposed to not let clouds travel outside blocks,
        //sometimes fails because only considers the current row, but good enough.
        for (int j = 0; j <= Mathf.Abs(distTravelled); j++)
        {
            float signedHalfWidth = spriteHalfWidth * velXSign;
            Vector2 endPoint = new(cloudPos.x + ((spriteHalfWidth + j + .5f) * velXSign), cloudPos.y);
            WorldToTileArrayPosition(endPoint, out int x, out int y, out bool xOutOfBounds, out bool yOutOfBounds);
            // j == (int)distTravelled is last iteration of the loop
            if (xOutOfBounds || yOutOfBounds || !TileExists(x, y) || j == (int)Mathf.Abs(distTravelled))
            {
                debug_endPointIndicatorWithBounds.Set(endPoint.x - velXSign, endPoint.y, 0);
                endPointX = endPoint.x - signedHalfWidth - velXSign;//subtract sign so it's 1 block before
                debug_endPointIndicator.Set(endPointX, endPoint.y, 0);
                break;
            }
        }
        cloud.velocity.x = Mathf.Min(Mathf.Abs(distTravelled), Mathf.Abs(cloudPos.x - endPointX)) / TyphoonTilesCloudParticle.ParticleDuration * velXSign;
        cloud.SetFlipX();
        cloud.gameObject.SetActive(true);

    }

    void SpawnLightning()
    {
        const float maxNodeDist = 6.1f;
        const float minNodeDist = 1.1f;

        if (!Helper.TryFindFreeIndex(lightningNodeGroupPool, out int index))
        {
            return;
        }

        for (int i = 0; i < 50; i++)
        {
            if (!GetRandomTilePositionInsideCameraBounds(out Vector3 centralPosVec3))
            {
                return;
            }
            Vector2 centralPos = centralPosVec3;
            Vector2 direction = Random2.Direction;
            Vector2 node1Pos;
            Vector2 node2Pos;
            Vector2 offset;
            Vector2 nextNode1Pos = centralPos;
            Vector2 nextNode2Pos = centralPos;
            Vector2 stepVector = direction / Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            float targetNodeDist = Mathf.Lerp(minNodeDist, maxNodeDist, Random.value);
            bool goToNextLoop;
            //push position outwards and check is the position has room to fit the notes within an accceptable distance
            for (int j = 0; ; j++)
            {
                offset = stepVector * j;

                WorldToTileArrayPosition(centralPos + offset, out int x1, out int y1, out bool x1OutOfBounds, out bool y1OutOfBounds);
                bool nextNode1PosInelligible = x1OutOfBounds || y1OutOfBounds;
                if (!nextNode1PosInelligible)
                {
                    nextNode1PosInelligible = !TileExists(x1, y1) || GetNeighbourCount(x1, y1) <= 7 || OverlapsExistingNodes(x1, y1);
                }
                if (!nextNode1PosInelligible)
                {
                    nextNode1Pos = centralPos + offset;
                }
                WorldToTileArrayPosition(centralPos - offset, out int x2, out int y2, out bool x2OutOfBounds, out bool y2OutOfBounds);
                bool nextNode2PosInelligible = x2OutOfBounds || y2OutOfBounds;
                if (!nextNode2PosInelligible)
                {
                    nextNode2PosInelligible = !TileExists(x2, y2) || GetNeighbourCount(x2, y2) <= 7 || OverlapsExistingNodes(x2, y2);
                }
                if (!nextNode2PosInelligible)
                {
                    nextNode2Pos = centralPos - offset;
                }
                float nodePosDist = Vector2.Distance(nextNode1Pos, nextNode2Pos);
                if (nodePosDist > targetNodeDist || (nextNode1PosInelligible && nextNode2PosInelligible))
                {
                    goToNextLoop = nodePosDist < minNodeDist;
                    node1Pos = nextNode1Pos;
                    node2Pos = nextNode2Pos;
                    break;
                }
            }
            if (goToNextLoop)
            {
                continue;
            }
            //if (!(centralPos.x < minX || centralPos.x > maxX || centralPos.y > maxY || centralPos.y < minY))
            //{
            TyphoonTilesLightningNodePair nodePair = lightningNodeGroupPool[index];
            Vector3 node1Pos3d = new(node1Pos.x, node1Pos.y, nodePair.transform1.position.z);
            Vector3 node2Pos3d = new(node2Pos.x, node2Pos.y, nodePair.transform2.position.z);
            nodePair.timeLeft = 1;
            nodePair.lightningRenderer.ActivateAndSetAttributes(0.1f, node1Pos, node2Pos, .8f);
            nodePair.gameObject1.SetActive(true);
            nodePair.gameObject2.SetActive(true);
            int randomHash = TyphoonTilesLightningNodePair.RandAnimHash;
            nodePair.animator1.CrossFade(randomHash, 0);
            nodePair.animator2.CrossFade(randomHash, 0);
            nodePair.transform1.SetPositionAndRotation(node1Pos3d, direction.ToRotation(225));
            //must set transform2 position and rotation *after* setting transform1 because former is parent
            //and will affect the position and rotation of transform2 is set afterwards.
            nodePair.transform2.SetPositionAndRotation(node2Pos3d, direction.ToRotation(45));
            break;
            //}
        }
    }

    private bool OverlapsExistingNodes(int i, int j)
    {
        Vector2 posToCheck = TileArrayToWorldPosition(i, j);
        for (int k = 0; k < lightningNodeGroupPool.Length; k++)
        {
            TyphoonTilesLightningNodePair node = lightningNodeGroupPool[k];
            if (!node.gameObject1.activeInHierarchy)
                continue;
            Vector3 direction = node.transform1.position - node.transform2.position;
            direction.z = 0;
            direction.Normalize();
            if (IsPointInRotatedRectangle(posToCheck, node.transform1.position + direction, node.transform2.position - direction, 2))
            {
                return true;
            }
        }
        return false;
    }
    public bool IsPointInRotatedRectangle(Vector2 point, Vector2 topEdge, Vector2 bottomEdge, float width)
    {
        Vector2 heightDirection = (topEdge - bottomEdge).normalized;
        Vector2 translatedPoint = point - (topEdge + bottomEdge) / 2f;
        float heightProjection = Vector2.Dot(translatedPoint, heightDirection);
        float widthProjection = Vector2.Dot(translatedPoint, new Vector2(-heightDirection.y, heightDirection.x));
        return (Mathf.Abs(heightProjection) <= ((topEdge - bottomEdge).magnitude / 2f)) &&
               (Mathf.Abs(widthProjection) <= (width / 2f));
    }
    int GetNeighbourCount(int i, int j)
    {
        return tileExistsAndNeighbourCount[i, j] >> 1;
    }
    bool TileExists(int i, int j)
    {
        return (tileExistsAndNeighbourCount[i, j] & 1) == 1;
    }
    Vector2 SnapToGrid(Vector2 position)
    {
        position.x = Mathf.Floor(position.x) + .5f;
        position.y = Mathf.Floor(position.y) + .5f;
        return position;
    }
    void WorldToTileArrayPosition(Vector2 position, out int i, out int j, out bool iOutOfBounds, out bool jOutOfBounds)
    {
        position.x -= tilemapBounds.xMin;
        position.y -= tilemapBounds.yMin;
        int maxI = tileExistsAndNeighbourCount.GetLength(0);
        int maxJ = tileExistsAndNeighbourCount.GetLength(1);
        i = (int)position.x;
        j = (int)position.y;
        iOutOfBounds = i < 0 || i >= maxI;
        jOutOfBounds = j < 0 || j >= maxJ;
    }
    Vector2 TileArrayToWorldPosition(int i, int j)
    {
        i += tilemapBounds.xMin;
        j += tilemapBounds.yMin;
        return new Vector2(i + .5f, j + .5f);//add .5 to center inside the block
    }
    void GetCameraBounds(out float minX, out float maxX, out float maxY, out float minY, float padding = 0, float targetZPos = 0)
    {
        Vector3 cameraPos = mainCam.transform.position;
        float viewHeight = 2f * Mathf.Abs(targetZPos - cameraPos.z) * Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad * .5f);
        float viewWidth = viewHeight * mainCam.aspect;
        float cameraX = cameraPos.x;
        float cameraY = cameraPos.y;
        minX = cameraX - viewWidth * .5f - padding;
        maxX = cameraX + viewWidth * .5f + padding;
        minY = cameraY - viewHeight * .5f - padding;
        maxY = cameraY + viewHeight * .5f + padding;
    }

    bool GetRandomTilePositionInsideCameraBounds(out Vector3 randomPosition)
    {
        GetCameraBounds(out float minX, out float maxX, out float maxY, out float minY, 10);
        int minXInt = (int)minX;
        int maxXInt = (int)maxX + 1;
        int maxYInt = (int)maxY + 1;
        int minYInt = (int)minY;
        List<(ushort x, ushort y)> result = new((maxYInt - minYInt) * (maxXInt - minXInt));
        for (int i = minXInt; i < maxXInt; i++)
        {
            for (int j = minYInt; j < maxYInt; j++)
            {
                Vector3Int tilePos = new(i, j, 0);
                if (TyphoonStageSingleton.instance.solidTiles.HasTile(tilePos))
                {
                    result.Add(((ushort)i, (ushort)j));
                }
            }
        }
        if (result.Count <= 0)
        {
            randomPosition = Vector3.zero;
            return false;
        }
        (ushort x, ushort y) = result[Random.Range(0, result.Count)];
        randomPosition = new(x + .5f, y + .5f, 0);
        return true;
    }

#if UNITY_EDITOR

    private void DisplayLightningNodeBoxes()
    {
        Gizmos.color = Color.red;
        for (int k = 0; k < lightningNodeGroupPool.Length; k++)
        {
            TyphoonTilesLightningNodePair node = lightningNodeGroupPool[k];
            if (!node.gameObject1.activeInHierarchy)
                continue;
            Vector3 direction = node.transform1.position - node.transform2.position;
            direction.z = 0;
            direction.Normalize();
            ConvertEdgeRepresentationToRect(node.transform1.position + direction, node.transform2.position - direction, 2, out Vector2 rectCenter, out Vector2 rectSize, out float angle);
            Gizmos2.DrawRotatedRectangle(rectCenter, rectSize, angle);
        }
    }

    static void ConvertRectToEdgeRepresentation(Vector2 rectCenter, Vector2 rectSize, float rotationAngle, out Vector2 topEdge, out Vector2 bottomEdge, out float width)
    {
        float halfHeight = rectSize.y / 2f;
        float radians = rotationAngle * Mathf.Deg2Rad;
        radians += Mathf.PI / 2;
        Vector2 heightDirection = new(Mathf.Sin(radians), Mathf.Cos(radians));
        topEdge = rectCenter + heightDirection * halfHeight;
        bottomEdge = rectCenter - heightDirection * halfHeight;
        width = rectSize.x;
    }
    static void ConvertEdgeRepresentationToRect(Vector2 topEdge, Vector2 bottomEdge, float width, out Vector2 rectCenter, out Vector2 rectSize, out float rotationAngle)
    {
        rectCenter = (topEdge + bottomEdge) / 2f;
        float height = Vector2.Distance(topEdge, bottomEdge);
        Vector2 heightDirection = (topEdge - bottomEdge).normalized;
        rotationAngle = Mathf.Atan2(heightDirection.y, heightDirection.x) * Mathf.Rad2Deg + 90;
        rectSize = new Vector2(width, height);
    }
    private void DisplayNeighbourAndTileExistInfo()
    {
        for (int i = 0; i < tileExistsAndNeighbourCount.GetLength(0); i++)
        {
            for (int j = 0; j < tileExistsAndNeighbourCount.GetLength(1); j++)
            {
                Vector2 pos = TileArrayToWorldPosition(i, j);
                Handles.color = TileExists(i, j) ? Color.blue : Color.red;
                int neighbourCount = GetNeighbourCount(i, j);
                pos.y -= .2f;
                Handles.Label(pos, neighbourCount.ToString());
                pos.y += .4f;
                Handles.Label(pos, TileExists(i, j).ToString());
            }
        }
    }
#endif
}
