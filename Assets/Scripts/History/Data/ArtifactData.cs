using System;
using System.Collections.Generic;

namespace History
{
    [Serializable]
    public class TraitSet
    {
        public string material = "";
        public float weight;
        public string size = "";
        public int year;
        public string textLanguage = "";

        public string GetValue(string traitId)
        {
            switch (traitId)
            {
                case "material": return material;
                case "weight": return weight > 0 ? weight + " г" : "";
                case "size": return size;
                case "year": return year > 0 ? "~" + year : "";
                case "textLanguage": return string.IsNullOrEmpty(textLanguage) ? "нет надписей" : textLanguage;
                default: return "";
            }
        }

        public static string TraitLabel(string traitId)
        {
            switch (traitId)
            {
                case "material": return "Материал";
                case "weight": return "Вес";
                case "size": return "Размер";
                case "year": return "Год";
                case "textLanguage": return "Язык текста";
                default: return traitId;
            }
        }
    }

    [Serializable]
    public class ZoneData
    {
        public string id;
        public string name;
        public string description;
        public List<string> revealsTraits = new List<string>();
    }

    [Serializable]
    public class ArtifactInfo
    {
        public string id;
        public string name;
        public TraitSet traits = new TraitSet();
        public List<ZoneData> zones = new List<ZoneData>();
        public string correctHall;
        public string correctWall;
        public string funFact = "";
    }

    [Serializable]
    public class HallData
    {
        public string id;
        public string name;
        public string description;
    }

    [Serializable]
    public class WallData
    {
        public string id;
        public string name;
        public string description;
    }

    [Serializable]
    public class ToolData
    {
        public string id;
        public string name;
        public string reveals;
        public string description;
    }

    [Serializable]
    public class ScoringData
    {
        public int correctHall = 50;
        public int correctWall = 50;
        public int bothCorrectBonus = 100;
        public int allPlacedBonus = 50;
        public int allCorrectBonus = 200;
        public int speedBonus = 30;
        public int speedThresholdSec = 180;
    }

    [Serializable]
    public class ConfigData
    {
        public List<HallData> halls = new List<HallData>();
        public List<WallData> walls = new List<WallData>();
        public List<ToolData> tools = new List<ToolData>();
        public ScoringData scoring = new ScoringData();
    }

    [Serializable]
    public class RoundData
    {
        public string id;
        public string name;
        public string description;
        public int timeSec = 300;
        public int difficulty = 1;
        public List<ArtifactInfo> artifacts = new List<ArtifactInfo>();
    }

    public class PlacementResult
    {
        public ArtifactInfo artifact;
        public string chosenHall;
        public string chosenWall;
        public bool hallCorrect;
        public bool wallCorrect;
        public int points;
        public bool skipped;
    }
}
