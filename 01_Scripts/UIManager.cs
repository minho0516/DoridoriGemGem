using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Text scoreText;

    public GameObject roundOverPanel;
    public GameObject roundStatPanel;

    public TMP_Text finalScore;
    public TMP_Text roundResultText;

    public GameObject star1, star2, star3;

    private void Start()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
    }
}
