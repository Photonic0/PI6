using Assets.Helpers;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpikeWaveSpike : Projectile
{
    public override int Damage => 3;
    [SerializeField] new Transform transform;
    //[SerializeField] new SpriteRenderer renderer;
    //[SerializeField] Texture2D spikeTexture;
    //[SerializeField] int cropAmount;
    
    Vector3 originalPos;
    float timer;
    float animationDuration = 1f;
    float timeToSpawnNextSpike = .4f;
    sbyte direction;
    bool spawnedNext;
    short spikesLeftToSpawn;//count down every spike spawned, to eventually stop
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if(animationDuration == 0)
        {
            animationDuration = 1;
        }
        if(timeToSpawnNextSpike == 0)
        {
            timeToSpawnNextSpike = .4f;
        }
        timer = 0;
        spawnedNext = false;
        originalPos = transform.position;
        //originalPos.y = Mathf.Round(originalPos.y) - .02f;
        //originalPos.x = Mathf.Round(originalPos.x);
        //transform.position = originalPos;
    }

    private void OnEnable()
    {
        Initialize();
    }
    void Update()
    {
        UpAndDown();
        //AdjustSprite();
    }

    private void UpAndDown()
    {
        Vector3 pos = originalPos;
        pos.y += Mathf.Sin(timer * Mathf.PI / animationDuration) * .5f;
        timer += Time.deltaTime;
        pos.y += Mathf.Sin(timer * Mathf.PI / animationDuration) * .5f;
        transform.position = pos;
        if(!spawnedNext && timer > timeToSpawnNextSpike)
        {
            SpawnSpike(originalPos + new Vector3(direction, 0), direction, spikesLeftToSpawn, animationDuration, timeToSpawnNextSpike);
            spawnedNext = true;
        }
        if(timer > animationDuration)
        {
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }
    }

    //~ 0.002s to execute once
    //private void AdjustSprite()
    //{
    //    cropAmount = (int)((spikeTexture.height / 3) * (1 - Mathf.Sin(Mathf.PI * timer / animationDuration)));
    //    int y = ((int)(animationTimer * 10) % 3) * (spikeTexture.height / 3);
    //    int rectHeight = spikeTexture.height / 3 - cropAmount;
    //    rectHeight = Mathf.Clamp(rectHeight, 0, spikeTexture.height / 3);
    //    Rect rect = new(0, y, spikeTexture.width, rectHeight);
    //    Sprite spikeSprite;
    //    //test execution time to see if it's fine to do it every frame
    //    spikeSprite = UnityEngine.Sprite.Create(spikeTexture, rect, new Vector2(0.5f, 0), 50);
    //    renderer.sprite = spikeSprite;
    //    animationTimer += Time.deltaTime;
    //}
    public static void StartSpikeWave(Vector3 from, float blindSpotWidth, short numberOfSpikesPerSide, float animationDuration, float timeToSpawnNextSpike, AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.volume = .5f;
            audioSource.PlayOneShot(SpikeStageSingleton.instance.spikeShockwave);
        }
        blindSpotWidth *= .5f;
        SpawnSpike(from + new Vector3(blindSpotWidth, 0), 1, numberOfSpikesPerSide, animationDuration, timeToSpawnNextSpike);
        SpawnSpike(from - new Vector3(blindSpotWidth, 0), -1, numberOfSpikesPerSide, animationDuration, timeToSpawnNextSpike);

    }

    static void SpawnSpike(Vector3 currentPos, sbyte direction, short spikesLeftToSpawn, float animationDuration, float timeToSpawnNextSpike)
    {
        if(spikesLeftToSpawn <= 0)
        {
            return;
        }
        spikesLeftToSpawn--;
        Quaternion rotation = Quaternion.Euler(0, 0, 180);
        Tilemap tiles = SpikeStageSingleton.instance.solidTiles;
        bool foundsuitableTile = false;
        for (int i = -4; i < 5; i++)
        {
            Vector3 posToCheck = currentPos;
            posToCheck.y += i;
            Vector3Int coordToCheck = tiles.WorldToCell(posToCheck);
            bool hasnoTileAtCurrent = !tiles.HasTile(coordToCheck);
            coordToCheck.y -= 1;
            bool hasTileBelow = tiles.HasTile(coordToCheck);
            foundsuitableTile = hasTileBelow && hasnoTileAtCurrent;
            if(foundsuitableTile)
            {
                currentPos.y = coordToCheck.y + .5f;
                break;
            }    
        }
        if(!foundsuitableTile)
        {
            return;
        }
        GameObject obj = Instantiate(SpikeStageSingleton.instance.spikeWaveSpike, currentPos, rotation);
        SpikeWaveSpike spike = obj.GetComponent<SpikeWaveSpike>();
        spike.direction = direction;
        spike.animationDuration = animationDuration;
        spike.timeToSpawnNextSpike = timeToSpawnNextSpike;
        spike.spikesLeftToSpawn = spikesLeftToSpawn;
        spike.Initialize();
        //spike.AdjustSprite();
    }
}
