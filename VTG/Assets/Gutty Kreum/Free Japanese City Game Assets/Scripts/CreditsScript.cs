using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsScript : MonoBehaviour
{
    [SerializeField] private int initialCount = 0;
    [SerializeField] private List<string> positionsList, namesList;
    [SerializeField] private TextMeshProUGUI positionText, nameText;
    [SerializeField] private GameObject vlad, nika;
    [SerializeField] private Animator square;
    private bool canExit; 
    public void NextText()
    { 
        initialCount++;
        if (initialCount < positionsList.Count)
        {
            positionText.text = positionsList[initialCount];
            nameText.text = namesList[initialCount];
        }
        else
        {
            positionText.text = "ÏÐÎÁÅË ÷òîáû âûéòè";
            nameText.text = "";
            canExit = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        positionText.text = positionsList[initialCount];
        nameText.text = namesList[initialCount];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canExit)
        { 
            Application.Quit();
        }
    }
}
