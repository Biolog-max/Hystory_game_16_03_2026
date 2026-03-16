using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace History
{
    public class CatalogDB
    {
        readonly List<ArtifactInfo> catalog;

        public CatalogDB(List<ArtifactInfo> catalogEntries)
        {
            catalog = catalogEntries;
        }

        public List<(ArtifactInfo entry, int matchPercent)> FindSimilar(
            ArtifactInfo artifact, Dictionary<string, bool> revealedTraits)
        {
            var results = new List<(ArtifactInfo, int)>();

            foreach (var entry in catalog)
            {
                int matches = 0;
                int total = 0;

                if (IsRevealed(revealedTraits, "material"))
                {
                    total++;
                    if (entry.traits.material == artifact.traits.material) matches++;
                }
                if (IsRevealed(revealedTraits, "weight"))
                {
                    total++;
                    float diff = Mathf.Abs(entry.traits.weight - artifact.traits.weight);
                    if (diff / Mathf.Max(entry.traits.weight, 1f) < 0.3f) matches++;
                }
                if (IsRevealed(revealedTraits, "size"))
                {
                    total++;
                    if (entry.traits.size == artifact.traits.size) matches++;
                }
                if (IsRevealed(revealedTraits, "year"))
                {
                    total++;
                    if (Mathf.Abs(entry.traits.year - artifact.traits.year) <= 20) matches++;
                }
                if (IsRevealed(revealedTraits, "textLanguage"))
                {
                    total++;
                    if (entry.traits.textLanguage == artifact.traits.textLanguage) matches++;
                }

                if (total == 0)
                {
                    total = 1;
                    if (entry.traits.material == artifact.traits.material) matches = 1;
                }

                int pct = (matches * 100) / total;
                results.Add((entry, pct));
            }

            return results.OrderByDescending(r => r.Item2).ToList();
        }

        static bool IsRevealed(Dictionary<string, bool> revealed, string key)
        {
            return revealed.ContainsKey(key) && revealed[key];
        }
    }
}
