using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionCounter : MonoBehaviour
{
    public static ReactionCounter Instance = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    [Header("UI")]
    public TMP_Text likeUI;

    private int likeCount;

    public int Like
    {
        get
        {
            return likeCount;
        }
        set
        {
            likeCount = value;
            likeUI.text = "Like : " + likeCount;
            //PlayerPrefs.SetInt("Like", likeCount);
        }
    }

    void Start()
    {
        likeCount = PlayerPrefs.GetInt("Like", 0);
        likeUI.text = "Like : " + likeCount;
    }

    public void OnClickLike()
    {
        Like++;
    }
}
