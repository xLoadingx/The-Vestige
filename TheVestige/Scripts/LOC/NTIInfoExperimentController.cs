using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheVestige.Scripts.LOC;

public class NTIInfoExperimentController : MonoBehaviour
{
    public static NTIInfoExperimentController Instance { get; private set; }

    public enum ExperimentPhase
    {
        Idle,
        ClosingWindow,
        FutureRecorded,
        OpeningWindow,
        Resolved,
        Paradox
    }
    
    [SerializeField] private Sector _sector;
    
    [Space]
    [SerializeField] private NomaiComputer _computer;
    [SerializeField] private NomaiGateway _window;
    [SerializeField] private CustomTimelineObliteration _timelineObliterationManager;

    [Space]
    [SerializeField] private SocketedQuantumObject _shard;
    [SerializeField] private SocketedQuantumObject _shardParadox;
    
    [Space]
    [SerializeField] private string _stateTextPrefix = "OBSERVATIONAL STATE";
    
    // Seconds after the windows closes to observe the predicted state
    public float secondsTillPresent = 4f;

    public ExperimentPhase Phase { get; private set; } = ExperimentPhase.Idle;
    public float PhaseTimer { get; private set; }
    
    private bool _hasParadoxOccured;
    private bool _hasRecordedFutureState;

    public int FutureShardState { get; private set; } = 0;

    private void SetFutureState()
    {
        int rand = Random.Range(0, _shard._socketList.Count);

        if (_shard._visibleInProbeSnapshot)
            FutureShardState = _shard._socketList.IndexOf(_shard._occupiedSocket);
        else
            FutureShardState = rand;
    }

    private bool IsStateContradictory() => _shard._occupiedSocket != _shard._socketList[FutureShardState];

    public string GetFutureStateText() => $"{_stateTextPrefix} {FutureShardState + 1}";

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        _hasParadoxOccured = false;
        Phase = ExperimentPhase.Idle;
        PhaseTimer = 0f;
        
        _timelineObliterationManager.ResetTimelineObliteration();

        if (_shard != null)
            _shard.gameObject.SetActive(true);

        if (_shardParadox != null)
            _shardParadox.gameObject.SetActive(false);
        
        _computer.ClearAllEntries();
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        
        _computer.ClearAllEntries();
    }

    public void Update()
    {
        switch (Phase)
        {
            case ExperimentPhase.Idle:
                break;
            
            case ExperimentPhase.ClosingWindow: {
                PhaseTimer += Time.deltaTime;

                if (_window.GetOpenFraction() <= 0.01f)
                    RecordFutureState();

                break;
            }

            case ExperimentPhase.FutureRecorded: {
                PhaseTimer += Time.deltaTime;

                if (PhaseTimer >= secondsTillPresent)
                {
                    Phase = ExperimentPhase.OpeningWindow;
                    PhaseTimer = 0f;
                }
                
                break;
            }

            case ExperimentPhase.OpeningWindow:
            {
                _window.OpenGate(null);
                ResolveExperimentResult();
                
                break;
            }
            
            case ExperimentPhase.Resolved:
                break;

            case ExperimentPhase.Paradox:
            {
                PhaseTimer += Time.deltaTime;
                
                if (_window.GetOpenFraction() >= 0.99f && !_timelineObliterationManager.effectStarted)
                    _timelineObliterationManager.BeginTimelineObliteration();
                
                break;
            }
        }
    }

    public void ActivateExperiment()
    {
        Start();
        
        if (Phase != ExperimentPhase.Idle || _hasParadoxOccured)
            return;

        Phase = ExperimentPhase.ClosingWindow;
        PhaseTimer = 0f;

        _window.CloseGate(null);
        
        _computer.DisplayEntry(2);
    }

    private void RecordFutureState()
    {
        SetFutureState();
        _computer.DisplayEntry(3);

        Phase = ExperimentPhase.FutureRecorded;
        PhaseTimer = 0f;
    }
    
    public void TriggerParadox()
    {
        if (_hasParadoxOccured || _shard == null || _shardParadox == null) 
            return;

        if (FutureShardState == -1) 
            return;

        var predictedSocket = _shard._socketList[FutureShardState];

        // Might make it force a paradox since it's a 1/3 chance, but
        // it's fine for now
        if (!IsStateContradictory())
        {
            Phase = ExperimentPhase.Resolved;
            PhaseTimer = 0f;
            _computer.DisplayEntry(4);
            return; // No paradox
        }
        
        _hasParadoxOccured = true;
        
        Phase = ExperimentPhase.Paradox;
        PhaseTimer = 0f;

        _computer.DisplayEntry(5);
        
        _shardParadox.gameObject.SetActive(true);
        _shardParadox.MoveToSocket(predictedSocket);
    }

    private void ResolveExperimentResult()
    {
        var predictedSocket = _shard._socketList[FutureShardState];

        if (Phase == ExperimentPhase.OpeningWindow)
        {
            if (!_shard._visibleInProbeSnapshot)
            {
                Phase = ExperimentPhase.Resolved;
                PhaseTimer = 0f;
                _computer.DisplayEntry(4);
                _shard.MoveToSocket(predictedSocket);
            }
            else
            {
                TriggerParadox(); 
            }
        }
    }
}