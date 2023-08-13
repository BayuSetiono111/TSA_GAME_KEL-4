using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static MoveBase;

[System.Serializable]
public class Mon
{
    [SerializeField] MonBase _base;
    [SerializeField] int level;

    public MonBase BaseStats
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }

    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status {get; private set;}
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HPChanged { get; set; }

    public event System.Action OnStatusChange;

    public void Init()
    {
        //generate move
        Moves = new List<Move>();
        foreach (var move in BaseStats.LearableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }
        CalculateStat();

        HP = MaxHP;

        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    void CalculateStat()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((BaseStats.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((BaseStats.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((BaseStats.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((BaseStats.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((BaseStats.Speed * Level) / 100f) + 5);

        MaxHP =  Mathf.FloorToInt((BaseStats.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0}
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //Stats boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            
            if (boost > 0)
            {
                StatusChanges.Enqueue($"{BaseStats.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{BaseStats.Name}'s {stat} fell!");
            }
        }
    }

    public int MaxHP
    { get; private set; }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public DamageDetail TakeDamage(Move move, Mon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f < 6.25f)
            critical = 2f;
        
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.BaseStats.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.BaseStats.Type2);

        var damageDetails = new DamageDetail()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.8f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HPChanged = true;
    }
    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;

        Status =  ConditionDB.conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{BaseStats.Name}{Status.StartMessages}");

        OnStatusChange?.Invoke();
    }
    public void CureStatus()
    {
        Status = null;
        OnStatusChange?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionDB.conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{BaseStats.Name}{VolatileStatus.StartMessages}");
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if(Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }
    public void OnAfterMove()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetail
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
