using UnityEngine;

public class InfoFeed : MonoBehaviour
{
    public static InfoFeed instance;

    [SerializeField] GameObject infoListingPrefab;

    void Start()
    {
        instance = this;
    }

    public void DisplayInfo(string info)
    {
        GameObject temp = Instantiate(infoListingPrefab, transform);
        temp.transform.SetSiblingIndex(0);
        InfoListing tempListing = temp.GetComponent<InfoListing>();
        tempListing.SetInfo(info);
    }
}
