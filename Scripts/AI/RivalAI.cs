using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Q-Learning AI for Rival Character
/// Learns to dodge player attacks, maintain optimal distance, and use attacks effectively
/// </summary>
public class RivalAI : MonoBehaviour
{
    [Header("References")]
    public RivalController rivalController;
    public Transform playerTransform;
    public CharacterHealth playerHealth;
    private CharacterHealth myHealth;
    private DirectionController directionController;

    [Header("Q-Learning Parameters")]
    [Range(0.01f, 1f)]
    public float learningRate = 0.1f;
    [Range(0.8f, 0.99f)]
    public float discountFactor = 0.95f;
    [Range(0f, 1f)]
    public float explorationRate = 0.2f;
    [Range(0.01f, 0.99f)]
    public float explorationDecay = 0.995f;
    public float minExplorationRate = 0.05f;

    [Header("AI Settings")]
    public float decisionInterval = 0.3f;
    public float optimalDistance = 2.5f;
    public float dangerDistance = 1.8f;
    public float safeDistance = 8f;
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool enableLearning = true;

    // Q-Learning components
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
    private string previousState;
    private int previousAction;
    private float decisionTimer;

    // State tracking
    private Vector2 lastPlayerPosition;
    private float playerMovementSpeed;
    private bool playerAttacking;
    private float playerAttackTimer;

    // Movement tracking
    private Vector2 currentMovementDirection;
    private float movementTimer;

    // Actions
    private enum Action
    {
        Idle = 0,
        MoveTowardsPlayer = 1,
        MoveAwayFromPlayer = 2,
        StrafeLeft = 3,
        StrafeRight = 4,
        DashToPlayer = 5,
        DashAway = 6,
        AttackPunch = 7,
        AttackHeavy = 8,
        AttackPierce = 9
    }

    private void Awake()
    {
        if (rivalController == null)
            rivalController = GetComponent<RivalController>();

        if (directionController == null)
            directionController = GetComponent<DirectionController>();

        if (myHealth == null)
            myHealth = GetComponent<CharacterHealth>();

        if (rivalController == null || directionController == null)
        {
            Debug.LogError("RivalAI: Missing required components!");
            enabled = false;
        }
    }

    private void Start()
    {
        lastPlayerPosition = playerTransform.position;
        decisionTimer = decisionInterval;
    }

