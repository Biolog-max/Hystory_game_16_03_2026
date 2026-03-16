using UnityEngine;
using UnityEditor;
using History;
using System.IO;

public class HistoryTestHelper
{
    [MenuItem("Test/History/Print State")]
    static void PrintState()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) { Debug.Log("[Test] GameManager not found"); return; }
        Debug.Log($"[Test] State={gm.State} Round={gm.CurrentRound?.name} " +
            $"Artifact={gm.CurrentArtifactIndex + 1}/{gm.ArtifactTotal} " +
            $"Placed={gm.PlacedCount} Time={gm.TimeRemaining:F0}s");
        if (gm.CurrentArtifact != null)
        {
            Debug.Log($"[Test] Current: {gm.CurrentArtifact.name} ({gm.CurrentArtifact.id})");
            Debug.Log($"[Test] Notebook: {gm.GetNotebookSummary()}");
            Debug.Log($"[Test] Revealed: {string.Join(", ", gm.RevealedTraits.Keys)}");
            Debug.Log($"[Test] UsedTools: {string.Join(", ", gm.UsedTools)}");
        }
    }

    [MenuItem("Test/History/New Round")]
    static void NewRound()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) { Debug.Log("[Test] GameManager not found"); return; }
        gm.StartNewRound();
    }

    [MenuItem("Test/History/Begin Examination")]
    static void Begin()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.BeginExamination();
    }

    [MenuItem("Test/History/Apply Ruler")]
    static void ApplyRuler() { Apply("ruler"); }

    [MenuItem("Test/History/Apply Carbon 14C")]
    static void ApplyCarbon() { Apply("carbon"); }

    [MenuItem("Test/History/Apply Scales")]
    static void ApplyScales() { Apply("scales"); }

    [MenuItem("Test/History/Apply Dictionary")]
    static void ApplyDictionary() { Apply("dictionary"); }

    static void Apply(string toolId)
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.ApplyTool(toolId);
    }

    [MenuItem("Test/History/Examine Zone 1")]
    static void Zone1()
    {
        var gm = HistoryGameManager.I;
        if (gm == null || gm.CurrentArtifact == null) return;
        if (gm.CurrentArtifact.zones.Count > 0)
            gm.ExamineZone(gm.CurrentArtifact.zones[0].id);
    }

    [MenuItem("Test/History/Examine Zone 2")]
    static void Zone2()
    {
        var gm = HistoryGameManager.I;
        if (gm == null || gm.CurrentArtifact == null) return;
        if (gm.CurrentArtifact.zones.Count > 1)
            gm.ExamineZone(gm.CurrentArtifact.zones[1].id);
    }

    [MenuItem("Test/History/Open Reference")]
    static void OpenRef()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.OpenReference();
    }

    [MenuItem("Test/History/Place Hall 1 (Russia)")]
    static void PlaceHall1()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.StartPlacement();
        gm.SelectHall("hall_russia_19");
    }

    [MenuItem("Test/History/Place Hall 2 (Europe)")]
    static void PlaceHall2()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.StartPlacement();
        gm.SelectHall("hall_europe_20");
    }

    [MenuItem("Test/History/Place Wall Weapons")]
    static void WallWeapons()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.SelectWall("weapons");
    }

    [MenuItem("Test/History/Place Wall Household")]
    static void WallHousehold()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.SelectWall("household");
    }

    [MenuItem("Test/History/Place Wall Documents")]
    static void WallDocuments()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.SelectWall("documents");
    }

    [MenuItem("Test/History/Confirm Placement")]
    static void Confirm()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.ConfirmPlacement();
    }

    [MenuItem("Test/History/Skip Artifact")]
    static void Skip()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        gm.SkipArtifact();
    }

    [MenuItem("Test/History/Force Timer End")]
    static void ForceTimer()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) return;
        // Set timer to 0 to trigger TimeUp
        var field = typeof(HistoryGameManager).GetProperty("TimeRemaining");
        // Use reflection or just call ShowResults
        gm.ShowResults();
    }

    [MenuItem("Test/History/Full Flow (Auto)")]
    static void FullFlow()
    {
        var gm = HistoryGameManager.I;
        if (gm == null) { Debug.Log("[Test] GameManager not found"); return; }
        gm.StartNewRound();
        gm.BeginExamination();
        Debug.Log("[Test] === FULL FLOW START ===");
        for (int i = 0; i < gm.ArtifactTotal; i++)
        {
            var a = gm.CurrentArtifact;
            if (a == null) break;
            Debug.Log($"[Test] Artifact {i + 1}: {a.name} (correct: {a.correctHall}/{a.correctWall})");
            // Apply all tools
            foreach (var tool in gm.Config.tools)
                gm.ApplyTool(tool.id);
            // Examine all zones
            foreach (var zone in a.zones)
                gm.ExamineZone(zone.id);
            // Place correctly
            gm.StartPlacement();
            gm.SelectHall(a.correctHall);
            gm.SelectWall(a.correctWall);
            gm.ConfirmPlacement();
            // Simulate auto-next
            gm.NextArtifact();
        }
        int score = gm.GetTotalScore();
        Debug.Log($"[Test] === FULL FLOW END === Score: {score}, Rank: {gm.GetRank(score)} {gm.GetRankStars(score)}");
    }

    [MenuItem("Test/History/Screenshot (Full Screen)")]
    static void TakeScreenshot()
    {
        string dir = Path.Combine(Application.dataPath, "Screenshots");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, $"screen_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        ScreenCapture.CaptureScreenshot(path, 1);
        Debug.Log($"[Test] Screenshot saved: {path}");
    }

    [MenuItem("Test/History/Load Data Test")]
    static void LoadDataTest()
    {
        var config = DataLoader.LoadConfig();
        Debug.Log($"[Test] Config: {config.halls.Count} halls, {config.walls.Count} walls, {config.tools.Count} tools");
        foreach (var h in config.halls) Debug.Log($"  Hall: {h.id} = {h.name}");
        foreach (var w in config.walls) Debug.Log($"  Wall: {w.id} = {w.name}");
        foreach (var t in config.tools) Debug.Log($"  Tool: {t.id} = {t.name} -> {t.reveals}");

        var catalog = DataLoader.LoadCatalog();
        Debug.Log($"[Test] Catalog: {catalog.Count} entries");
        foreach (var e in catalog) Debug.Log($"  {e.id}: {e.name} ({e.traits.material}, {e.traits.year})");

        var round = DataLoader.LoadRound("round_01");
        Debug.Log($"[Test] Round: {round.name}, {round.artifacts.Count} artifacts, {round.timeSec}s");
        foreach (var a in round.artifacts)
            Debug.Log($"  {a.id}: {a.name} -> {a.correctHall}/{a.correctWall}, zones={a.zones.Count}");
    }
}
