using System;
using System.Collections.Generic;
using UnityEngine;
using RadioDispatch.Calls;
using RadioDispatch.Radio;
using RadioDispatch.Units;

namespace RadioDispatch.Core
{
    /// <summary>
    /// Entry point for the simulation. Responsible for bootstrapping managers and tracking the high-level game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Menu,
            Briefing,
            Simulation,
            Pause,
            Debrief
        }

        [SerializeField]
        private GameState currentState = GameState.Menu;

        [Header("Core Managers")]
        [SerializeField]
        private CallManager callManager;

        [SerializeField]
        private UnitManager unitManager;

        [SerializeField]
        private RadioSystem radioSystem;

        [SerializeField]
        private ScoringSystem scoringSystem;

        [SerializeField]
        private Records.RecordsManager recordsManager;

        [SerializeField]
        private ChatterManager chatterManager;

        [Header("Content & Progression")]
        [SerializeField]
        private ShiftConfig shiftConfig;

        [SerializeField]
        private CallLibrary defaultCallLibrary;

        [SerializeField]
        private UnitRoster defaultUnitRoster;

        [SerializeField]
        private Records.RecordsDatabase defaultRecords;

        [SerializeField]
        private ChatterLibrary defaultChatterLibrary;

        private float shiftTimer;
        private float difficultyTimer;
        private bool shiftRunning;

        public event Action<string> OnShiftEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Duplicate GameManager detected. Destroying instance.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            BootstrapContent();
            BeginShift();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// Changes the current game state and performs any transition logic needed for the new state.
        /// </summary>
        /// <param name="state">Desired next state.</param>
        public void SetState(GameState state)
        {
            currentState = state;
            Debug.Log($"Game state changed to: {state}");
        }

        /// <summary>
        /// Retrieves the current state for other systems.
        /// </summary>
        public GameState GetState() => currentState;

        /// <summary>
        /// Starts a new dispatcher shift and initializes timers/difficulty.
        /// </summary>
        public void BeginShift()
        {
            if (callManager == null || scoringSystem == null)
            {
                Debug.LogWarning("GameManager missing dependencies. Cannot start shift.");
                return;
            }

            scoringSystem.ResetScore();
            shiftTimer = shiftConfig != null ? shiftConfig.ShiftLengthSeconds : 600f;
            difficultyTimer = 0f;
            shiftRunning = true;

            if (shiftConfig != null)
            {
                callManager.SetAutoSpawnInterval(shiftConfig.SpawnIntervalStart);
            }

            callManager.StartSpawning();
            SetState(GameState.Simulation);
            radioSystem?.LogMessage("Dispatch", "Shift started. Stay sharp.");
        }

        /// <summary>
        /// Manual tick entry point for tests and non-MonoBehaviour callers.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!shiftRunning || deltaTime <= 0f)
            {
                return;
            }

            shiftTimer -= deltaTime;
            difficultyTimer += deltaTime;

            if (shiftTimer <= 0f)
            {
                EndShift(success: true, reason: "Shift timer complete.");
                return;
            }

            if (shiftConfig != null && callManager != null && callManager.ActiveUnresolvedCount > shiftConfig.MaxUnresolvedCalls)
            {
                EndShift(success: false, reason: "Too many unresolved incidents.");
                return;
            }

            if (shiftConfig != null && callManager != null && difficultyTimer >= shiftConfig.SpawnRampSeconds)
            {
                difficultyTimer = 0f;
                var newInterval = Mathf.Max(shiftConfig.SpawnIntervalMin, callManager.AutoSpawnInterval - shiftConfig.SpawnIntervalStep);
                callManager.SetAutoSpawnInterval(newInterval);
            }
        }

        /// <summary>
        /// Ends the current shift, stops spawning, and persists performance.
        /// </summary>
        public void EndShift(bool success, string reason)
        {
            if (!shiftRunning)
            {
                return;
            }

            shiftRunning = false;
            callManager?.StopSpawning();
            currentState = success ? GameState.Debrief : GameState.Pause;

            var rank = scoringSystem?.GetRank();
            ProfileManager.SaveBestScore(scoringSystem?.GetScore() ?? 0);
            ProfileManager.SaveLastRank(rank);
            radioSystem?.LogMessage("Dispatch", success ? "Shift complete." : "Shift failed.");
            radioSystem?.LogMessage("System", reason);

            OnShiftEnded?.Invoke(reason);
        }

        private void BootstrapContent()
        {
            if (defaultCallLibrary != null && callManager != null)
            {
                callManager.SetTemplates(defaultCallLibrary.GetTemplates());
            }

            if (defaultUnitRoster != null && unitManager != null)
            {
                unitManager.SetUnits(defaultUnitRoster.GetClonedUnits());
            }

            if (unitManager != null && unitManager.GetAllUnits().Count == 0)
            {
                unitManager.SetUnits(BuildFallbackUnits());
            }

            if (callManager != null && !callManager.HasTemplates)
            {
                callManager.SetTemplates(BuildFallbackTemplates());
            }

            if (recordsManager != null)
            {
                if (defaultRecords != null)
                {
                    recordsManager.SetDatabase(defaultRecords);
                    recordsManager.LoadDatabases();
                }
                else
                {
                    // Seed a few sample records when no asset is wired to keep radio lookups meaningful.
                    var fallbackDb = ScriptableObject.CreateInstance<Records.RecordsDatabase>();
                    fallbackDb.Records = BuildFallbackRecords();
                    recordsManager.SetDatabase(fallbackDb);
                    recordsManager.LoadDatabases();
                }
            }

            if (chatterManager != null)
            {
                if (defaultChatterLibrary != null)
                {
                    chatterManager.SetLibrary(defaultChatterLibrary);
                }
                else
                {
                    chatterManager.SetLibrary(BuildFallbackChatterLibrary());
                }
            }
        }

        private List<CallTemplate> BuildFallbackTemplates()
        {
            var robbery = ScriptableObject.CreateInstance<CallTemplate>();
            robbery.id = "2101";
            robbery.title = "Armed Robbery";
            robbery.description = "Caller reports masked suspects with weapons.";
            robbery.location = "Downtown Bank";
            robbery.priority = CallPriority.Critical;
            robbery.category = CallCategory.Robbery;
            robbery.allowedResponseTime = 90f;
            robbery.minimumRecommendedUnits = 2;
            robbery.recommendedUnitTypes = new List<UnitType> { UnitType.Patrol, UnitType.SWAT };

            var traffic = ScriptableObject.CreateInstance<CallTemplate>();
            traffic.id = "3102";
            traffic.title = "Multi-Car Pileup";
            traffic.description = "Multiple vehicles involved with possible injuries.";
            traffic.location = "Highway Ramp";
            traffic.priority = CallPriority.High;
            traffic.category = CallCategory.Traffic;
            traffic.allowedResponseTime = 120f;
            traffic.minimumRecommendedUnits = 2;
            traffic.recommendedUnitTypes = new List<UnitType> { UnitType.Traffic, UnitType.EMS };

            var disturbance = ScriptableObject.CreateInstance<CallTemplate>();
            disturbance.id = "4103";
            disturbance.title = "Loud Disturbance";
            disturbance.description = "Neighbors report shouting and possible fight.";
            disturbance.location = "2nd & Willow";
            disturbance.priority = CallPriority.Medium;
            disturbance.category = CallCategory.Disturbance;
            disturbance.allowedResponseTime = 180f;
            disturbance.minimumRecommendedUnits = 1;
            disturbance.recommendedUnitTypes = new List<UnitType> { UnitType.Patrol };

            var medical = ScriptableObject.CreateInstance<CallTemplate>();
            medical.id = "5104";
            medical.title = "Medical Emergency";
            medical.description = "Civilian unconscious, CPR in progress.";
            medical.location = "Mall Food Court";
            medical.priority = CallPriority.High;
            medical.category = CallCategory.Medical;
            medical.allowedResponseTime = 60f;
            medical.minimumRecommendedUnits = 1;
            medical.recommendedUnitTypes = new List<UnitType> { UnitType.EMS, UnitType.Patrol };

            var grandTheft = ScriptableObject.CreateInstance<CallTemplate>();
            grandTheft.id = "2025-340555";
            grandTheft.title = "Grand Theft Just Occurred";
            grandTheft.description = "Victim reports two suspects smashed her 2024 Accord window, took laptop and purse.";
            grandTheft.location = "Target Parking Lot - 1690 Folsom St";
            grandTheft.priority = CallPriority.Medium;
            grandTheft.category = CallCategory.Robbery;
            grandTheft.allowedResponseTime = 150f;
            grandTheft.minimumRecommendedUnits = 2;
            grandTheft.recommendedUnitTypes = new List<UnitType> { UnitType.Patrol, UnitType.AirSupport };

            var stabbing = ScriptableObject.CreateInstance<CallTemplate>();
            stabbing.id = "2025-340994";
            stabbing.title = "Stabbing Just Occurred";
            stabbing.description = "Multiple callers report male stabbed multiple times; suspect fled southbound on foot.";
            stabbing.location = "El Gran Taco - 1950 Mission St";
            stabbing.priority = CallPriority.Critical;
            stabbing.category = CallCategory.Medical;
            stabbing.allowedResponseTime = 45f;
            stabbing.minimumRecommendedUnits = 3;
            stabbing.recommendedUnitTypes = new List<UnitType> { UnitType.EMS, UnitType.Patrol, UnitType.SWAT };

            return new List<CallTemplate> { robbery, traffic, disturbance, medical, grandTheft, stabbing };
        }

        private List<Unit> BuildFallbackUnits()
        {
            return new List<Unit>
            {
                new Unit { Id = "12", DisplayName = "Unit 12", Status = UnitStatus.Available, CurrentZone = "Zone B", Type = UnitType.Patrol },
                new Unit { Id = "21", DisplayName = "Unit 21", Status = UnitStatus.Available, CurrentZone = "Zone C", Type = UnitType.Traffic },
                new Unit { Id = "35", DisplayName = "Unit 35", Status = UnitStatus.Available, CurrentZone = "Zone D", Type = UnitType.EMS },
                new Unit { Id = "44", DisplayName = "Unit 44", Status = UnitStatus.Available, CurrentZone = "Zone A", Type = UnitType.K9 },
                new Unit { Id = "55", DisplayName = "Unit 55", Status = UnitStatus.Available, CurrentZone = "Zone Downtown", Type = UnitType.SWAT }
            };
        }

        private List<Records.RecordEntry> BuildFallbackRecords()
        {
            return new List<Records.RecordEntry>
            {
                new Records.RecordEntry { Id = "A12345", Name = "Jamie Lee", Type = Records.RecordType.Civilian, VehiclePlate = "4HND213", Notes = "Valid license, no wants." },
                new Records.RecordEntry { Id = "B98211", Name = "Morgan Diaz", Type = Records.RecordType.Prisoner, Notes = "On parole, caution: resistive." },
                new Records.RecordEntry { Id = "U-21", Name = "Officer Taylor", Type = Records.RecordType.Officer, VehiclePlate = "UNIT21", Notes = "Traffic division." },
                new Records.RecordEntry { Id = "CAR-77", Name = "Unknown", Type = Records.RecordType.Vehicle, VehiclePlate = "7XKZ991", Notes = "Reported stolen." },
                new Records.RecordEntry { Id = "SARAH-340555", Name = "Sarah Nguyen", Type = Records.RecordType.Civilian, Notes = "Grand theft victim, 2024 Honda Accord." },
                new Records.RecordEntry { Id = "BOLO-CAMRY", Name = "Silver Toyota Camry", Type = Records.RecordType.Vehicle, VehiclePlate = "NO-PLATE", Notes = "BOLO issued, linked to 487 at 1690 Folsom." }
            };
        }

        private ChatterLibrary BuildFallbackChatterLibrary()
        {
            var chatter = ScriptableObject.CreateInstance<ChatterLibrary>();
            chatter.Entries = new List<ChatterEntry>
            {
                new ChatterEntry
                {
                    Id = "plate-check-7XKZ991",
                    Title = "Traffic plate check",
                    UnitId = "21",
                    TransmissionText = "Dispatch, run plate 7XKZ991 for me.",
                    RecordQuery = "7XKZ991",
                    UseTypeFilter = true,
                    TypeFilter = Records.RecordType.Vehicle
                },
                new ChatterEntry
                {
                    Id = "victim-followup-sarah",
                    Title = "Victim callback",
                    UnitId = "3S12",
                    TransmissionText = "Central, confirm victim Sarah Nguyen contact for the Target lot 487.",
                    RecordQuery = "Sarah Nguyen",
                    DispatchResponseOverride = "Copy, victim Sarah Nguyen is on scene. Contact confirmed.",
                    UseTypeFilter = true,
                    TypeFilter = Records.RecordType.Civilian
                },
                new ChatterEntry
                {
                    Id = "bolo-camry",
                    Title = "BOLO reminder",
                    UnitId = "Air-5",
                    TransmissionText = "Air-5 checking for the silver Camry, no plate on file.",
                    RecordQuery = "NO-PLATE",
                    UseTypeFilter = true,
                    TypeFilter = Records.RecordType.Vehicle,
                    DispatchResponseOverride = "Dispatch copies, BOLO Camry last seen southbound."
                }
            };

            return chatter;
        }
    }
}
