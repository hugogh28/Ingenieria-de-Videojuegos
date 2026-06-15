using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BasicRat : MonoBehaviour, IPoolableObject, IHealth
{
    [Header("Rat Type")]
    [SerializeField] private RatType type;
    [SerializeField] private RatSubType subType;

    [Header("Combat")]
    [Tooltip("Si está desactivado, el daño/curación se ejecuta desde el State Pattern. Si está activado, se espera a un Animation Event.")]
    [SerializeField] private bool actionFromAnimationEvent = false;

    [Tooltip("Retraso opcional entre lanzar la animación y aplicar el daño cuando no se usan Animation Events.")]
    [SerializeField] [Min(0f)] private float stateActionImpactDelay = 0.15f;

    [SerializeField] private bool debugCombat = false;

    public RatData data;

    [HideInInspector] public Animator animator;
    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public GameObject player;
    [HideInInspector] public bool doneSomething = false;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public WaveManager waveManager;

    private PlayerController playerController;
    private RatStateMachine stateMachine;
    private Coroutine pendingStateActionCoroutine;

    public float health { get; set; }
    public bool Active { get; set; }

    public IRatState IdleState { get; private set; }
    public IRatState ChaseState { get; private set; }
    public IRatState AttackState { get; private set; }

    public bool ActionFromAnimationEvent => actionFromAnimationEvent;
    public bool DebugCombat => debugCombat;

    public string CurrentStateName => stateMachine?.CurrentState == null
        ? "None"
        : stateMachine.CurrentState.GetType().Name;

    public bool IsPlayerDetected => playerController != null && data != null && distanceToPlayer <= data.DetectionRange;
    public bool IsPlayerInActionRange => playerController != null && data != null && distanceToPlayer <= data.ActionRange;

    private void Awake()
    {
        CacheReferences();
        CreateStates();
        ResetRatState();
    }

    private void OnEnable()
    {
        CacheReferences();
        CreateStates();
        ResetRatState();
        ChangeState(IdleState);
    }

    private void OnDisable()
    {
        CancelPendingStateAction();
        stateMachine?.Clear(this);
    }

    private void Update()
    {
        try
        {
            if (!HasRequiredReferences())
            {
                CacheReferences();
                return;
            }

            stateMachine.Tick(this);
        }
        catch (Exception e)
        {
            Debug.LogError("Error en Update de " + gameObject.name + ": " + e.Message, this);
        }
    }

    private void CacheReferences()
    {
        if (data == null)
        {
            data = RatDataFactory.GetRatData(type, subType);
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        CachePlayerReference();
    }

    private void CachePlayerReference()
    {
        if (playerController != null)
        {
            player = playerController.gameObject;
            return;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");

        if (taggedPlayer == null)
        {
            player = null;
            playerController = null;
            return;
        }

        playerController = taggedPlayer.GetComponent<PlayerController>();

        if (playerController == null)
        {
            playerController = taggedPlayer.GetComponentInParent<PlayerController>();
        }

        if (playerController == null)
        {
            playerController = taggedPlayer.GetComponentInChildren<PlayerController>();
        }

        player = playerController != null ? playerController.gameObject : taggedPlayer;
    }

    private bool HasRequiredReferences()
    {
        return data != null
            && playerController != null
            && navAgent != null
            && stateMachine != null;
    }

    private void CreateStates()
    {
        if (stateMachine != null)
        {
            return;
        }

        stateMachine = new RatStateMachine();
        IdleState = new RatIdleState();
        ChaseState = new RatChaseState();
        AttackState = new RatAttackState();
    }

    private void ResetRatState()
    {
        if (data != null)
        {
            health = data.InitialHealth;
        }

        Active = true;
        doneSomething = false;
        distanceToPlayer = float.MaxValue;
        CancelPendingStateAction();
        SetWalkingAnimation(false);
        SetAgentStopped(false);
    }

    public void ChangeState(IRatState nextState)
    {
        stateMachine.ChangeState(this, nextState);
    }

    public PlayerController GetPlayerController()
    {
        if (playerController == null)
        {
            CachePlayerReference();
        }

        return playerController;
    }

    public void UpdateSensing()
    {
        PlayerController targetPlayer = GetPlayerController();

        if (targetPlayer == null)
        {
            distanceToPlayer = float.MaxValue;
            return;
        }

        distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
    }

    // Se conserva por compatibilidad con el código anterior.
    public void DetectPlayer()
    {
        UpdateSensing();

        if (IsPlayerDetected)
        {
            FacePlayer();

            if (!IsPlayerInActionRange)
            {
                MoveToPlayer();
            }
        }
    }

    // Se conserva por compatibilidad con el código anterior.
    public bool ShouldStop()
    {
        return IsPlayerInActionRange;
    }

    // Se conserva por compatibilidad con el código anterior.
    public IEnumerator HasDoneSomething()
    {
        doneSomething = true;
        yield return new WaitForSeconds(data.Delay);
        doneSomething = false;
    }

    public void SetWalkingAnimation(bool isWalking)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool("isWalking", isWalking);
    }

    public void TriggerActionAnimation()
    {
        if (animator == null || data == null || string.IsNullOrWhiteSpace(data.ActionNextToPlayer))
        {
            return;
        }

        animator.ResetTrigger(data.ActionNextToPlayer);
        animator.SetTrigger(data.ActionNextToPlayer);
    }

    public void PerformActionFromState()
    {
        if (actionFromAnimationEvent)
        {
            if (debugCombat)
            {
                Debug.Log($"{name}: esperando Animation Event para ejecutar la acción.", this);
            }

            return;
        }

        CancelPendingStateAction();

        if (stateActionImpactDelay <= 0f)
        {
            PerformAction();
            return;
        }

        pendingStateActionCoroutine = StartCoroutine(PerformStateActionAfterDelay());
    }

    public void PerformActionFromAnimationEvent()
    {
        if (!actionFromAnimationEvent)
        {
            return;
        }

        PerformAction();
    }

    private IEnumerator PerformStateActionAfterDelay()
    {
        yield return new WaitForSeconds(stateActionImpactDelay);
        pendingStateActionCoroutine = null;
        PerformAction();
    }

    private void CancelPendingStateAction()
    {
        if (pendingStateActionCoroutine == null)
        {
            return;
        }

        StopCoroutine(pendingStateActionCoroutine);
        pendingStateActionCoroutine = null;
    }

    public bool TryDamageObservedPlayer(float damage, bool requireActionRange = true)
    {
        PlayerController targetPlayer = GetPlayerController();

        if (targetPlayer == null)
        {
            if (debugCombat)
            {
                Debug.LogWarning($"{name}: no se puede dañar porque no se encontró PlayerController.", this);
            }

            return false;
        }

        UpdateSensing();

        if (requireActionRange && !IsPlayerInActionRange)
        {
            if (debugCombat)
            {
                Debug.Log($"{name}: daño cancelado. Jugador fuera de rango de acción.", this);
            }

            return false;
        }

        // Punto único de integración con el Observer de vida:
        // la rata NO modifica la HUD y NO resta currentHealth directamente.
        // Siempre llama al Subject PlayerController.TakeDamage(), que lanza HealthChanged.
        targetPlayer.TakeDamage(damage);
        return true;
    }

    public virtual void PerformAction()
    {
        if (debugCombat)
        {
            Debug.Log($"{name}: BasicRat.PerformAction no tiene efecto. Usa CommonRat, ShooterRat o SupportRat.", this);
        }
    }

    public void SetAgentStopped(bool stopped)
    {
        if (!CanUseNavAgent())
        {
            return;
        }

        navAgent.isStopped = stopped;

        if (stopped)
        {
            navAgent.ResetPath();
        }
    }

    public void MoveToPlayer()
    {
        if (!CanUseNavAgent())
        {
            return;
        }

        PlayerController targetPlayer = GetPlayerController();

        if (targetPlayer == null)
        {
            return;
        }

        navAgent.isStopped = false;
        navAgent.SetDestination(targetPlayer.transform.position);
    }

    public void FacePlayer()
    {
        PlayerController targetPlayer = GetPlayerController();

        if (targetPlayer == null)
        {
            return;
        }

        SmoothLookAt(targetPlayer.transform);
    }

    private bool CanUseNavAgent()
    {
        return navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;
    }

    public void TakeDamage(float dmg)
    {
        TakeDamage(dmg, false, transform.position + Vector3.up);
    }

    public void TakeDamage(float dmg, bool isCritical, Vector3 hitPoint)
    {
        if (dmg <= 0f)
        {
            return;
        }

        health -= dmg;
        GameFeelManager.Instance?.PlayRatHitFeedback(hitPoint, dmg, isCritical);

        if (health <= 0f)
        {
            Die();
        }
    }

    public bool RollDice(float actionProbability)
    {
        return UnityEngine.Random.value < actionProbability;
    }

    public void SmoothLookAt(Transform target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void ResetObject()
    {
        Active = false;
        gameObject.SetActive(false);
        ResetRatState();
    }

    public IPoolableObject Clone()
    {
        return Instantiate(this);
    }

    public void Die()
    {
        Vector3 deathPosition = transform.position + Vector3.up * 0.6f;

        PlayerController targetPlayer = GetPlayerController();

        if (targetPlayer != null && data != null)
        {
            int minimumPoints = Mathf.Max(0, data.PointsGivenAtDeath);
            int maximumPoints = Mathf.Max(minimumPoints, Mathf.RoundToInt(data.PointsGivenAtDeath * 1.5f));
            int pointsToGive = UnityEngine.Random.Range(minimumPoints, maximumPoints + 1);

            // Punto único de integración con el Observer de puntos:
            // la rata NO modifica la HUD y NO escribe directamente en un texto.
            // Siempre llama al Subject PlayerController.AddPoints(), que lanza PointsChanged.
            targetPlayer.AddPoints(pointsToGive);

            if (debugCombat)
            {
                Debug.Log($"{name}: otorga {pointsToGive} puntos al jugador. Observer de puntos notificado desde PlayerController.", this);
            }
        }

        if (waveManager != null)
        {
            waveManager.NotifyRatDied(this);
        }

        if (debugCombat)
        {
            Debug.Log("Die() llamado en " + gameObject.name, this);
        }

        GameFeelManager.Instance?.PlayRatDeathFeedback(deathPosition);
        ResetObject();
    }
}
