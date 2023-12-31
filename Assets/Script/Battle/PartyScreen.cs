﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Mon> mons;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Mon> mons)
    {
        this.mons = mons;

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < mons.Count)
            {
                memberSlots[i].SetData(mons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateMemberSelection(int SelectedMember)
    {
        for (int i = 0; i < mons.Count; i++)
        {
            if(i == SelectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessagesText(string message)
    {
        messageText.text = message;
    }
}
