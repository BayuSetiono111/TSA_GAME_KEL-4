using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlights;

    Mon _mon;

    public void SetData(Mon mon)
    {
        _mon = mon;

        nameText.text = mon.BaseStats.Name;
        levelText.text = "Lvl " + mon.Level;
        hpBar.SetHP((float)mon.HP / mon.MaxHP);
    }

    public void SetSelected(bool Selected)
    {
        if (Selected)
            nameText.color = highlights;
        else
            nameText.color = Color.black;
    }
}
