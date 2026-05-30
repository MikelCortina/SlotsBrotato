using UnityEngine;

public class SymbolInventoryListUI : MonoBehaviour
{
    [Header("References")]
    public Transform content;
    public SymbolInventoryItemUI itemPrefab;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (content == null || itemPrefab == null) return;
        if (RunConfig.Instance == null) return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var symbol in RunConfig.Instance.selectedSymbols)
        {
            if (symbol == null) continue;

            SymbolInventoryItemUI item =
                Instantiate(itemPrefab, content);

            item.symbol = symbol;
            item.Refresh();
        }
    }
}