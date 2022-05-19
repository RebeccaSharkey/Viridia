using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    private Button button;
    private Ability ability;
    [SerializeField] private bool isFightButton;
    [SerializeField] private bool isInInventory;
    [SerializeField] private Item thisItem;

    [SerializeField] private AudioClip audioClip;

    private void Awake()
    {
        button = gameObject.GetComponent<Button>();
    }

    public void SetUp(Attack newAttack)
    {
        ability = newAttack;
        buttonText.text = newAttack.abilityName;
    }
    public void SetUp(PowerUp newPowerUp)
    {
        ability = newPowerUp;
        buttonText.text = newPowerUp.abilityName;
    }
    public void SetUp(Attack newAttack, Item newItem)
    {
        ability = newAttack;
        buttonText.text = newAttack.abilityName;
        thisItem = newItem;
    }
    public void SetUp(PowerUp newPowerUp, Item newItem)
    {
        ability = newPowerUp;
        buttonText.text = newPowerUp.abilityName;
        thisItem = newItem;
    }

    public void OnPress()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if(isFightButton)
        {
            MyEventManager.LevelManager.FightPhase.OnPlayerChoseAbility?.Invoke(ability);
        }
        if(isInInventory)
        {
            MyEventManager.UI.MapUI.OnAddNewAbility?.Invoke(ability, thisItem);
        }
        else
        {
            MyEventManager.UI.MapUI.OnOpenSwapAbilityUI?.Invoke(ability);
        }
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
