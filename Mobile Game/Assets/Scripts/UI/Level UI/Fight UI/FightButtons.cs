using UnityEngine;
using UnityEngine.UI;

public class FightButtons : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = gameObject.GetComponent<Button>();
    }

    private void ToggleButton(bool newState)
    {
        button.interactable = newState;
    }

    private void OnEnable()
    {
        MyEventManager.UI.FightUI.OnToggleButtons += ToggleButton;
    }

    private void OnDisable()
    {
        MyEventManager.UI.FightUI.OnToggleButtons -= ToggleButton;
    }
}
