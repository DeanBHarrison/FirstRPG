using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogmanager : MonoBehaviour{

   public Text dialogText;
   public Text nameText;
   public GameObject dialogbox;
   public GameObject nameBox;

    public string[] dialogLines;

    public int currentLine;
    private bool justStarted;

    public static Dialogmanager instance;

    private string questToMark;
    private bool markQuestComplete;
    private bool shouldMarkQuest;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

       // dialogText.text = dialogLines[currentLine];
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogbox.activeInHierarchy)
        {
            if (Input.GetButtonUp("Fire1"))
            {
                if (!justStarted)
                {

                    currentLine++;

                    if (currentLine >= dialogLines.Length)
                    {
                        dialogbox.SetActive(false);

                        PlayerController.instance.canMove = true;

                        GameManager.instance.dialogActive = false;

                        if(shouldMarkQuest)
                        {
                            shouldMarkQuest = false;
                            if(markQuestComplete)
                            {
                                QuestManager.instance.MarkQuestComplete(questToMark);
                            }else
                            {
                                QuestManager.instance.MarkQuestIncomplete(questToMark);
                            }
                        }
                    }
                    else
                    {
                        CheckIfName();
                        dialogText.text = dialogLines[currentLine];
                    }
                }
                else
                {
                    justStarted = false;
                }


             }
                
          
        }
        
    }
    public void Showdialog(string[] newLines, bool isPerson)
    {
        dialogLines = newLines;

        currentLine = 0;

        CheckIfName();

        dialogText.text = dialogLines[currentLine];
        dialogbox.SetActive(true);

        justStarted = true;

        nameBox.SetActive(isPerson);

        PlayerController.instance.canMove = false;

        GameManager.instance.dialogActive = true;
    }

    public void CheckIfName()
    {
        if (dialogLines[currentLine].StartsWith("n-"))
        {
            nameText.text = dialogLines[currentLine].Replace("n-", "");
            currentLine++;
        }
    }

    public void ShouldActivateQuestAtEnd(string questName, bool markComplete)
    {
        questToMark = questName;
        markQuestComplete = markComplete;

        shouldMarkQuest = true;
    }
}
