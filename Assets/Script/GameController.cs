using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Freeroam, Battle}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Awake()
    {
        ConditionDB.Init();
    }

    private void Start()
    {
        playerMovement.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerMovement.GetComponent<TeamParty>();
        var wildMon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildMon();

        battleSystem.StartBattle(playerParty, wildMon);
    }

    void EndBattle(bool won)
    {
        state = GameState.Freeroam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(state == GameState.Freeroam)
        {
            playerMovement.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
