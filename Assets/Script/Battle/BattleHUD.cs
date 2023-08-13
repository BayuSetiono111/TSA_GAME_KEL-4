using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Mon _mon;
    Dictionary<ConditionID, Color> statusColor;

    public void SetData(Mon mon)
    {
        _mon = mon;

        nameText.text = mon.BaseStats.Name;
        levelText.text = "Lvl " + mon.Level;
        hpBar.SetHP((float)mon.HP / mon.MaxHP);

        statusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor},
        };

        SetStatusText();
        _mon.OnStatusChange += SetStatusText;
    }

    void SetStatusText()
    {
        if (_mon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _mon.Status.Id.ToString().ToUpper();
            statusText.color = statusColor[_mon.Status.Id];
        }

    }

    public IEnumerator UpdateHP()
    {
        if (_mon.HPChanged)
        {
            yield return hpBar.SetHPSmooth((float)_mon.HP / _mon.MaxHP);
            _mon.HPChanged = false;
        }
    }
}
