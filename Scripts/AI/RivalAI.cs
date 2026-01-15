using System.Collections.Generic;
using System.Linq;
using System.IO;
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

    [Header("Weight Loading")]
    public bool weightsLoaded = false; // Ana menüden ayarlanacak

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

    // Singleton-like static reference for weight transfer
    public static Dictionary<string, float[]> LoadedQTable = null;
    public static bool ShouldLoadWeights = false;

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
            Debug.LogError("RivalAI:  Missing required components!");
            enabled = false;
        }
    }

    private void Start()
    {
        lastPlayerPosition = playerTransform.position;
        decisionTimer = decisionInterval;

        // Ağırlıklar yüklendiyse uygula
        if (ShouldLoadWeights && LoadedQTable != null)
        {
            qTable = new Dictionary<string, float[]>(LoadedQTable);
            weightsLoaded = true;
            explorationRate = minExplorationRate; // Eğitilmiş AI daha az explore eder
            Debug.Log($"AI Weights loaded!  States: {qTable.Count}");
        }
        else
        {
            // Rastgele mod - yüksek exploration
            weightsLoaded = false;
            explorationRate = 1.0f; // Tamamen rastgele
            Debug.Log("AI running in RANDOM mode (no weights loaded)");
        }
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
        Vector2 currentPlayerPos = playerTransform.position;
        playerMovementSpeed = Vector2.Distance(currentPlayerPos, lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = currentPlayerPos;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerAttackTimer -= Time.deltaTime;

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
    /// Main Q-Learning step:  Observe, Decide, Act, Learn
    /// </summary>
    private void ExecuteQLearningStep()
    {
        string currentState = GetStateRepresentation();

        if (!qTable.ContainsKey(currentState))
        {
            qTable[currentState] = new float[System.Enum.GetValues(typeof(Action)).Length];
        }

        if (enableLearning && previousState != null && qTable.ContainsKey(previousState))
        {
            float reward = CalculateReward();
            UpdateQValue(previousState, previousAction, reward, currentState);
        }

        int actionIndex = SelectAction(currentState);
        Action action = (Action)actionIndex;

        if (showDebugInfo)
        {
            string mode = weightsLoaded ? "TRAINED" : "RANDOM";
            Debug.Log($"[{mode}] State: {currentState} | Action: {action} | Exploration: {explorationRate:F3}");
        }

        PerformAction(action);

        previousState = currentState;
        previousAction = actionIndex;

        if (enableLearning && weightsLoaded)
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

        string distState;
        if (distanceToPlayer < dangerDistance) distState = "VeryClose";
        else if (distanceToPlayer < optimalDistance) distState = "Close";
        else if (distanceToPlayer < safeDistance) distState = "Medium";
        else distState = "Far";

        float myHealthPercent = (float)myHealth.currentHealth / myHealth.maxHealth;
        string healthState = myHealthPercent > 0.7f ? "High" : myHealthPercent > 0.3f ? "Med" : "Low";

        float playerHealthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        string enemyHealthState = playerHealthPercent > 0.7f ? "High" : playerHealthPercent > 0.3f ? "Med" : "Low";

        bool canPunch = rivalController.punchTimer <= 0;
        bool canHeavy = rivalController.heavyTimer <= 0;
        bool canDash = rivalController.dashTimer <= 0;
        bool canPierce = rivalController.pierceTimer <= 0;

        string attackState = $"{(canPunch ? "1" : "0")}{(canHeavy ? "1" : "0")}{(canDash ? "1" : "0")}{(canPierce ? "1" : "0")}";

        string threatState = playerAttacking ? "Attacking" : playerMovementSpeed > 3f ? "Chasing" : "Idle";

        return $"{distState}_{healthState}_{enemyHealthState}_{attackState}_{threatState}";
    }

    /// <summary>
    /// Select action using epsilon-greedy strategy
    /// </summary>
    private int SelectAction(string state)
    {
        // Ağırlık yüklenmemişse tamamen rastgele
        if (!weightsLoaded)
        {
            return Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
        }

        // Exploration:  random action
        if (Random.value < explorationRate)
        {
            return Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
        }

        // Exploitation: best known action
        float[] qValues = qTable[state];
        float maxQ = qValues.Max();

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
                    directionController.SetDirection(dirToPlayer);
                    movementTimer = decisionInterval;
                }
                break;

            case Action.StrafeRight:
                if (!CheckWallInDirection(strafeRight))
                {
                    currentMovementDirection = strafeRight;
                    directionController.SetDirection(dirToPlayer);
                    movementTimer = decisionInterval;
                }
                break;

            case Action.DashToPlayer:
                if (rivalController.dashTimer <= 0 && !CheckDashCollision(dirToPlayer))
                {
                    directionController.SetDirection(dirToPlayer);
                    rivalController.PerformDash();
                    currentMovementDirection = Vector2.zero;
                    movementTimer = 0f;
                }
                break;

            case Action.DashAway:
                if (rivalController.dashTimer <= 0 && !CheckDashCollision(dirAwayFromPlayer))
                {
                    directionController.SetDirection(dirAwayFromPlayer);
                    rivalController.PerformDash();
                    currentMovementDirection = Vector2.zero;
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

    private bool CheckWallInDirection(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        return hit.collider != null;
    }

    private bool CheckDashCollision(Vector2 direction)
    {
        float dashDist = rivalController.dashDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dashDist, wallLayer);

        if (hit.collider != null)
        {
            if (showDebugInfo)
                Debug.Log($"Dash blocked:  Wall detected at {hit.distance: F2} units");
            return true;
        }
        return false;
    }

    private void ApplyMovement()
    {
        if (movementTimer > 0f)
        {
            movementTimer -= Time.deltaTime;

            if (currentMovementDirection != Vector2.zero)
            {
                Vector2 movement = currentMovementDirection * rivalController.moveSpeed * Time.deltaTime;

                if (!CheckWallInDirection(currentMovementDirection))
                {
                    transform.Translate(movement);
                }
                else
                {
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

    private float CalculateReward()
    {
        float reward = 0f;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        int myCurrentHealth = myHealth.currentHealth;
        int playerCurrentHealth = playerHealth.currentHealth;

        if (playerHealth.currentHealth < playerCurrentHealth)
        {
            reward += 10f;
        }

        if (myHealth.currentHealth < myCurrentHealth)
        {
            reward -= 15f;
        }

        if (distanceToPlayer >= dangerDistance && distanceToPlayer <= optimalDistance)
        {
            reward += 1f;
        }
        else if (distanceToPlayer < dangerDistance && playerAttacking)
        {
            reward -= 5f;
        }
        else if (distanceToPlayer > safeDistance)
        {
            reward -= 0.5f;
        }

        if (playerAttacking && distanceToPlayer > dangerDistance)
        {
            reward += 3f;
        }

        if (previousAction == (int)Action.DashAway && playerAttacking)
        {
            reward += 2f;
        }
        else if (previousAction == (int)Action.DashToPlayer && distanceToPlayer > safeDistance && rivalController.dashTimer > rivalController.dashCooldown * 0.8f)
        {
            reward += 1.5f;
        }

        reward -= 0.1f;

        return reward;
    }

    private void UpdateQValue(string state, int actionIndex, float reward, string nextState)
    {
        float currentQ = qTable[state][actionIndex];
        float maxNextQ = qTable[nextState].Max();

        float newQ = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
        qTable[state][actionIndex] = newQ;

        if (showDebugInfo)
        {
            Debug.Log($"Q-Update:  Reward={reward:F2}, OldQ={currentQ:F2}, NewQ={newQ: F2}");
        }
    }

    /// <summary>
    /// Q-Table'ı JSON dosyasına kaydet
    /// </summary>
    /// <summary>
    /// Q-Table'ı JSON dosyasına kaydet
    /// NOT: WebGL'de çalışmaz - sadece Editor ve Standalone buildlerde kullanın
    /// </summary>
    /// <summary>
    /// Q-Table'ı JSON dosyasına kaydet
    /// NOT: WebGL'de çalışmaz! 
    /// </summary>
    public void SaveQTable(string filePath = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    Debug.LogWarning("SaveQTable is not supported on WebGL builds.");
    return;
#else
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = System.IO.Path.Combine(Application.persistentDataPath, "rival_ai_weights.json");
        }

        QTableData data = new QTableData();
        data.states = new List<StateData>();

        foreach (var kvp in qTable)
        {
            StateData stateData = new StateData();
            stateData.stateName = kvp.Key;
            stateData.qValues = kvp.Value.ToList();
            data.states.Add(stateData);
        }

        data.explorationRate = explorationRate;
        data.totalStates = qTable.Count;

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(filePath, json);

        Debug.Log($"Q-Table saved to:  {filePath} (States: {qTable.Count})");
#endif
    }

    /// <summary>
    /// JSON dosyasından Q-Table yükle
    /// NOT:  WebGL'de dosya yolu ile çalışmaz - Resources. Load kullanın
    /// </summary>
    public bool LoadQTable(string filePath)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    Debug.LogWarning("LoadQTable from file path is not supported on WebGL.  Use Resources.Load instead.");
    return false;
#else
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"Weight file not found: {filePath}");
            return false;
        }

        try
        {
            string json = System.IO.File.ReadAllText(filePath);
            QTableData data = JsonUtility.FromJson<QTableData>(json);

            qTable.Clear();
            foreach (var stateData in data.states)
            {
                qTable[stateData.stateName] = stateData.qValues.ToArray();
            }

            explorationRate = data.explorationRate;
            weightsLoaded = true;

            Debug.Log($"Q-Table loaded from: {filePath} (States: {qTable.Count})");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading Q-Table: {e.Message}");
            return false;
        }
#endif
    }

    /// <summary>
    /// Static method - Ana menüden ağırlık yüklemek için
    /// </summary>
    public static bool LoadWeightsFromJson(string jsonContent)
    {
        // jsonContent artık direkt JSON string, dosya yolu değil! 
        if (string.IsNullOrEmpty(jsonContent))
        {
            Debug.LogError("JSON content is empty!");
            return false;
        }

        try
        {
            // Direkt parse et - File.ReadAllText KULLANMA! 
            QTableData data = JsonUtility.FromJson<QTableData>(jsonContent);

            LoadedQTable = new Dictionary<string, float[]>();
            foreach (var stateData in data.states)
            {
                LoadedQTable[stateData.stateName] = stateData.qValues.ToArray();
            }

            ShouldLoadWeights = true;
            Debug.Log($"✓ Weights loaded (States: {LoadedQTable.Count})");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ JSON parse error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Ağırlıkları sıfırla (rastgele mod)
    /// </summary>
    public static void ResetWeights()
    {
        LoadedQTable = null;
        ShouldLoadWeights = false;
        Debug.Log("AI weights reset - will run in random mode");
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dangerDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, optimalDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        Gizmos.color = playerAttacking ? Color.red : Color.white;
        Gizmos.DrawLine(transform.position, playerTransform.position);
    }
}

/// <summary>
/// Q-Table verilerini JSON'a serialize etmek için
/// </summary>
[System.Serializable]
public class QTableData
{
    public List<StateData> states;
    public float explorationRate;
    public int totalStates;
}

[System.Serializable]
public class StateData
{
    public string stateName;
    public List<float> qValues;
}