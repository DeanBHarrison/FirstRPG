using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{

    public static BattleManager instance;

    public bool battleActive;

    public GameObject BattleScene;
    public GameObject battleCanvas;
    public bool playerDamaged;

    public Transform[] playerPositions;
    public Transform[] enemyPositions;

    public Battlechar[] playerPrefabs;
    public Battlechar[] enemyPrefabs;

    public List<Battlechar> activeBattlers = new List<Battlechar>();

    public int currentTurn;
    public bool turnwaiting;

    public GameObject uiButtonsHolder;

    public BattleMove[] movesList;

    public GameObject enemyAttackEffect;

    public DamageNumber theDamageNumber;

    public Text[] playerName, playerHP, playerMP;

    public GameObject targetMenu;
    public BattleTargetButton[] targetButtons;

    public GameObject magicMenu;

    public GameObject itemMenu;
    public bool usingItemDuringBattle;
    public bool ItemUsed;

    public BattleMagicSelect[] magicButtons;

    public BattleNotification battleNotice;

    public int chanceToFlee = 35;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            BattleStart(new string[] { "Eyeball", "Spider" });
        }

        if(battleActive)
        {
            if(turnwaiting)
            {
                if(activeBattlers[currentTurn].isPlayer)
                {
                    uiButtonsHolder.SetActive(true);
                }else
                {
                    uiButtonsHolder.SetActive(false);

                    //enemy should attack
                    StartCoroutine(EnemyMoveCo());
                }
            }

            if(Input.GetKeyDown(KeyCode.N))
            {
                NextTurn();
            }
        }

    }

    public void BattleStart(string[] enemiesToSpawn)
    {
        if(!battleActive)
        {
            battleActive = true;

            GameManager.instance.battleActive = true;

            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
            BattleScene.SetActive(true);

            AudioManager.instance.PlayBGM(0);

            for(int i = 0; i < playerPositions.Length; i++)
            {
                if(GameManager.instance.playerstats[i].gameObject.activeInHierarchy)
                {
                    for(int j = 0; j < playerPrefabs.Length; j++)
                    {
                        if(playerPrefabs[j].charName == GameManager.instance.playerstats[i].charName)
                        {
                            Battlechar newPlayer = Instantiate(playerPrefabs[j], playerPositions[i].position, playerPositions[i].rotation);
                            newPlayer.transform.parent = playerPositions[i];

                            activeBattlers.Add(newPlayer);


                            CharStats thePlayer = GameManager.instance.playerstats[i];
                            activeBattlers[i].currentHP = thePlayer.currentHP;
                            activeBattlers[i].maxHP = thePlayer.maxHP;
                            activeBattlers[i].currentMP = thePlayer.currentMP;
                            activeBattlers[i].maxMP = thePlayer.maxHP;
                            activeBattlers[i].strength = thePlayer.strength;
                            activeBattlers[i].defence = thePlayer.defence;
                            activeBattlers[i].wpnPower = thePlayer.wpnPwr;
                            activeBattlers[i].armrPower = thePlayer.armrPwr;
                        }
                    }


                }
            }

            for(int i = 0; i < enemiesToSpawn.Length; i++)
            {
                if(enemiesToSpawn[i] != "")
                {
                    for(int j = 0; j < enemyPrefabs.Length; j++)
                    {
                        if(enemyPrefabs[j].charName == enemiesToSpawn[i])
                        { 
                        Battlechar newEnemy = Instantiate(enemyPrefabs[j], enemyPositions[i].position, enemyPositions[i].rotation);
                            newEnemy.transform.parent = enemyPositions[i];
                            activeBattlers.Add(newEnemy);
                        }
                    }
                }
            }

            turnwaiting = true;
            currentTurn = Random.Range(0, activeBattlers.Count);

            UpdateUIstats();
        }

    
    }

    public void NextTurn()
    {
        currentTurn++;
        if(currentTurn >= activeBattlers.Count)
        {
            currentTurn = 0;
        }

        turnwaiting = true;

        UpdateBattle();

        UpdateUIstats();
    }

    public void UpdateBattle()
    {
        bool allEnemiesDead = true;
        bool allPlayersDead = true;

        for(int i = 0; i < activeBattlers.Count; i++)
        {
            if(activeBattlers[i].currentHP < 0)
            {
                activeBattlers[i].currentHP = 0;    
            }

            if(activeBattlers[i].currentHP == 0)
            {
                //handle dead battler.
                if(activeBattlers[i].isPlayer)
                {
                    activeBattlers[i].theSprite.sprite = activeBattlers[i].deadSprite;
                }
                else
                {
                    activeBattlers[i].EnemyFade();
                }
            }else
            {
                if(activeBattlers[i].isPlayer)
                {
                    allPlayersDead = false;
                    activeBattlers[i].theSprite.sprite = activeBattlers[i].aliveSprite;
                }else
                {
                    allEnemiesDead = false;
                }
            }

        }

        if(allEnemiesDead || allPlayersDead)
        {
            if(allEnemiesDead)
            {
                //end battle in victory
                StartCoroutine(EndBattleCo());
            }
            else
            {
                //end battle in failure
            }

           /* BattleScene.SetActive(false);
            GameManager.instance.battleActive = false;
            battleActive = false;*/
        }
        else
        {
            while(activeBattlers[currentTurn].currentHP == 0)
            {
                currentTurn++;
                if(currentTurn >= activeBattlers.Count)
                {
                    currentTurn = 0;
                }
            }
        }
    }

    public IEnumerator EnemyMoveCo()
    {
        turnwaiting = false;
        yield return new WaitForSeconds(1f);
        EnemyAttack();
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    public void EnemyAttack()
    {
        List<int> players = new List<int>();
        for(int i = 0; i< activeBattlers.Count; i++)
        {
            if(activeBattlers[i].isPlayer && activeBattlers[i].currentHP > 0)
            {
                players.Add(i);
            }
        }
        int selectedTarget = players[Random.Range(0, players.Count)];

        //activeBattlers[selectedTarget].currentHP -= 30;

        int selectAttack = Random.Range(1, activeBattlers[currentTurn].movesAvailable.Length);
        int movePower = 0;
        for(int i = 0; i < movesList.Length; i++)
        {
            if(movesList[i].moveName == activeBattlers[currentTurn].movesAvailable[selectAttack])
            {
                Instantiate(movesList[i].theEffect, activeBattlers[selectedTarget].transform.position, activeBattlers[selectedTarget].transform.rotation);
                movePower = movesList[i].movePower;
            }
        }
        Instantiate(enemyAttackEffect, activeBattlers[currentTurn].transform.position, activeBattlers[currentTurn].transform.rotation);

        DealDamage(selectedTarget, movePower);
    }

    public void DealDamage(int target, int movePower)
    {
        float atkPwr = activeBattlers[currentTurn].strength + activeBattlers[currentTurn].wpnPower;
        float defPwr = activeBattlers[target].defence + activeBattlers[target].armrPower;

        float damageCalc = (atkPwr / defPwr) * movePower * Random.Range(.9f, 1.1f);
        int damagetoGive = Mathf.RoundToInt(damageCalc);

        Debug.Log(activeBattlers[currentTurn].charName + " is dealing" + damageCalc + "(" + damagetoGive + ") damage to " + activeBattlers[target].charName);

        activeBattlers[target].currentHP -= damagetoGive;

        Instantiate(theDamageNumber, activeBattlers[target].transform.position, activeBattlers[target].transform.rotation).SetDamage(damagetoGive);

        UpdateUIstats();

        if(activeBattlers[target].isPlayer)
        {
            playerDamaged = true;
        }
    }

    public void UpdateUIstats()
    {

        if (ItemUsed)
        {
            for (int i = 0; i < playerPositions.Length; i++)
            {
                if (GameManager.instance.playerstats[i].gameObject.activeInHierarchy)
                {
                    for (int j = 0; j < playerPrefabs.Length; j++)
                    {
                        if (playerPrefabs[j].charName == GameManager.instance.playerstats[i].charName)
                        {
                            CharStats thePlayer = GameManager.instance.playerstats[i];
                            activeBattlers[i].currentHP = thePlayer.currentHP;
                            activeBattlers[i].maxHP = thePlayer.maxHP;
                            activeBattlers[i].currentMP = thePlayer.currentMP;
                            activeBattlers[i].maxMP = thePlayer.maxMP;
                            activeBattlers[i].strength = thePlayer.strength;
                            activeBattlers[i].defence = thePlayer.defence;
                            activeBattlers[i].wpnPower = thePlayer.wpnPwr;
                            activeBattlers[i].armrPower = thePlayer.armrPwr;
                        }
                    }
                }
            }

            // Reset _itemUsed
            ItemUsed = false;
        }

        if (playerDamaged)
        {
            for (int i = 0; i < playerPositions.Length; i++)
            {
                if (GameManager.instance.playerstats[i].gameObject.activeInHierarchy)
                {
                    for (int j = 0; j < playerPrefabs.Length; j++)
                    {
                        if (playerPrefabs[j].charName == GameManager.instance.playerstats[i].charName)
                        {
                            CharStats thePlayer = GameManager.instance.playerstats[i];
                            thePlayer.currentHP = activeBattlers[i].currentHP;
                            thePlayer.maxHP = activeBattlers[i].maxHP;
                            thePlayer.currentMP = activeBattlers[i].currentMP;
                            thePlayer.maxMP = activeBattlers[i].maxMP;
                            thePlayer.strength = activeBattlers[i].strength;
                            thePlayer.defence = activeBattlers[i].defence;
                            thePlayer.wpnPwr = activeBattlers[i].wpnPower;
                            thePlayer.armrPwr = activeBattlers[i].armrPower;
                        }
                    }
                }
            }

            // Reset _playerDamaged
            playerDamaged = false;
        }
            for (int i = 0; i < playerName.Length; i++)
        {
            if(activeBattlers.Count > i)
            {
                if(activeBattlers[i].isPlayer)
                {
                    Battlechar playerData = activeBattlers[i];

                    playerName[i].gameObject.SetActive(true);
                    playerName[i].text = playerData.charName;
                    playerHP[i].text = Mathf.Clamp(playerData.currentHP, 0, int.MaxValue) + "/" + playerData.maxHP;
                    playerMP[i].text = Mathf.Clamp(playerData.currentMP, 0, int.MaxValue) + "/" + playerData.maxMP;
                }else
                {
                    playerName[i].gameObject.SetActive(false);
                }
            }
            else
            {
                playerName[i].gameObject.SetActive(false);
            }           
        }
    }

    public void PlayerAttack(string moveName, int selectedTarget)
    {

        int movePower = 0;
        for (int i = 0; i < movesList.Length; i++)
        {
            if (movesList[i].moveName == moveName)
            {
                Instantiate(movesList[i].theEffect, activeBattlers[selectedTarget].transform.position, activeBattlers[selectedTarget].transform.rotation);
                movePower = movesList[i].movePower;
            }
        }

        Instantiate(enemyAttackEffect, activeBattlers[currentTurn].transform.position, activeBattlers[currentTurn].transform.rotation);

        DealDamage(selectedTarget, movePower);
        uiButtonsHolder.SetActive(false);
        targetMenu.SetActive(false);
        NextTurn();
       
    }

    public void OpenTargetMenu(string moveName)
    {
        targetMenu.SetActive(true);

        List<int> Enemies = new List<int>();

        for(int i = 0; i < activeBattlers.Count; i++)
        {
            if(!activeBattlers[i].isPlayer)
            {
                Enemies.Add(i);
            }
        }

        for(int i = 0; i < targetButtons.Length; i++)
        {
            if(Enemies.Count > i && activeBattlers[Enemies[i]].currentHP > 0)
            {
                targetButtons[i].gameObject.SetActive(true);

                targetButtons[i].moveName = moveName;
                targetButtons[i].activeBattlerTarget = Enemies[i];
                targetButtons[i].targetName.text = activeBattlers[Enemies[i]].charName;
            }else
            {
                targetButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenItemMenu()
    {
        usingItemDuringBattle = true;
        battleCanvas.SetActive(false);
    }

    public void OpenMagicMenu()
    {
        magicMenu.SetActive(true);

        for(int i = 0; i < magicButtons.Length; i++)
        {
            if(activeBattlers[currentTurn].movesAvailable.Length > i)
            {
                magicButtons[i].gameObject.SetActive(true);

                magicButtons[i].spellName = activeBattlers[currentTurn].movesAvailable[i];
                magicButtons[i].nameText.text = magicButtons[i].spellName;

                for(int j = 0; j < movesList.Length; j++)
                {
                    if(movesList[j].moveName == magicButtons[i].spellName)
                    {
                        magicButtons[i].spellCost = movesList[j].moveCost;
                        magicButtons[i].costText.text = magicButtons[i].spellCost.ToString();
                    }
                }
            }else
            {
                magicButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Flee()
    {
        int fleeSucess = Random.Range(0, 100);
        if(fleeSucess < chanceToFlee)
        {
            //end the battle
            //battleActive = false;
            //BattleScene.SetActive(false);
            StartCoroutine(EndBattleCo());
        }
        else
        {
            NextTurn();
            battleNotice.theText.text = "Couldn't escape!";
            battleNotice.Activate();
        }
    }

    public IEnumerator EndBattleCo()
    {
        battleActive = false;
        uiButtonsHolder.SetActive(false);
        targetMenu.SetActive(false);
        magicMenu.SetActive(false);
        itemMenu.SetActive(false);

        yield return new WaitForSeconds(.5f);

        UIFade.instance.FadeToBlack();

        yield return new WaitForSeconds(1.5f);

        for(int i = 0; i < activeBattlers.Count; i++)
        {
            if(activeBattlers[i].isPlayer)
            {
                for(int j = 0; j < GameManager.instance.playerstats.Length; j++)
                {
                    if(activeBattlers[i].charName == GameManager.instance.playerstats[j].charName)
                    {
                        GameManager.instance.playerstats[j].currentHP = activeBattlers[i].currentHP;
                        GameManager.instance.playerstats[j].currentHP = activeBattlers[i].currentMP;
                    }
                }
            }

            Destroy(activeBattlers[i].gameObject);
        }

        UIFade.instance.FadeFromBlack();
        BattleScene.SetActive(false);
        activeBattlers.Clear();
        currentTurn = 0;
        GameManager.instance.battleActive = false;
                                
    }
}
