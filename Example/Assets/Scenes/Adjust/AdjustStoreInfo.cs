using UnityEngine;

public class AdjustStoreInfo
{
    public string StoreName { get; private set; }
    public string StoreAppId { get; set; }

    public AdjustStoreInfo(string storeName)
    {
        StoreName = storeName;
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(StoreName))
        {
            Debug.LogError("[Adjust]: Store name can't be null or empty");
            return false;
        }

        return true;
    }
}
