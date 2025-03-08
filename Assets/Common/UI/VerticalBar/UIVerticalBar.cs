using Assets.Common.Interfaces;
using Assets.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class UIVerticalBar : MonoBehaviour, IUpdatableWhenPaused
{
    [SerializeField] Image[] segments;
    int[] segmentSpriteIndices;
    [SerializeField] Sprite[] segmentSprites;
    [SerializeField] Sprite[] barSprites;
    [SerializeField] Image image;
    int currentSpriteIndex;
    float timer;
    new GameObject gameObject;
    public GameObject GameObject => gameObject;
    public bool IsNull => gameObject == null || this == null;
    void Awake()
    {
        gameObject = base.gameObject;
        InitializeSegments();
    }
    public void PausedUpdate(float unscaledDeltaTime)
    {
        UpdateWithDT(unscaledDeltaTime);
    }
    void UpdateWithDT(float dt)
    {
        timer += dt;
        if (timer > 0.1f)
        {
            timer -= 0.1f;
            currentSpriteIndex = (++currentSpriteIndex) % barSprites.Length;
            image.sprite = barSprites[currentSpriteIndex];
            RandomizeFrames();
        }
    }
    public void InitializeSegments()
    {
        float numSegments = segments.Length;
        const float SegmentSpriteHeight = 9f;
        const float SegmentPaddingHeight = 5f;

        float barSpriteHeight = numSegments * SegmentSpriteHeight + numSegments * SegmentPaddingHeight;

        segmentSpriteIndices = new int[segments.Length];
        for (int i = 0; i < segmentSpriteIndices.Length; i++)
        {
            int spriteIndex = Random.Range(0, segmentSprites.Length);
            segmentSpriteIndices[i] = spriteIndex;
            segments[i].sprite = segmentSprites[spriteIndex];
            segments[i].transform.position = transform.position + new Vector3(0, Helper.Remap(i, 0, segmentSpriteIndices.Length - 1, -barSpriteHeight / 2f + SegmentSpriteHeight / 2f, barSpriteHeight / 2f - SegmentSpriteHeight / 2f));
        }
    }
    void RandomizeFrames()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            int[] possibleRandomIndices = new int[segmentSprites.Length];
            for (int j = 0; j < possibleRandomIndices.Length; j++)
            {
                possibleRandomIndices[j] = j;
            }
            int spriteIndex = segmentSpriteIndices[i];
            possibleRandomIndices[spriteIndex] = segmentSprites.Length - 1;
            // subtract 1 because we don't want to index the last slot
            int newSpriteIndex = possibleRandomIndices[Random.Range(0, possibleRandomIndices.Length - 1)];
            Image img = segments[i];
            img.sprite = segmentSprites[newSpriteIndex];
            segmentSpriteIndices[i] = newSpriteIndex;
        }
    }
    private void Update()
    {
        UpdateWithDT(Time.deltaTime);
    }
    public void SetBackColor(Color color)
    {
        image.color = color;
    }
    public void SetSegmentsColor(Color color)
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].color = color;
        }
    }
    private void OnDestroy()
    {
        GameManager.RemoveFromPausedUpdateObjs(this);
    }
    public void UpdateFill(float progress)
    {
        int threshold = (int)Mathf.Lerp(0, segments.Length - 1, progress);
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].gameObject.SetActive(i <= threshold);
        }
    }
}
