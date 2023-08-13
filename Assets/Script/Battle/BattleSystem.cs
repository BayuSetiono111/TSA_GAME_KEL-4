using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;

    int currentAction;
    int currentMove;
    int currentMember;

    TeamParty playerParty;
    Mon wildMon;

    public void StartBattle(TeamParty playerParty, Mon wildMon)
    {
        this.playerParty = playerParty;
        this.wildMon = wildMon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealtyMon());
        enemyUnit.Setup(wildMon);


        partyScreen.Init();

        dialogBox.SetMoveName(playerUnit.Mon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Mon.BaseStats.Name} appeared.");

        ChooseFirstTurn();

    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Mon.Speed >= enemyUnit.Mon.Speed)
        { 
            ActionSelection(); 
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Mons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.enableActionSelector(true);
    }
    void OpenPartyPanel()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Mons);
        partyScreen.gameObject.SetActive(true);
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.enableActionSelector(false);
        dialogBox.enableDialogText(false);
        dialogBox.enableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Mon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMove)
        { 
            StartCoroutine(EnemyMove()); 
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Mon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Mon.OnBeforeMove();
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Mon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Mon);


        yield return dialogBox.TypeDialog($"{sourceUnit.Mon.BaseStats.Name} used {move.Base.Name}");

        move.PP--;

        sourceUnit.PlayAttackAnimation();

        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Mon, targetUnit.Mon);
        }
        else
        {
            var damageDetail = targetUnit.Mon.TakeDamage(move, sourceUnit.Mon);
            yield return targetUnit.Hud.UpdateHP();

            yield return ShowDamageDetails(damageDetail);
        }


        if (targetUnit.Mon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Mon.BaseStats.Name} Fainted");
            targetUnit.PlayFeintAnimation();

            yield return new WaitForSeconds(2f);

            CheckBattleOver(targetUnit);

            
        }

        //status damageing
        sourceUnit.Mon.OnAfterMove();
        yield return ShowStatusChanges(sourceUnit.Mon);
        yield return sourceUnit.Hud.UpdateHP();
        
        if (sourceUnit.Mon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Mon.BaseStats.Name} Fainted");
            sourceUnit.PlayFeintAnimation();

            yield return new WaitForSeconds(2f);

            CheckBattleOver(sourceUnit);

        }
    }
    IEnumerator RunMoveEffects(Move move, Mon source, Mon target)
    {
        var effects = move.Base.Effects;
        
        //stats changes
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoost(effects.Boosts);
            }
            else
            {
                source.ApplyBoost(effects.Boosts);
            }
        }
        //status changes
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Mon mon)
    {
        while (mon.StatusChanges.Count > 0)
        {
            var message = mon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextMon = playerParty.GetHealtyMon();
            if (nextMon != null)
            {
                OpenPartyPanel();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetail damageDetail)
    {
        if (damageDetail.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        
        if(damageDetail.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        if (damageDetail.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");

    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(currentAction == 0)
            {
                //fight
                MoveSelection();

            }
            else if(currentAction == 1)
            {
                //bag
            }
            else if(currentAction == 2)
            {
                OpenPartyPanel();
            }
            else if(currentAction == 3)
            {
                //run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }
        

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Mon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Mon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.enableMoveSelector(false);
            dialogBox.enableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Mons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var selectedMember = playerParty.Mons[currentMember];
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessagesText("Can't send out fainted Mon!");
                return;
            }
            if (selectedMember == playerUnit.Mon)
            {
                partyScreen.SetMessagesText("Mon already out!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitcMon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            state = BattleState.ActionSelection;
        }
    }

    IEnumerator SwitcMon(Mon newMon)
    {
        bool currentMonFainted = true;
        if (playerUnit.Mon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Mon.BaseStats.Name}");
            playerUnit.PlayFeintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMon);

        dialogBox.SetMoveName(newMon.Moves);

        yield return dialogBox.TypeDialog($"Go {newMon.BaseStats.Name}!");

        if(currentMonFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }

        StartCoroutine(EnemyMove());
    }
}
