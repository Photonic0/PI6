using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TMP_Text tmp;
    [SerializeField] float scaleMult;
    public static void SpawnText(Vector3 position, string text, float scaleMult = 1f)
    {
        if(text == null || text == string.Empty)
        {
            return;
        }
        GameObject obj = Instantiate(CommonPrefabs.FloatingText, position, Quaternion.identity);
        FloatingText floatingText = obj.GetComponent<FloatingText>();
        floatingText.tmp.text = text;
        floatingText.scaleMult = scaleMult; 
    }

    void Update()
    {
        //STOREPOSITION AND CONVERT IT INTO PROPER RECT TRANSFORM SPACE
        Resolution res = Screen.currentResolution;
        float scale = 100f / res.height;
        scale *= scaleMult;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
