﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int letterPerSecond;
    [SerializeField] Color highlights;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> actionText;
    [SerializeField] List<Text> moveText;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";

        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/letterPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void enableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    public void enableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    public void enableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for(int i = 0; i<actionText.Count; i++)
        {
            if (i == selectedAction)
                actionText[i].color = highlights;
            else
                actionText[i].color = Color.black;
        }
    }

    public void SetMoveName(List<Move> moves)
    {
        for (int i = 0; i < moveText.Count; ++i)
        {
            if (i < moves.Count)
                moveText[i].text = moves[i].Base.Name;
            else
                moveText[i].text = "-";
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for(int i = 0; i<moveText.Count; i++)
        {
            if (i == selectedMove)
                moveText[i].color = highlights;

            else
                moveText[i].color = Color.black;
        }
        ppText.text = $"PP : {move.PP}/{move.Base.PP}";
        typeText.text = $"Type : {move.Base.Type.ToString()}";
    }
}