    private void Update()
    {
        UpdatePlayerTracking();
        ApplyMovement();

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            ExecuteQLearningStep();
        }
    }

    /// <summary>
    /// Track player movement and attack patterns
    /// </summary>
    private void UpdatePlayerTracking()
    {
        // Calculate player movement speed
        Vector2 currentPlayerPos = playerTransform.position;
        playerMovementSpeed = Vector2.Distance(currentPlayerPos, lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = currentPlayerPos;

        // Detect if player is attacking (simplified - can be improved with actual attack detection)
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerAttackTimer -= Time.deltaTime;

        // Assume player is attacking if very close
        if (distanceToPlayer < 2f && playerMovementSpeed < 1f)
        {
            playerAttacking = true;
            playerAttackTimer = 0.5f;
        }
        else if (playerAttackTimer <= 0f)
        {
            playerAttacking = false;
        }
    }

    /// <summary>
    /// Main Q-Learning step: Observe, Decide, Act, Learn
    /// </summary>
    private void ExecuteQLearningStep()
    {
        // Get current state
        string currentState = GetStateRepresentation();

        // Initialize Q-values for new state
        if (!qTable.ContainsKey(currentState))
        {
            qTable[currentState] = new float[System.Enum.GetValues(typeof(Action)).Length];
        }

        // Update Q-value for previous action if exists
        if (enableLearning && previousState != null && qTable.ContainsKey(previousState))
        {
            float reward = CalculateReward();
            UpdateQValue(previousState, previousAction, reward, currentState);
        }

        // Select and perform action
        int actionIndex = SelectAction(currentState);
        Action action = (Action)actionIndex;

        if (showDebugInfo)
        {
            Debug.Log($"State: {currentState} | Action: {action} | Exploration: {explorationRate:F3}");
        }

        PerformAction(action);

        // Store for next iteration
        previousState = currentState;
        previousAction = actionIndex;

        // Decay exploration rate
        if (enableLearning)
        {
            explorationRate = Mathf.Max(minExplorationRate, explorationRate * explorationDecay);
        }
    }

    /// <summary>
    /// Create state representation based on current game situation
    /// </summary>
    private string GetStateRepresentation()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Discretize distance
        string distState;
        if (distanceToPlayer < dangerDistance) distState = "VeryClose";
        else if (distanceToPlayer < optimalDistance) distState = "Close";
        else if (distanceToPlayer < safeDistance) distState = "Medium";
        else distState = "Far";

        // Health state
        float myHealthPercent = (float)myHealth.currentHealth / myHealth.maxHealth;
        string healthState = myHealthPercent > 0.7f ? "High" : myHealthPercent > 0.3f ? "Med" : "Low";

        float playerHealthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        string enemyHealthState = playerHealthPercent > 0.7f ? "High" : playerHealthPercent > 0.3f ? "Med" : "Low";

        // Attack availability
        bool canPunch = rivalController.punchTimer <= 0;
        bool canHeavy = rivalController.heavyTimer <= 0;
        bool canDash = rivalController.dashTimer <= 0;
        bool canPierce = rivalController.pierceTimer <= 0;

        string attackState = $"{(canPunch ? "1" : "0")}{(canHeavy ? "1" : "0")}{(canDash ? "1" : "0")}{(canPierce ? "1" : "0")}";

        // Player threat level
        string threatState = playerAttacking ? "Attacking" : playerMovementSpeed > 3f ? "Chasing" : "Idle";

        return $"{distState}_{healthState}_{enemyHealthState}_{attackState}_{threatState}";
    }

    /// <summary>
    /// Select action using epsilon-greedy strategy
    /// </summary>
    private int SelectAction(string state)
    {
        // Exploration: random action
        if (Random.value < explorationRate)
        {
            return Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
        }

        // Exploitation: best known action
        float[] qValues = qTable[state];
        float maxQ = qValues.Max();

        // If multiple actions have same Q-value, pick randomly among them
        List<int> bestActions = new List<int>();
        for (int i = 0; i < qValues.Length; i++)
        {
            if (Mathf.Approximately(qValues[i], maxQ))
            {
                bestActions.Add(i);
            }
        }

        return bestActions[Random.Range(0, bestActions.Count)];
    }

    /// <summary>
    /// Perform the selected action
    /// </summary>
    private void PerformAction(Action action)
    {
        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 dirAwayFromPlayer = -dirToPlayer;
        Vector2 strafeLeft = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        Vector2 strafeRight = new Vector2(dirToPlayer.y, -dirToPlayer.x);

        switch (action)
        {
            case Action.Idle:
                currentMovementDirection = Vector2.zero;
                break;

            case Action.MoveTowardsPlayer:
                if (!CheckWallInDirection(dirToPlayer))
                {
                    currentMovementDirection = dirToPlayer;
                    directionController.SetDirection(dirToPlayer);
                    movementTimer = decisionInterval;
                }
                break;

            case Action.MoveAwayFromPlayer:
                if (!CheckWallInDirection(dirAwayFromPlayer))
                {
                    currentMovementDirection = dirAwayFromPlayer;
                    directionController.SetDirection(dirAwayFromPlayer);
                    movementTimer = decisionInterval;
                }
                break;

            case Action.StrafeLeft:
                if (!CheckWallInDirection(strafeLeft))
                {
                    currentMovementDirection = strafeLeft;
                    directionController.SetDirection(dirToPlayer); // Keep facing player
                    movementTimer = decisionInterval;
                }
                break;

            case Action.StrafeRight:
                if (!CheckWallInDirection(strafeRight))
                {
                    currentMovementDirection = strafeRight;
                    directionController.SetDirection(dirToPlayer); // Keep facing player
                    movementTimer = decisionInterval;
                }
                break;

            case Action.DashToPlayer:
                if (rivalController.dashTimer <= 0 && !CheckDashCollision(dirToPlayer))
                {
                    directionController.SetDirection(dirToPlayer);
                    rivalController.PerformDash();
                    currentMovementDirection = Vector2.zero; // Stop normal movement
                    movementTimer = 0f;
                }
                break;

            case Action.DashAway:
                if (rivalController.dashTimer <= 0 && !CheckDashCollision(dirAwayFromPlayer))
                {
                    directionController.SetDirection(dirAwayFromPlayer);
                    rivalController.PerformDash();
                    currentMovementDirection = Vector2.zero; // Stop normal movement
                    movementTimer = 0f;
                }
                break;

            case Action.AttackPunch:
                if (rivalController.punchTimer <= 0)
                {
                    directionController.SetDirection(dirToPlayer);
                    rivalController.PerformPunch();
                }
                break;

            case Action.AttackHeavy:
                if (rivalController.heavyTimer <= 0)
                {
                    directionController.SetDirection(dirToPlayer);
                    rivalController.PerformHeavy();
                }
                break;

            case Action.AttackPierce:
                if (rivalController.pierceTimer <= 0)
                {
                    directionController.SetDirection(dirToPlayer);
                    rivalController.PerformPierce();
                }
                break;
        }
    }

    /// <summary>
    /// Check if there's a wall in the movement direction
    /// </summary>
    private bool CheckWallInDirection(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// Check if dash would collide with a wall using full dash distance
    /// </summary>
    private bool CheckDashCollision(Vector2 direction)
    {
        float dashDist = rivalController.dashDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dashDist, wallLayer);

        if (hit.collider != null)
        {
            if (showDebugInfo)
                Debug.Log($"Dash blocked: Wall detected at {hit.distance:F2} units");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Apply smooth movement every frame
    /// </summary>
    private void ApplyMovement()
    {
        if (movementTimer > 0f)
        {
            movementTimer -= Time.deltaTime;

            if (currentMovementDirection != Vector2.zero)
            {
                Vector2 movement = currentMovementDirection * rivalController.moveSpeed * Time.deltaTime;

                // Check wall before moving
                if (!CheckWallInDirection(currentMovementDirection))
                {
                    transform.Translate(movement);
                }
                else
                {
                    // Stop movement if hit wall
                    currentMovementDirection = Vector2.zero;
                    movementTimer = 0f;
                }
            }
        }
        else
        {
            currentMovementDirection = Vector2.zero;
        }
    }

    /// <summary>
    /// Calculate reward based on current situation
    /// </summary>
    private float CalculateReward()
    {
        float reward = 0f;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Damage rewards/penalties
        int myCurrentHealth = myHealth.currentHealth;
        int playerCurrentHealth = playerHealth.currentHealth;

        // Big reward for damaging player
        if (playerHealth.currentHealth < playerCurrentHealth)
        {
            reward += 10f;
        }

        // Big penalty for taking damage
        if (myHealth.currentHealth < myCurrentHealth)
        {
            reward -= 15f;
        }

        // Distance management rewards
        if (distanceToPlayer >= dangerDistance && distanceToPlayer <= optimalDistance)
        {
            reward += 1f; // Good position
        }
        else if (distanceToPlayer < dangerDistance && playerAttacking)
        {
            reward -= 5f; // Dangerous - too close while player attacking
        }
        else if (distanceToPlayer > safeDistance)
        {
            reward -= 0.5f; // Too far away
        }

        // Reward for dodging when player attacks
        if (playerAttacking && distanceToPlayer > dangerDistance)
        {
            reward += 3f; // Successfully maintained distance during attack
        }

        // Reward strategic dash usage
        if (previousAction == (int)Action.DashAway && playerAttacking)
        {
            reward += 2f; // Good defensive dash
        }
        else if (previousAction == (int)Action.DashToPlayer && distanceToPlayer > safeDistance && rivalController.dashTimer > rivalController.dashCooldown * 0.8f)
        {
            reward += 1.5f; // Good aggressive dash to close distance
        }

        // Small time penalty to encourage action
        reward -= 0.1f;

        return reward;
    }

    /// <summary>
    /// Update Q-value using Q-learning formula
    /// </summary>
    private void UpdateQValue(string state, int actionIndex, float reward, string nextState)
    {
        float currentQ = qTable[state][actionIndex];
        float maxNextQ = qTable[nextState].Max();

        // Q-learning update formula
        float newQ = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
        qTable[state][actionIndex] = newQ;

        if (showDebugInfo)
        {
            Debug.Log($"Q-Update: Reward={reward:F2}, OldQ={currentQ:F2}, NewQ={newQ:F2}");
        }
    }

    /// <summary>
    /// Save Q-table to PlayerPrefs (optional persistence)
    /// </summary>
    public void SaveQTable()
    {
        // Implementation for saving Q-table if needed
        Debug.Log($"Q-Table saved. Total states: {qTable.Count}");
    }

    /// <summary>
    /// Load Q-table from PlayerPrefs (optional persistence)
    /// </summary>
    public void LoadQTable()
    {
        // Implementation for loading Q-table if needed
        Debug.Log("Q-Table loaded.");
    }

    // Visualization for debugging
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Draw distance zones
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, optimalDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        // Draw line to player
        Gizmos.color = playerAttacking ? Color.red : Color.white;
        Gizmos.DrawLine(transform.position, playerTransform.position);
    }
}