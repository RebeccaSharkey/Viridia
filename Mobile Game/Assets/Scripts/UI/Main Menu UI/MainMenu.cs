using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;

    [SerializeField] private GameObject startButton;
    private string currentSpritePath = "";
    private string currentName = "";
    TouchScreenKeyboard keyboard = null;

    [SerializeField] private GameObject mainMenuBox;

    private void SetUp()
    {
        StartCoroutine(ISetUp());
    }

    IEnumerator ISetUp()
    {
        DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath);
        if (directory.GetFiles("*.dat").Length == 3)
        {
            MyEventManager.SaveLoad.OnLoadPlayer?.Invoke();
            yield return new WaitForSeconds(1f);
            if ((bool)MyEventManager.SaveLoad.OnGetTutorialStatus?.Invoke())
            {
                continueButton.SetActive(true);
            }
            else
            {
                continueButton.SetActive(false);
            }
        }
        else
        {
            continueButton.SetActive(false);
        }
        mainMenuBox.SetActive(true);
    }

    public void OnNewGame()
    {
        //Deletes all old saves
        MyEventManager.SaveLoad.OnDeleteHubSaveInfo?.Invoke();
        MyEventManager.SaveLoad.OnDeleteBossSave?.Invoke();
        MyEventManager.SaveLoad.OnNewPlayer?.Invoke();

        //Sets up new save info
        MyEventManager.SaveLoad.OnSetPlayerName?.Invoke(currentName);
        MyEventManager.SaveLoad.OnSetPlayerSprite?.Invoke(currentSpritePath);

        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("GameScene");
    }

    public void OnContinue()
    {
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("HubWorld");
    }

    public void OnCharacterSelected(Sprite sprite)
    {
        currentSpritePath = sprite.name;
        if (currentName != "" && currentSpritePath != "")
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void OnInputSelect(TextMeshProUGUI s)
    {
        startButton.SetActive(false);
        if (keyboard == null)
        {
            keyboard = TouchScreenKeyboard.Open(s.text, TouchScreenKeyboardType.Default);
        }
        else
        {
            keyboard.active = true;
        }
    }

    public void OnInputDeselect(TextMeshProUGUI s)
    {
        if (keyboard != null)
        {
            keyboard.active = false;
        }

        if (s.text != "")
        {
            currentName = s.text;
        }
        if (currentName != "" && currentSpritePath != "")
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void OnComplete(TextMeshProUGUI s)
    {
        if (s.text != "")
        {
            currentName = s.text;
        }
        if (currentName != "" && currentSpritePath != "")
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void OnEnable()
    {
        MyEventManager.UI.OnBeginMainMenu += SetUp;
    }

    private void OnDisable()
    {
        MyEventManager.UI.OnBeginMainMenu -= SetUp;
    }
}
