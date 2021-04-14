using UnityEngine;
using UnityEngine.UI;

public class InfoListing : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 5f);
    }
    public void SetInfo(string info)
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = " " + info + " ";
    }
}
