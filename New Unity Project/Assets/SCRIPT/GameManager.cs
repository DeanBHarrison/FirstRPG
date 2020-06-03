using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    public CharStats[] playerstats;

    public bool gameMenuOpen, dialogActive, fadingBetweenareas, shopActive, battleActive;

    public string[] itemsHeld;
    public int[] numberOfItems;
    public Item[] referenceItems;

    public int currentGold;

    public bool isthereagamemanager;
 

    // Start is called before the first frame update
    void Start()
    {
        SortItems();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);

                
            }
        }

        DontDestroyOnLoad(gameObject);
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if(gameMenuOpen || dialogActive || fadingBetweenareas || shopActive || battleActive)
        {
            PlayerController.instance.canMove = false;
        }
        else
        {
            PlayerController.instance.canMove = true;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            AddItem("Leather Armor");
            AddItem("Blabla");

            RemoveItem("Health Potion");
            RemoveItem("Bleep");

        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveData();

            print("Game saved");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadData();

            print("Game Loaded");
        }
    }

    public Item GetItemDetails(string itemToGrab)
    {
        for(int i = 0; i < referenceItems.Length; i++)
        {
            if(referenceItems[i].itemName == itemToGrab)
            {
                return referenceItems[i];
            }
        }


        return null;

    }

    public void SortItems()
    {
        bool itemAfterSpace = true;

        while (itemAfterSpace)
        {
            itemAfterSpace = false;
            for (int i = 0; i < itemsHeld.Length - 1; i++)
            {
                if (itemsHeld[i] == "")
                {
                    itemsHeld[i] = itemsHeld[i + 1];
                    itemsHeld[i + 1] = "";

                    numberOfItems[i] = numberOfItems[i + 1];
                    numberOfItems[i + 1] = 0;

                    if(itemsHeld[i] != "")
                    {
                        itemAfterSpace = true; 
                    }
                }

            }
        }
    }

    public void AddItem(string itemToAdd)
    {
        int newItemPosition = 0;
        bool foundSpace = false;

        for(int i = 0; i < itemsHeld.Length; i++)
        {
            if(itemsHeld[i] == "" || itemsHeld[i] == itemToAdd)
            {
                newItemPosition = i;
                i = itemsHeld.Length;
                foundSpace = true;
            }
        }

        if (foundSpace)
        {
            bool itemExists = false;
            for(int i = 0; i < referenceItems.Length; i++)
            {
                if(referenceItems[i].itemName == itemToAdd)
                {
                    itemExists = true;

                    i = referenceItems.Length;
                }
            }
            if (itemExists)
            {
                itemsHeld[newItemPosition] = itemToAdd;
                numberOfItems[newItemPosition]++;
            }
            else
            {
                Debug.LogError(itemToAdd + " Does not Exist!!");
            }
        }

        GameMenu.instance.ShowItems();
    }

    public void RemoveItem(string itemToRemove)
    {
        bool foundItem = false;
        int itemPosition = 0;

        for(int i = 0; i < itemsHeld.Length; i++)
        {
            if(itemsHeld[i] == itemToRemove)
            {
                foundItem = true;
                itemPosition = i;

                i = itemsHeld.Length;


            }
        }
        if (foundItem)
        {
            numberOfItems[itemPosition]--;

            if(numberOfItems[itemPosition] <= 0)
            {
                itemsHeld[itemPosition] = "";
            }

            GameMenu.instance.ShowItems();
        }
        else
        {
            Debug.LogError("Couldn't find " + itemToRemove);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetString("Current_Scene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetFloat("player_position_x", PlayerController.instance.transform.position.x);
        PlayerPrefs.SetFloat("player_position_y", PlayerController.instance.transform.position.y);
        PlayerPrefs.SetFloat("player_position_z", PlayerController.instance.transform.position.z);

        //save character info
        for(int i = 0; i < playerstats.Length; i++)
        {
            if(playerstats[i].gameObject.activeInHierarchy)
            {
                PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_active", 1);
            }
            else
            {
                PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_active", 0);
            }

            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_Level", playerstats[i].playerLevel);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_CurrentEXP", playerstats[i].currentEXP);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_CurrentHP", playerstats[i].currentHP);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_MaxHP", playerstats[i].maxHP);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_CurrentMP", playerstats[i].currentMP);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_MaxMP", playerstats[i].maxMP);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_Strength", playerstats[i].strength);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_Defence", playerstats[i].defence);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_WpnPwr", playerstats[i].wpnPwr);
            PlayerPrefs.SetInt("Player_" + playerstats[i].charName + "_ArmrPwr", playerstats[i].armrPwr);
            PlayerPrefs.SetString("Player_" + playerstats[i].charName + "_EquippedWpn", playerstats[i].equippedWpn);
            PlayerPrefs.SetString("Player_" + playerstats[i].charName + "_EquippedArmr", playerstats[i].equippedArmr);
        }

        //store inventory data
        for(int i = 0; i < itemsHeld.Length; i++)
        {
            PlayerPrefs.SetString("ItemInInventory_" + i, itemsHeld[i]);
            PlayerPrefs.SetInt("ItemAmount_" + i, numberOfItems[i]);
        }
    }

    public void LoadData()
    {

        PlayerController.instance.areaTransitionName = "";
        SceneManager.LoadScene(PlayerPrefs.GetString("Current_Scene"));

        PlayerController.instance.transform.position = new Vector3(PlayerPrefs.GetFloat("player_position_x"), PlayerPrefs.GetFloat("player_position_y"), PlayerPrefs.GetFloat("player_position_z"));

        for(int i=0; i < playerstats.Length; i++)
        {
            if(PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_active") == 0)
            {
                playerstats[i].gameObject.SetActive(false);
            }
            else
            {
                playerstats[i].gameObject.SetActive(true);
            }

            playerstats[i].playerLevel =  PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_Level");
            playerstats[i].currentEXP = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_CurrentEXP");
            playerstats[i].currentHP = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_CurrentHP");
            playerstats[i].maxHP = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_MaxHP");
            playerstats[i].currentMP = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_CurrentMP");
            playerstats[i].maxMP = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_MaxMP");
            playerstats[i].strength = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_Strength");
            playerstats[i].defence = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_Defence");
            playerstats[i].wpnPwr = PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_WpnPwr");
            playerstats[i].armrPwr =  PlayerPrefs.GetInt("Player_" + playerstats[i].charName + "_ArmrPwr");
            playerstats[i].equippedWpn = PlayerPrefs.GetString("Player_" + playerstats[i].charName + "_EquippedWpn");
            playerstats[i].equippedArmr = PlayerPrefs.GetString("Player_" + playerstats[i].charName + "_EquippedArmr");
        }

        for(int i = 0; i < itemsHeld.Length; i++)
        {
            itemsHeld[i] = PlayerPrefs.GetString("ItemInInventory_" + i);
            numberOfItems[i] = PlayerPrefs.GetInt("ItemAmount_" + i);
        }
    }
}
