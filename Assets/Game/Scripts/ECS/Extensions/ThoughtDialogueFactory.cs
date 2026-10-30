using System.Collections.Generic;
using UnityEngine;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Extensions
{
    public static class ThoughtDialogueFactory
    {
        private const string Table = "Main";
        private const float HoldSeconds = 3f;

        private static readonly Dictionary<string, DialogueAsset> Cache = new Dictionary<string, DialogueAsset>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => Cache.Clear();

        public static DialogueAsset Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (Cache.TryGetValue(key, out DialogueAsset cached) && cached != null) return cached;

            DialogueAsset asset = ScriptableObject.CreateInstance<DialogueAsset>();
            asset.hideFlags = HideFlags.HideAndDontSave;
            asset.DefaultTable = Table;

            StartNode start = asset.AddNode(new StartNode());
            LineNode line = asset.AddNode(new LineNode { Key = key, Advance = LineAdvance.AfterText, Seconds = HoldSeconds });
            EndNode end = asset.AddNode(new EndNode());
            start.Next = line.Id;
            line.Next = end.Id;

            Cache[key] = asset;
            return asset;
        }
    }
}
