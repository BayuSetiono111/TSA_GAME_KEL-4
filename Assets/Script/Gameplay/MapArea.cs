﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Mon> wildMons;

    public Mon GetRandomWildMon()
    {
        var wildMon =  wildMons[Random.Range(0, wildMons.Count)];
        wildMon.Init();
        return wildMon;
    }
}
