using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UmbraEssenceUI : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> essenceAmount;

    private void UpdateEssenceUI(long amount)
    {
        if(essenceAmount.Count > 0)
        {
            foreach (TextMeshProUGUI t in essenceAmount)
            {
                t.text = $"Umbra Essence: {amount}";
            }
        }
    }

    private void OnEnable()
    {
        MyEventManager.Inventory.OnUpdateEssenceUI += UpdateEssenceUI;
    }

    private void OnDisable()
    {
        MyEventManager.Inventory.OnUpdateEssenceUI -= UpdateEssenceUI;
    }
}
