﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamParty : MonoBehaviour
{
    [SerializeField] List<Mon> mons;

    public List<Mon> Mons
    {
        get
        {
            return mons;
        }
    }
    private void Start()
    {
        foreach(var mon in mons)
        {
            mon.Init();
        }
    }

    public Mon GetHealtyMon()
    {
        return mons.Where(x => x.HP > 0).FirstOrDefault();
    }
}
