// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal sealed class CueSheetValidator : ICueSheetValidator
    {
        private readonly IReadOnlyList<ICueValidationRule>? _injectedCueRules;

        private readonly IReadOnlyList<ICueSheetValidationRule>? _injectedCueSheetRules;
        private readonly IReadOnlyList<ITrackValidationRule>? _injectedTrackRules;

        internal CueSheetValidator(
            IReadOnlyList<ICueSheetValidationRule>? cueSheetRules = null,
            IReadOnlyList<ICueValidationRule>? cueRules = null,
            IReadOnlyList<ITrackValidationRule>? trackRules = null)
        {
            _injectedCueSheetRules = cueSheetRules;
            _injectedCueRules = cueRules;
            _injectedTrackRules = trackRules;
        }

        public IReadOnlyList<ValidationIssue> Validate(CueSheetAsset asset,
            AudioConductorSettings? settings)
        {
            EnsureRulesLoaded();
            var activeCueSheetRules = _injectedCueSheetRules ?? RuleCache.CueSheetRules!;
            var activeCueRules = _injectedCueRules ?? RuleCache.CueRules!;
            var activeTrackRules = _injectedTrackRules ?? RuleCache.TrackRules!;

            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var ctx = new ValidationContext(assetGuid);

            foreach (var rule in activeCueSheetRules)
                rule.Validate(asset.cueSheet, ctx);

            foreach (var cue in asset.cueSheet.cueList)
            {
                if (cue is null)
                {
                    ctx.SetCurrentCue(null);
                    ctx.AddError("Cue.StructureInvalid", "A cue entry is missing or corrupted.");
                    continue;
                }

                ctx.SetCurrentCue(cue.Id);

                foreach (var rule in activeCueRules)
                    rule.Validate(cue, asset.cueSheet, settings, ctx);

                foreach (var track in cue.trackList)
                {
                    if (track is null)
                    {
                        ctx.AddError("Track.StructureInvalid", "A track entry is missing or corrupted.");
                        continue;
                    }

                    foreach (var rule in activeTrackRules)
                        rule.Validate(track, cue, ctx);
                }
            }

            return ctx.Issues;
        }

        private void EnsureRulesLoaded()
        {
            if (_injectedCueSheetRules is null && RuleCache.CueSheetRules is null)
                RuleCache.CueSheetRules = Collect<ICueSheetValidationRule>();
            if (_injectedCueRules is null && RuleCache.CueRules is null)
                RuleCache.CueRules = Collect<ICueValidationRule>();
            if (_injectedTrackRules is null && RuleCache.TrackRules is null)
                RuleCache.TrackRules = Collect<ITrackValidationRule>();
        }

        private static IReadOnlyList<T> Collect<T>()
        {
            return TypeCache.GetTypesDerivedFrom<T>()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.GetConstructor(Type.EmptyTypes) is not null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .Select(t => (T)Activator.CreateInstance(t)!)
                .ToList();
        }

        private static class RuleCache
        {
            internal static IReadOnlyList<ICueSheetValidationRule>? CueSheetRules;
            internal static IReadOnlyList<ICueValidationRule>? CueRules;
            internal static IReadOnlyList<ITrackValidationRule>? TrackRules;

            [InitializeOnLoadMethod]
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            internal static void Reset()
            {
                CueSheetRules = null;
                CueRules = null;
                TrackRules = null;
            }
        }
    }
}
