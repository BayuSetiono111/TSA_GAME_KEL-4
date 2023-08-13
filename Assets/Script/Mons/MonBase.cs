using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mon", menuName = "Mon/Creat new Mon")]
public class MonBase : ScriptableObject
{
    [SerializeField] string monName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] MonsType type1;
    [SerializeField] MonsType type2;

    //stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int sAttack;
    [SerializeField] int sDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearableMove> learableMoves;

    public string Name
    {
        get { return monName; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public MonsType Type1
    {
        get { return type1; }
    }
    public MonsType Type2
    {
        get { return type2; }
    }
    public int MaxHP
    {
        get { return maxHP; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return sAttack; }
    }
    public int SpDefense
    {
        get { return sDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearableMove> LearableMoves
    {
        get { return learableMoves; }
    }
}

[System.Serializable]
public class LearableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }

}

public enum MonsType
{
    None,
    Normal,
    Grass,
    Fire,
    Water
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}

public class TypeChart
{
    static float[][] chart =
    {                   
        //                       Nor    Gras    Fire    Watr       
        /*Normal*/  new float[] {1f,    1f,     1f,     1f  },
        /*Grass*/   new float[] {1f,    1f,     0.5f,   2f  },
        /*Fire*/    new float[] {1f,    2f,     1f,     0.5f},
        /*Water*/   new float[] {1f,    1f,     2f,     0.5f}
    };

    public static float GetEffectiveness(MonsType attackType, MonsType defenseType)
    {
        if (attackType == MonsType.None || defenseType == MonsType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}