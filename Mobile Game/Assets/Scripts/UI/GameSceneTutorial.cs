using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Tutorial
{
    [SerializeField] private string title;
    [SerializeField] private string text;

    public IEnumerator PlayTutorial(TextMeshProUGUI _title, TextMeshProUGUI _textField, float speed)
    {
        _title.text = title;
        _textField.text = "";
        yield return null;
        ScrollRect scroll = _textField.gameObject.GetComponentInParent<ScrollRect>();

        foreach (char c in text)
        {
            if(c == '_')
            {
                _textField.text += "\n";
            }
            else
            {
                _textField.text += c;
                yield return new WaitForEndOfFrame();
                if(scroll != null)
                {
                    scroll.verticalNormalizedPosition = 0;
                }
                yield return new WaitForSeconds(speed);
            }
        }
    }

    public void Skip(TextMeshProUGUI _title, TextMeshProUGUI _textField)
    {
        _title.text = title;
        string temp = text;
        temp = temp.Replace("_", "\n");
        _textField.text = temp;
        _textField.gameObject.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0;
    }
}

public class GameSceneTutorial : MonoBehaviour
{
    [Header("Main UI Elements")]
    [SerializeField] private GameObject mainTutorialBox;
    [SerializeField] private TextMeshProUGUI mainTutorialTitle;
    [SerializeField] private TextMeshProUGUI mainTutorialText;

    [SerializeField] private Tutorial[] tutorials;
    private Tutorial currentTutorial;

    private GameObject[] tree;
    private GameObject[] traverseSigns;

    [SerializeField] private MonsterSO tutorialMonster;

    private void SetUpTutorial()
    {
        MyEventManager.Tutorial.OnTutorialSetUp -= SetUpTutorial;

        tree = GameObject.FindGameObjectsWithTag("Tree");
        foreach (GameObject g in tree)
        {
            g.SetActive(false);
        }
        traverseSigns = GameObject.FindGameObjectsWithTag("Traverse Sign");
        foreach(GameObject g in traverseSigns)
        {
            g.SetActive(false);
        }

        MyEventManager.Tutorial.OnCloseTutorialBox += CloseUI;
        MyEventManager.Tutorial.OnBeginTutorial += BeginTutorial;
        MyEventManager.Tutorial.OnReachedDestination += ReachedDestination;
        MyEventManager.Tutorial.OnTreeViewed += TreeViewed;
        MyEventManager.Tutorial.OnNewMapSetUp += NewMapSetUp;
        MyEventManager.Tutorial.OnEnemyFaught += EnemyFaught;
    }

    private void CloseUI()
    {
        mainTutorialBox.SetActive(false);
    }

    private void BeginTutorial()
    {
        MyEventManager.Tutorial.OnBeginTutorial -= BeginTutorial;
        StopAllCoroutines();
        mainTutorialBox.SetActive(true);
        currentTutorial = tutorials[0];
        StartCoroutine(currentTutorial.PlayTutorial(mainTutorialTitle, mainTutorialText, 0.075f));
    }

    private void ReachedDestination()
    {
        MyEventManager.Tutorial.OnReachedDestination -= ReachedDestination;
        StopAllCoroutines();
        mainTutorialBox.SetActive(true);
        currentTutorial = tutorials[1];
        StartCoroutine(currentTutorial.PlayTutorial(mainTutorialTitle, mainTutorialText, 0.075f));
        foreach (GameObject g in tree)
        {
            g.SetActive(true);
        }
    }

    private void TreeViewed()
    {
        StopAllCoroutines();
        MyEventManager.Tutorial.OnTreeViewed -= TreeViewed;
        StopAllCoroutines();
        mainTutorialBox.SetActive(true);
        currentTutorial = tutorials[2];
        StartCoroutine(currentTutorial.PlayTutorial(mainTutorialTitle, mainTutorialText, 0.075f)); 
        foreach (GameObject g in traverseSigns)
        {
            g.SetActive(true);
        }
    }

    private void NewMapSetUp()
    {
        MyEventManager.Tutorial.OnNewMapSetUp -= NewMapSetUp;
        StopAllCoroutines();

        //Removes ability to change map until ready
        traverseSigns = GameObject.FindGameObjectsWithTag("Traverse Sign");
        foreach (GameObject g in traverseSigns)
        {
            g.SetActive(false);
        }

        //Creates the tutorial monster
        MyEventManager.Monsters.Manager.OnCreateTutorialMonster?.Invoke(tutorialMonster);

        //At the end
        MapTraversed();
    }

    private void MapTraversed()
    {
        StopAllCoroutines();
        mainTutorialBox.SetActive(true);
        currentTutorial = tutorials[3];
        StartCoroutine(currentTutorial.PlayTutorial(mainTutorialTitle, mainTutorialText, 0.075f));
    }

    private void EnemyFaught()
    {
        MyEventManager.Tutorial.OnEnemyFaught -= EnemyFaught;
        StopAllCoroutines();
        mainTutorialBox.SetActive(true);
        currentTutorial = tutorials[4];
        StartCoroutine(currentTutorial.PlayTutorial(mainTutorialTitle, mainTutorialText, 0.075f));
        foreach (GameObject g in traverseSigns)
        {
            g.SetActive(true);
        }
    }

    public void Skipped()
    {
        StopAllCoroutines();
        currentTutorial.Skip(mainTutorialTitle, mainTutorialText);
    }

    private void OnEnable()
    {
        MyEventManager.Tutorial.OnTutorialSetUp += SetUpTutorial;
    }

    private void OnDestroy()
    {
        MyEventManager.Tutorial.OnTutorialSetUp -= SetUpTutorial;
        MyEventManager.Tutorial.OnReachedDestination -= ReachedDestination;
        MyEventManager.Tutorial.OnTreeViewed -= TreeViewed;
        MyEventManager.Tutorial.OnNewMapSetUp -= NewMapSetUp;
        MyEventManager.Tutorial.OnEnemyFaught -= EnemyFaught;
        MyEventManager.Tutorial.OnCloseTutorialBox -= CloseUI;
    }

}
