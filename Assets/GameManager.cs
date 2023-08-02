using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
/// <summary>
/// The GameManager is responsible for game logic like turns, animations and reconciling played cards
/// </summary>
public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public CardManager playerCardManager;

    public Transform[] cardSlots;

    public TextMeshProUGUI deckSizeText;
    public TextMeshProUGUI discardPileText;
    public TextMeshProUGUI turnText;
    public GameObject victoryScreen;
    public GameObject lossScreen;
    
    public Animator playerAnimator, enemyAnimator, turnAnimator;
    public PlayerScript player;
    public EnemyAI enemy;
    public List<Entity> entities = new List<Entity>();
    public Clock clock;

    public int playerRoundTime = 0;
    public int enemyRoundTime = 0;

    public int maxTime = 12;

    public bool roundOver = false;

    public enum TurnState
    {
        Starting,
        PlayerTurn,
        EnemyTurn,
        RoundOver
    }

    public TurnState currentTurnState;

    void Start()
    {
        entities.Add(player);
        entities.Add(enemy);

        playerCardManager.Initialize(cardSlots);
        currentTurnState = TurnState.Starting;
    }

    public void StartGame()
    {
        StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
    }

    public void PlayAttackAnimation()
    {
        playerAnimator.SetBool("attack", true);
        StartCoroutine(ResetAttackAnimation());
    }

    private IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(.2f);
        playerAnimator.SetBool("attack", false);
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
                // Handle enemy's turn logic here
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
        foreach (Card card in playerCardManager.hand)
        {
            card.EndPlayerTurn();
        }

        if (enemy.timeSpent < maxTime)
        {
            turnText.text = "Enemy's turn!";
            StartCoroutine(PlayChangeTurnAnimationAndStartEnemyTurn());
            playerRoundTime = 0;
        }
    }

    public void EndEnemyTurn()
    {
        if (player.timeSpent < maxTime)
        {
            turnText.text = "Your turn!";
            StartCoroutine(PlayChangeTurnAnimationAndStartPlayerTurn());
            enemyRoundTime = 0;
        }
        else
        {
            EndDay(); // Fix if we have multiple enemies
        }
    }

    private IEnumerator PlayChangeTurnAnimationAndStartEnemyTurn()
    {        
        turnAnimator.SetBool("turnChanged", true);
        yield return new WaitForSeconds(turnAnimator.GetCurrentAnimatorStateInfo(0).length);
        turnAnimator.SetBool("turnChanged", false);
        yield return new WaitForSeconds(2f);
        StartEnemyTurn();
    }

    private IEnumerator PlayChangeTurnAnimationAndStartPlayerTurn()
    {
        turnAnimator.SetBool("turnChanged", true);
        yield return new WaitForSeconds(turnAnimator.GetCurrentAnimatorStateInfo(0).length);
        turnAnimator.SetBool("turnChanged", false);
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        currentTurnState = TurnState.PlayerTurn;
        playerCardManager.DrawCard();
        
        foreach (Card card in playerCardManager.hand)
        {
            card.StartPlayerTurn();
        }
    }

    private void StartEnemyTurn()
    {
        currentTurnState = TurnState.EnemyTurn;
        enemy.DrawEnemyCard();
        StartCoroutine(enemy.PlayTurn());
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
    public void PlayCard(Card card, bool isPlayer)
    {
        int energyCost = card.energy;
        int timeEffect = card.time;
        int workDone = card.work;

        // Apply effects from card
        if (isPlayer)
        {
            int timedifference = enemy.timeSpent - player.timeSpent;

            // Apply buff to player
            player.workDone += card.workDoneBuff;

            player.UpdateInfo(energyCost, workDone);
            player.energy -= energyCost;
            player.timeSpent += timeEffect;
            player.workDone += workDone;
            playerRoundTime += timeEffect;

            PlayAttackAnimation();

            StartCoroutine(MoveToDiscardPile(card));
            if (timedifference > 0) clock.IncrementClock(timeEffect - timedifference);
            else clock.IncrementClock(timeEffect);

            if (workDone > 0 )
            {
                StartCoroutine(playerCardManager.SpawnMultiplePaperPiles(card.work));
            }
            
        }
        else
        {
            enemy.workDone += card.workDoneDebuff;

            enemy.UpdateInfo(energyCost, workDone);
            enemy.energy -= energyCost;
            enemy.timeSpent += timeEffect;
            enemy.workDone += workDone;
            enemyRoundTime += timeEffect;
            if (enemy.timeSpent > player.timeSpent) clock.IncrementClock(enemy.timeSpent - player.timeSpent);
        }
        
        if (player.timeSpent > enemy.timeSpent) clock.UpdateClockColor(1); else if (player.timeSpent < enemy.timeSpent) clock.UpdateClockColor(-1); else clock.UpdateClockColor(0);
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
}
