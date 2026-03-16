using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace History
{
    public enum GameState
    {
        Menu, Briefing, Examine, Reference,
        ChooseHall, ChooseWall, Confirm,
        Placed, Results, TimeUp
    }

    public class HistoryGameManager : MonoBehaviour
    {
        public static HistoryGameManager I { get; private set; }

        // Data
        public ConfigData Config { get; private set; }
        public List<ArtifactInfo> Catalog { get; private set; }
        public CatalogDB CatalogDB { get; private set; }

        // State
        public GameState State { get; private set; } = GameState.Menu;
        public RoundData CurrentRound { get; private set; }
        public int CurrentArtifactIndex { get; private set; }
        public ArtifactInfo CurrentArtifact =>
            CurrentRound != null && CurrentRound.artifacts != null &&
            CurrentArtifactIndex < CurrentRound.artifacts.Count
                ? CurrentRound.artifacts[CurrentArtifactIndex] : null;

        // Timer
        public float TimeRemaining { get; private set; }
        public bool TimerRunning { get; private set; }
        public int TimeOverrideSec { get; set; } = 0; // 0 = use round default
        public float RoundElapsed { get; private set; } // сколько прошло с начала раунда
        public static readonly int[] TimeOptions = { 30, 60, 90, 120, 180, 300 };

        // Notebook
        public Dictionary<string, bool> RevealedTraits { get; private set; } = new Dictionary<string, bool>();
        public HashSet<string> UsedTools { get; private set; } = new HashSet<string>();
        public HashSet<string> ExaminedZones { get; private set; } = new HashSet<string>();

        // Placement
        public string ChosenHall { get; set; }
        public string ChosenWall { get; set; }
        public List<PlacementResult> Placements { get; private set; } = new List<PlacementResult>();
        public int PlacedCount => Placements.Count(p => !p.skipped);
        public int ArtifactTotal => CurrentRound != null ? CurrentRound.artifacts.Count : 0;
        public int RemainingCount => ArtifactTotal - Placements.Count;

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<string, string> OnTraitRevealed;
        public event Action<ZoneData> OnZoneExamined;

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            LoadData();
        }

        void LoadData()
        {
            Config = DataLoader.LoadConfig();
            Catalog = DataLoader.LoadCatalog();
            CatalogDB = new CatalogDB(Catalog);
            Debug.Log($"[History] Loaded {Config.halls.Count} halls, {Config.walls.Count} walls, {Config.tools.Count} tools, {Catalog.Count} catalog entries");
        }

        void Update()
        {
            if (TimerRunning && TimeRemaining > 0)
            {
                TimeRemaining -= Time.deltaTime;
                RoundElapsed += Time.deltaTime;
                if (TimeRemaining <= 0)
                {
                    TimeRemaining = 0;
                    TimerRunning = false;
                    SetState(GameState.TimeUp);
                }
            }
        }

        public void SetState(GameState s)
        {
            State = s;
            Debug.Log($"[History] State -> {s}");
            OnStateChanged?.Invoke(s);
        }

        // ========== FLOW ==========

        public void StartNewRound(string roundId = "round_01")
        {
            CurrentRound = DataLoader.LoadRound(roundId);
            Placements.Clear();
            CurrentArtifactIndex = 0;
            int time = TimeOverrideSec > 0 ? TimeOverrideSec : CurrentRound.timeSec;
            TimeRemaining = time;
            RoundElapsed = 0;
            TimerRunning = false;
            Debug.Log($"[History] Round: {CurrentRound.name}, {CurrentRound.artifacts.Count} artifacts, {time}s");
            SetState(GameState.Briefing);
        }

        public int GetRoundTimeSec()
        {
            return TimeOverrideSec > 0 ? TimeOverrideSec : (CurrentRound != null ? CurrentRound.timeSec : 60);
        }

        public void BeginExamination()
        {
            TimerRunning = true;
            RoundElapsed = 0;
            PrepareArtifact();
            SetState(GameState.Examine);
        }

        void PrepareArtifact()
        {
            RevealedTraits.Clear();
            UsedTools.Clear();
            ExaminedZones.Clear();
            ChosenHall = null;
            ChosenWall = null;
            // Material always visible from visual inspection
            if (CurrentArtifact != null)
                RevealedTraits["material"] = true;
        }

        public void ExamineZone(string zoneId)
        {
            if (CurrentArtifact == null) return;
            var zone = CurrentArtifact.zones.Find(z => z.id == zoneId);
            if (zone == null) return;
            ExaminedZones.Add(zoneId);
            foreach (var trait in zone.revealsTraits)
            {
                if (!RevealedTraits.ContainsKey(trait) || !RevealedTraits[trait])
                {
                    RevealedTraits[trait] = true;
                    string val = CurrentArtifact.traits.GetValue(trait);
                    OnTraitRevealed?.Invoke(trait, val);
                    Debug.Log($"[History] Zone '{zone.name}' -> {trait} = {val}");
                }
            }
            OnZoneExamined?.Invoke(zone);
        }

        public void ApplyTool(string toolId)
        {
            if (CurrentArtifact == null) return;
            var tool = Config.tools.Find(t => t.id == toolId);
            if (tool == null) return;
            if (UsedTools.Contains(toolId)) return;
            UsedTools.Add(toolId);
            RevealedTraits[tool.reveals] = true;
            string val = CurrentArtifact.traits.GetValue(tool.reveals);
            OnTraitRevealed?.Invoke(tool.reveals, val);
            Debug.Log($"[History] Tool '{tool.name}' -> {tool.reveals} = {val}");
        }

        public void OpenReference() => SetState(GameState.Reference);
        public void CloseReference() => SetState(GameState.Examine);
        public void StartPlacement() => SetState(GameState.ChooseHall);

        public void SelectHall(string hallId)
        {
            ChosenHall = hallId;
            SetState(GameState.ChooseWall);
        }

        public void SelectWall(string wallId)
        {
            ChosenWall = wallId;
            SetState(GameState.Confirm);
        }

        public void CancelPlacement() => SetState(GameState.ChooseWall);
        public void BackToHalls() => SetState(GameState.ChooseHall);
        public void BackToExamine() => SetState(GameState.Examine);

        public void ConfirmPlacement()
        {
            if (CurrentArtifact == null) return;
            bool hallOk = ChosenHall == CurrentArtifact.correctHall;
            bool wallOk = ChosenWall == CurrentArtifact.correctWall;
            int pts = 0;
            if (hallOk) pts += Config.scoring.correctHall;
            if (wallOk) pts += Config.scoring.correctWall;
            if (hallOk && wallOk) pts += Config.scoring.bothCorrectBonus;

            Placements.Add(new PlacementResult
            {
                artifact = CurrentArtifact,
                chosenHall = ChosenHall,
                chosenWall = ChosenWall,
                hallCorrect = hallOk,
                wallCorrect = wallOk,
                points = pts,
                skipped = false
            });
            Debug.Log($"[History] Placed '{CurrentArtifact.name}' -> {ChosenHall}/{ChosenWall} Hall:{hallOk} Wall:{wallOk} +{pts}");
            SetState(GameState.Placed);
        }

        public void SkipArtifact()
        {
            if (CurrentArtifact == null) return;
            Placements.Add(new PlacementResult { artifact = CurrentArtifact, skipped = true });
            Debug.Log($"[History] Skipped '{CurrentArtifact.name}'");
            SetState(GameState.Placed);
        }

        public void NextArtifact()
        {
            CurrentArtifactIndex++;
            if (CurrentArtifactIndex >= ArtifactTotal)
            {
                TimerRunning = false;
                ShowResults();
                return;
            }
            PrepareArtifact();
            SetState(GameState.Examine);
        }

        public void ShowResults()
        {
            TimerRunning = false;
            SetState(GameState.Results);
        }

        public int GetTotalScore()
        {
            int total = Placements.Sum(p => p.points);
            int placed = Placements.Count(p => !p.skipped);
            if (placed == ArtifactTotal)
                total += Config.scoring.allPlacedBonus;
            if (Placements.All(p => !p.skipped && p.hallCorrect && p.wallCorrect))
                total += Config.scoring.allCorrectBonus;
            float elapsed = CurrentRound.timeSec - TimeRemaining;
            if (elapsed < Config.scoring.speedThresholdSec && placed == ArtifactTotal)
                total += Config.scoring.speedBonus;
            return total;
        }

        public string GetRank(int score)
        {
            if (score >= 800) return "Директор музея";
            if (score >= 500) return "Ст. хранитель";
            if (score >= 300) return "Хранитель";
            if (score >= 100) return "Стажёр";
            return "Практикант";
        }

        public string GetRankStars(int score)
        {
            if (score >= 800) return "★★★★";
            if (score >= 500) return "★★★";
            if (score >= 300) return "★★";
            if (score >= 100) return "★";
            return "";
        }

        public void ReturnToMenu() => SetState(GameState.Menu);

        // Helpers
        public HallData GetHall(string id) => Config.halls.Find(h => h.id == id);
        public WallData GetWall(string id) => Config.walls.Find(w => w.id == id);
        public int GetPlacedInHall(string hallId) => Placements.Count(p => !p.skipped && p.chosenHall == hallId);
        public int GetPlacedOnWall(string hallId, string wallId) =>
            Placements.Count(p => !p.skipped && p.chosenHall == hallId && p.chosenWall == wallId);

        public string GetNotebookSummary()
        {
            if (CurrentArtifact == null) return "";
            var parts = new List<string>();
            if (RevealedTraits.ContainsKey("material") && RevealedTraits["material"])
                parts.Add(CurrentArtifact.traits.material);
            if (RevealedTraits.ContainsKey("year") && RevealedTraits["year"])
                parts.Add("~" + CurrentArtifact.traits.year);
            if (RevealedTraits.ContainsKey("textLanguage") && RevealedTraits["textLanguage"]
                && !string.IsNullOrEmpty(CurrentArtifact.traits.textLanguage))
                parts.Add(CurrentArtifact.traits.textLanguage);
            return string.Join(" · ", parts);
        }
    }
}
