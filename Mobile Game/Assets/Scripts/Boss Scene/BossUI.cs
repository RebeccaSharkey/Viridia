using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BossUI : MonoBehaviour
{
    [Header("Boss UI")]
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI content;

    private void SetHeader(string newHeader)
    {
        header.text = newHeader;
        if (newHeader == "Player")
        {
            header.text = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>().playerName;
        }
        content.text = "";
    }

    private void SetContent(char c)
    {
        content.text += c;
    }

    public void ContinueBossUI()
    {
        MyEventManager.BossManager.OnContinueStory?.Invoke();
    }

    public void SkipBossUI()
    {
        MyEventManager.BossManager.OnSkipStory?.Invoke();
    }

    private void OnEnable()
    {
        MyEventManager.UI.BossUI.OnSetHeader += SetHeader;
        MyEventManager.UI.BossUI.OnSetContent += SetContent;
    }

    private void OnDisable()
    {
        MyEventManager.UI.BossUI.OnSetHeader -= SetHeader;
        MyEventManager.UI.BossUI.OnSetContent -= SetContent;
    }
}
