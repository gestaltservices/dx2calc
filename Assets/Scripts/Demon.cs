using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Demon", menuName = "Make Demon", order = 1)]
[System.Serializable]
public class Demon
{
    //[Visibility(method = "NameCheck")]
    public string name;
    public enum Race
    {
        Herald, Megami, Deity, Avatar, Holy, Genma, Fury, Lady, Kishin, Divine, Yoma, Snake, Beast, Fairy, Fallen, Brute, Femme, Night, Vile, Wilder, Foul, Tyrant, Haunt, Rumor, UMA, Enigma, Fiend, Hero, Null
    }
    public Race race;
    public int grade;
    public int rarity;
    public int rarityValue
    {
        get
        {
            switch (rarity)
            {
                case 1:
                    return 1;
                case 2:
                    return 3;
                case 3:
                    return 6;
                case 4:
                    return 11;
                case 5:
                    return 22;
            }
            return rarity;
        }
    }

    public Sprite icon;
    public int HP;
    public int STR;
    public int MAG;
    public int VIT;
    public int AGI;
    public int LUK;
}
