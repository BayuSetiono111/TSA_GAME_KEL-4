using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB 
{
    public static void Init()
    {
        foreach(var kvp in conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessages = " has been Poisoned",
                OnAfterTurn = (Mon Mon) =>
                {
                    Mon.UpdateHP(Mon.MaxHP/8);
                    Mon.StatusChanges.Enqueue($"{Mon.BaseStats.Name} suffering from poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessages = " has been burn",
                OnAfterTurn = (Mon Mon) =>
                {
                    Mon.UpdateHP(Mon.MaxHP/8);
                    Mon.StatusChanges.Enqueue($"{Mon.BaseStats.Name} suffering from burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessages = " has been paralyzed",
                OnBeforeMove = (Mon mon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} kejang cok!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessages = " has been Frozen",
                OnBeforeMove = (Mon mon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        mon.CureStatus();
                        mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} dah gak beku cok!");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessages = " has been Sleep",
                OnStart = (Mon mon) =>
                {
                    mon.StatusTime = Random.Range(1,4);
                    Debug.Log($"{mon.StatusTime}");
                },
                OnBeforeMove = (Mon mon) =>
                {
                    if(mon.StatusTime == 0)
                    {
                        mon.CureStatus();
                        mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} wake up!");
                        return true;
                    }
                    mon.StatusTime--;
                    mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} is sleeping!");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessages = " has been Confused",
                OnStart = (Mon mon) =>
                {
                    mon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"{mon.VolatileStatusTime}");
                },
                OnBeforeMove = (Mon mon) =>
                {
                    if(mon.VolatileStatusTime == 0)
                    {
                        mon.CureVolatileStatus();
                        mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} snap out of confusion!");
                        return true;
                    }
                    mon.VolatileStatusTime--;

                    if(Random.Range(1,3) == 1)
                    {
                        return true;
                    }
                    
                    mon.StatusChanges.Enqueue($"{mon.BaseStats.Name} is confused");
                    mon.UpdateHP(mon.MaxHP / 8);
                    mon.StatusChanges.Enqueue($"it hurt itself");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}