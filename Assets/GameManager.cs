using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
/// <summary>
/// The GameManager is responsible for game logic like turns, animations and reconciling played cards
/// </summary>
public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public CardManager playerCardManager;

    public List<Transform> cardSlots;

    public TextMeshProUGUI deckSizeText;
    public TextMeshProUGUI discardPileText;
    public TextMeshProUGUI turnText;
    public GameObject victoryScreen;
    public GameObject lossScreen;
    
    public Animator playerAnimator, enemyAnimator, turnAnimator;
    public PlayerScript player;
    public EnemyAI enemy;
    public List<Entity> entities = new List<Entity>();
    private Queue<EnemyAI> enemiesTurnQueue = new Queue<EnemyAI>();
    public Clock clock;

    public int playerRoundTime = 0;
    public int enemyRoundTime = 0;

    public int maxTime = 12;
    private int turnCounter = 0;
    public bool roundOver = false;

    private bool lunchOptionChosen = false;
    public GameObject lunchBreakUI, showLunchMenuButton;

    private bool extraTurn = false;
    private bool extraCards = false;

    public enum TurnState
    {
        Starting,
        PlayerTurn,
        EnemyTurn,
        LunchBreak,
        RoundOver
    }

    public TurnState currentTurnState;

    private enum LunchOptions
    {
        SardineSushi,
        CatnipSandwich,
        TunaSalad
    }

    private LunchOptions selectedLunchOption;

    void Start()
    {
        currentTurnState = TurnState.Starting;
        
        entities.Add(player);
        entities.Add(enemy);

    }

    public void StartGame()
    {
        StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
    }

    public void PlayAttackAnimation()
    {
        playerAnimator.SetBool("attack", true);
        enemyAnimator.SetBool("enemyHurt", true);
        StartCoroutine(ResetAttackAnimation());
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(.2f);
        playerAnimator.SetBool("attack", false);
        enemyAnimator.SetBool("enemyHurt", false);
    }

    public void PlayEnemyAttackAnimation()
    {
        enemyAnimator.SetBool("enemyAttack", true);
        StartCoroutine(ResetEnemyAttackAnimation());
    }

    private IEnumerator ResetEnemyAttackAnimation()
    {
        yield return new WaitForSeconds(.2f);
        enemyAnimator.SetBool("enemyAttack", false);
    }

    public void PlayChangeTurnAnimation()
    {
        turnAnimator.SetBool("turnChanged", true);
        StartCoroutine(ResetChangeTurnAnimation());
    }

    private IEnumerator ResetChangeTurnAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        turnAnimator.SetBool("turnChanged", false);
    }

    public void PlayDebuffAnimation(Animator animator)
    {
        animator.SetBool("hurt", true);
        StartCoroutine(ResetHurtAnimation(animator));
    }

    private IEnumerator ResetHurtAnimation(Animator animator)
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("hurt", false);
    }

    private void Update()
    {
        switch (currentTurnState)
        {
            case TurnState.Starting:
                break;
            case TurnState.PlayerTurn:
                deckSizeText.text = playerCardManager.deck.Count.ToString();
                discardPileText.text = playerCardManager.discardPile.Count.ToString();
                break;
            case TurnState.EnemyTurn:
                break;
            case TurnState.LunchBreak:
                break;
            case TurnState.RoundOver:
                break;
        }

        // Check if it's time to end the day
        if (player.timeSpent >= maxTime && enemy.timeSpent >= maxTime && !roundOver)
        {
            EndDay();
        }
    }

    public void EndPlayerTurn()
    {
        if (extraTurn)
        {
            extraTurn = false;
            StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
        }
        else
        {
            StartCoroutine(PlayChangeTurnAnimationAndStartEnemyTurn());
        }
    }

    public IEnumerator EndEnemyTurn()
    {
        turnCounter += 1;
        yield return clock.IncrementClock(1);
        CheckForLunchBreak();
        CheckForEndOfDay();
    }

    private void CheckForLunchBreak()
    {
        if (turnCounter == 1)
        {
            currentTurnState = TurnState.LunchBreak;            
            StartCoroutine(HandleLunchBreakEvent());
        }
        else
        {
            StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
        }
    }

    private void CheckForEndOfDay()
    {
        if (turnCounter == 8)
        {
            EndDay();
        }
    }

    private IEnumerator HandleLunchBreakEvent()
    {
        lunchBreakUI.SetActive(true);
        showLunchMenuButton.SetActive(true);
        while (!lunchOptionChosen)
        {
            yield return null;
        }

        switch (selectedLunchOption)
        {
            case LunchOptions.SardineSushi:
                player.ResetEnergyToMax();
                break;
            case LunchOptions.CatnipSandwich:
                extraTurn = true;
                break;
            case LunchOptions.TunaSalad:
                extraCards = true;
                break;
        }

        lunchOptionChosen = false;
        lunchBreakUI.SetActive(false);
        showLunchMenuButton.SetActive(false);
        foreach (Entity entity in entities)
        {
            yield return new WaitForSeconds(1f);
            entity.ResetEnergyToMax();
        }

        // Once the lunch break event has been handled, it's time to start the player's turn
        // You might want to add a delay or some kind of indicator that the lunch break is over before proceeding
        yield return new WaitForSeconds(1f);
        StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
    }

    private IEnumerator PlayChangeTurnAnimationAndStartEnemyTurn()
    {
        turnText.text = "Enemy's turn!";
        turnAnimator.SetBool("turnChanged", true);
        yield return new WaitForSeconds(turnAnimator.GetCurrentAnimatorStateInfo(0).length);
        turnAnimator.SetBool("turnChanged", false);
        yield return new WaitForSeconds(2f);
        StartEnemyTurn();
    }

    private IEnumerator PlayChangeTurnAnimationAndStartPlayerTurn()
    {
        turnText.text = "Your turn!";
        turnAnimator.SetBool("turnChanged", true);
        yield return new WaitForSeconds(turnAnimator.GetCurrentAnimatorStateInfo(0).length);
        turnAnimator.SetBool("turnChanged", false);        

        StartCoroutine(StartPlayerTurn());
    }

    private IEnumerator StartPlayerTurn()
    {
        yield return new WaitForSeconds(0.5f);
        if (turnCounter > 0) playerCardManager.DrawCard();        
        if (extraCards)
        {
            yield return new WaitForSeconds(0.5f);
            playerCardManager.DrawCard();
            yield return new WaitForSeconds(0.5f);
            playerCardManager.DrawCard();
            extraCards = false;
        }
        currentTurnState = TurnState.PlayerTurn;
    }

    private void StartEnemyTurn()
    {
        currentTurnState = TurnState.EnemyTurn;

        foreach (var entity in entities)
        {
            if (entity is EnemyAI enemy)
            {
                enemy.turnFinished = false;
                enemiesTurnQueue.Enqueue(enemy);
            }
        }

        StartNextEnemyTurn();
    }

    private void StartNextEnemyTurn()
    {
        if (enemiesTurnQueue.Count > 0)
        {
            EnemyAI currentEnemy = enemiesTurnQueue.Dequeue();            
            currentEnemy.DrawEnemyCard();
            StartCoroutine(EnemyTurnRoutine(currentEnemy));
        }
        else
        {
            StartCoroutine(EndEnemyTurn());            
        }
    }

    private IEnumerator EnemyTurnRoutine(EnemyAI currentEnemy)
    {
        
        while (!currentEnemy.turnFinished)
        { 
            yield return StartCoroutine(currentEnemy.PlayTurn());
        }

        // Once the enemy has finished its turn, start the next enemy's turn
        StartNextEnemyTurn();
    }

    // Call this function to end the day
    public void EndDay()
    {
        roundOver = true;
        player.timeSpent = 0;
        enemy.timeSpent = 0;

        StartCoroutine(PlayRoundOverAnimation());        
    }

    private IEnumerator PlayRoundOverAnimation()
    {
        turnAnimator.SetBool("roundOver", true);
        yield return new WaitForSeconds(turnAnimator.GetCurrentAnimatorStateInfo(0).length);

        StartCoroutine(RoundSummary());
    }

    private IEnumerator RoundSummary()
    {
        string result;
        DisableIngameLayer();
        if (player.workDone > enemy.workDone) victoryScreen.SetActive(true);
        else if (player.workDone < enemy.workDone) lossScreen.SetActive(true);
        else result = "It's a draw!"; // Do something overtime related here

        yield return null;

    }

    // Call this function when a player plays a card
    public void PlayCard(Card card, Entity entity, Entity target)
    {
        int energyCost = card.energy;
        int timeEffect = card.time;
        int workDone = card.work;

        entity.workDone += card.workDoneDebuff;
        entity.energy -= energyCost;
        entity.timeSpent += timeEffect;
        entity.workDone += workDone;
        entity.roundTime += timeEffect;
        
        entity.UpdateHud();

        // Apply effects from card
        if (entity is PlayerScript)
        {
            // Do animation stuff
            if (card.workDoneBuff != 0 || card.energyGainBuff != 0 || card.timeCostReduction != 0)
            {
                //entity.PlayBuffAnimation();
            }

            if (card.workDoneDebuff != 0)
            {
                target.PlayDebuffAnimation();
            }

            if (card.work > 0)
            {
                entity.PlayAttackAnimation();
                target.PlayHurtAnimation();
            }

            //int timedifference = enemy.timeSpent - player.timeSpent;

            StartCoroutine(MoveToDiscardPile(card));
            //if (timedifference > 0) clock.IncrementClock(timeEffect - timedifference);
            //else clock.IncrementClock(timeEffect);

            if (workDone > 0 )
            {
                StartCoroutine(playerCardManager.SpawnMultiplePaperPiles(card.work));
            }
            
        }
        else
        {
            
            //if (enemy.timeSpent > player.timeSpent) clock.IncrementClock(enemy.timeSpent - player.timeSpent);
        }
        
        //if (player.timeSpent > enemy.timeSpent) clock.UpdateClockColor(1); else if (player.timeSpent < enemy.timeSpent) clock.UpdateClockColor(-1); else clock.UpdateClockColor(0);
    }

    IEnumerator MoveToDiscardPile(Card card)
    {
        Vector3 originalScale = card.transform.localScale; // Store the original scale
        Vector3 targetScale = originalScale / 2f; // Calculate the target scale

        float elapsedTime = 0;

        // Loop over the duration
        while (elapsedTime < .5f)
        {
            // Calculate the new scale
            Vector3 newScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / .5f);

            // Apply the new scale
            card.transform.localScale = newScale;

            elapsedTime += Time.deltaTime; // Increment the elapsed time
            yield return null;
        }

        // Ensure the scale is exactly the target scale at the end
        card.transform.localScale = targetScale;
        playerCardManager.RemoveFromHand(card);
        playerCardManager.discardPile.Add(card);
        card.gameObject.SetActive(false);
    }

    public void DisableIngameLayer()
    {
        // Calculate the layer mask for the "Ingame" layer
        int ingameLayerMask = 1 << LayerMask.NameToLayer("Ingame");

        // Remove the "Ingame" layer from the culling mask
        mainCamera.cullingMask &= ~ingameLayerMask;
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void OnSardineSushiButtonClicked()
    {
        selectedLunchOption = LunchOptions.SardineSushi;
        lunchOptionChosen = true;
    }

    public void OnCatnipSandwichButtonClicked()
    {
        selectedLunchOption = LunchOptions.CatnipSandwich;
        lunchOptionChosen = true;
    }

    public void OnTunaSaladButtonClicked()
    {
        selectedLunchOption = LunchOptions.TunaSalad;
        lunchOptionChosen = true;
    }
}
