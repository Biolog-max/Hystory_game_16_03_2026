using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace History
{
    public static class DataLoader
    {
        public static ConfigData LoadConfig()
        {
            var text = Resources.Load<TextAsset>("HistoryData/config");
            if (text == null) { Debug.LogError("[History] config.json not found in Resources/HistoryData/"); return new ConfigData(); }
            return JsonConvert.DeserializeObject<ConfigData>(text.text);
        }

        public static List<ArtifactInfo> LoadCatalog()
        {
            var text = Resources.Load<TextAsset>("HistoryData/catalog");
            if (text == null) { Debug.LogError("[History] catalog.json not found"); return new List<ArtifactInfo>(); }
            return JsonConvert.DeserializeObject<List<ArtifactInfo>>(text.text);
        }

        public static RoundData LoadRound(string roundId)
        {
            var text = Resources.Load<TextAsset>("HistoryData/" + roundId);
            if (text == null) { Debug.LogError($"[History] {roundId}.json not found"); return new RoundData(); }
            return JsonConvert.DeserializeObject<RoundData>(text.text);
        }
    }
}
