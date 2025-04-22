using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_ThoughtBlockerRegistry
    {
        public static readonly HashSet<ThoughtDef> blockedDefs = new HashSet<ThoughtDef>();
        public static readonly HashSet<InspirationDef> blockedInspirations = new HashSet<InspirationDef>();
        public static readonly HashSet<MentalStateDef> blockedMentalStates = new HashSet<MentalStateDef>();

        [Unsaved]
        private static readonly HashSet<Pawn> sanitizedPawns = new HashSet<Pawn>();
        private static readonly FieldInfo MemoriesField = typeof(MemoryThoughtHandler).GetField("memories", BindingFlags.NonPublic | BindingFlags.Instance);
        [Unsaved]
        private static readonly Dictionary<Pawn, SanitizerHistory> sanitizerStateRegistry = new Dictionary<Pawn, SanitizerHistory>();
        [Unsaved]
        private static readonly HashSet<(Pawn, Thing)> appliedApparelTracker = new HashSet<(Pawn, Thing)>();
        [Unsaved]
        private static readonly HashSet<(Pawn, ThingDef)> usedConsumableTracker = new HashSet<(Pawn, ThingDef)>();

        public static bool DebugMode => LoadedModManager.GetMod<Mod_MIMWH40kFactions>()?.GetSettings<ModSettings_MIMWH40kFactions>()?.debugMode == true;

        public static void RegisterRemovedNeed(Pawn pawn, NeedDef def) => GetOrCreateHistory(pawn).removedNeeds.Add(def);
        public static void RegisterRemovedHediff(Pawn pawn, HediffDef def) => GetOrCreateHistory(pawn).removedHediffs.Add(def);
        public static void RegisterRemovedThought(Pawn pawn, ThoughtDef def) => GetOrCreateHistory(pawn).removedThoughts.Add(def);
        public static void RegisterBlockedInspiration(Pawn pawn, InspirationDef def) => GetOrCreateHistory(pawn).blockedInspirations.Add(def);
        public static void RegisterBlockedMentalState(Pawn pawn, MentalStateDef def) => GetOrCreateHistory(pawn).blockedMentalStates.Add(def);

        public static bool IsPawnSanitizable(Pawn pawn)
        {
            return pawn != null && !pawn.Destroyed && pawn.RaceProps?.Humanlike == true;
        }

        public static void ApplySanitizerRules(Pawn pawn, ISanitizerSource source)
        {
            if (!IsPawnSanitizable(pawn) || source == null)
            {
                return;
            }

            ApplyNeeds(pawn, source);
            ApplyHediffs(pawn, source);
            ApplyThoughts(pawn, source);
            ApplyInspirations(pawn, source);
            ApplyMentalStates(pawn, source);

            // Memory wipe
            if (source.wipeAllMemories)
                WipeAllMemories(pawn);

            // Prevent new thoughts
            if (source.preventNewThoughts && LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtBlocking)
                RegisterSanitizedPawn(pawn);

            // Inspiration & mental state reset
            if (source.removeInspiration)
            {
                pawn?.mindState?.inspirationHandler?.Reset();
            }

            if (source.removeMentalState && pawn.InMentalState)
            {
                if (pawn?.InMentalState == true)
                {
                    pawn.mindState?.mentalStateHandler?.Reset();
                }
            }

            // Addiction removal
            if (source.removeAllAddictions)
                RemoveAllAddictions(pawn);
        }

        public static void ApplySanitizerRulesSafe(Pawn pawn, ISanitizerSource source)
        {
            if (Current.ProgramState != ProgramState.Playing || pawn?.Spawned != true) return;

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ApplySanitizerRules(pawn, source);
            });
        }

        private static void ApplyNeeds(Pawn pawn, ISanitizerSource source)
        {
            if (source.needsToRemove == null)
            {
                return;
            }

            foreach (var needDefName in source.needsToRemove)
            {
                var def = DefDatabase<NeedDef>.GetNamedSilentFail(needDefName);
                if (def != null && pawn.needs.TryGetNeed(def) != null)
                {
                    RegisterRemovedNeed(pawn, def);
                    pawn.needs.AllNeeds.RemoveAll(n => n.def == def);
                }
            }
        }

        private static void ApplyHediffs(Pawn pawn, ISanitizerSource source)
        {
            if (source.hediffsToRemove == null)
            {
                return;
            }

            foreach (var hediffDefName in source.hediffsToRemove)
            {
                var def = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
                if (def != null && pawn.health.hediffSet.HasHediff(def))
                {
                    RegisterRemovedHediff(pawn, def);
                    pawn.health.hediffSet.hediffs.RemoveAll(h => h.def == def);
                }
            }
        }

        private static void ApplyThoughts(Pawn pawn, ISanitizerSource source)
        {
            if (source.thoughtsToRemove == null)
            {
                return;
            }

            foreach (var thoughtDefName in source.thoughtsToRemove)
            {
                var def = DefDatabase<ThoughtDef>.GetNamedSilentFail(thoughtDefName);
                if (def != null)
                {
                    pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(def);
                    blockedDefs.Add(def);
                    RegisterRemovedThought(pawn, def);
                }
            }
        }

        private static void ApplyInspirations(Pawn pawn, ISanitizerSource source)
        {
            if (source.blockAllInspiration)
            {
                foreach (var def in DefDatabase<InspirationDef>.AllDefs)
                {
                    if (blockedInspirations.Add(def))
                        RegisterBlockedInspiration(pawn, def);
                }
            }
            else if (source.blockedInspirationDefs != null)
            {
                foreach (var defName in source.blockedInspirationDefs)
                {
                    var def = DefDatabase<InspirationDef>.GetNamedSilentFail(defName);
                    if (def != null && blockedInspirations.Add(def))
                        RegisterBlockedInspiration(pawn, def);
                }
            }
        }

        private static void ApplyMentalStates(Pawn pawn, ISanitizerSource source)
        {
            if (source.blockAllMentalStates)
            {
                foreach (var def in DefDatabase<MentalStateDef>.AllDefs)
                {
                    if (blockedMentalStates.Add(def))
                        RegisterBlockedMentalState(pawn, def);
                }
            }
            else if (source.blockedMentalStateDefs != null)
            {
                foreach (var defName in source.blockedMentalStateDefs)
                {
                    var def = DefDatabase<MentalStateDef>.GetNamedSilentFail(defName);
                    if (def != null && blockedMentalStates.Add(def))
                        RegisterBlockedMentalState(pawn, def);
                }
            }
        }

        public static void WipeAllMemories(Pawn pawn, string limitsto = null)
        {
            if (pawn == null || pawn.needs?.mood?.thoughts?.memories == null || MemoriesField == null)
                return;

            var memoryHandler = pawn.needs?.mood?.thoughts?.memories;
            if (MemoriesField.GetValue(memoryHandler) is List<Thought_Memory> memoryList)
            {
                if (limitsto != null)
                {
                    foreach (var memory in memoryList.ToList())
                    {
                        if (memory?.def?.defName?.ToLowerInvariant().Contains(limitsto) == true)
                        {
                            RegisterRemovedThought(pawn, memory.def);
                            memoryList.Remove(memory);
                        }
                    }
                }
                else
                {
                    foreach (var memory in memoryList.ToList())
                    {
                        if (memory?.def != null)
                        {
                            RegisterRemovedThought(pawn, memory.def);
                        }
                    }
                    memoryList.Clear(); // ✅ Correctly placed after loop
                }
            }
        }

        private static void RemoveAllAddictions(Pawn pawn)
        {
            if (pawn == null || !IsPawnSanitizable(pawn))
            {
                return;
            }

            // Addiction needs
            if (pawn.needs?.AllNeeds != null)
            {
                foreach (var need in pawn.needs.AllNeeds.ToList())
                {
                    if (need.def?.defName?.ToLowerInvariant().Contains("addiction") == true)
                    {
                        RegisterRemovedNeed(pawn, need.def);
                        pawn.needs.AllNeeds.Remove(need);
                    }
                }
            }

            // Addiction hediffs
            if (pawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
                {
                    if (hediff.def?.defName?.ToLowerInvariant().Contains("addiction") == true)
                    {
                        RegisterRemovedHediff(pawn, hediff.def);
                        pawn.health.hediffSet.hediffs.Remove(hediff);
                    }
                }
            }

            // Addiction thoughts
            WipeAllMemories(pawn, "addiction");
        }

        public static void RegisterSanitizedPawn(Pawn pawn)
        {
            if (pawn != null && !pawn.Destroyed)
            {
                sanitizedPawns.Add(pawn);
            }
        }

        public static void UnregisterSanitizedPawn(Pawn pawn)
        {
            if (pawn != null)
            {
                sanitizedPawns.Remove(pawn);
            }
        }

        public static bool IsSanitized(Pawn pawn) => pawn != null && sanitizedPawns.Contains(pawn);

        public static void Clear()
        {
            sanitizedPawns.Clear();
        }

        public static void LoadGlobalBlocklist()
        {
            foreach (string defName in LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().globalBlockedThoughtDefs)
            {
                ThoughtDef def = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName);
                if (def != null)
                {
                    blockedDefs.Add(def);
                }
            }
        }

        public static bool IsBlocked(ThoughtDef def) => blockedDefs.Contains(def);
        public static bool IsBlocked(InspirationDef def) => blockedInspirations.Contains(def);
        public static bool IsBlocked(MentalStateDef def) => blockedMentalStates.Contains(def);


        //Restoration
        private static SanitizerHistory GetOrCreateHistory(Pawn pawn)
        {
            if (!sanitizerStateRegistry.TryGetValue(pawn, out var history))
            {
                history = new SanitizerHistory();
                sanitizerStateRegistry[pawn] = history;
            }
            return history;
        }

        public static void RegisterSanitizerSource(Pawn pawn, string source)
        {
            GetOrCreateHistory(pawn).sources.Add(source);
        }

        public static void RestoreSanitizerEffects(Pawn pawn)
        {
            if (pawn == null || pawn.Destroyed)
            {
                return;
            }

            if (!sanitizerStateRegistry.TryGetValue(pawn, out var history))
            {
                return;
            }

            RestoreNeeds(pawn, history);
            RestoreHediffs(pawn, history);
            RestoreInspirations(pawn, history);
            RestoreMentalStates(pawn, history);

            sanitizerStateRegistry.Remove(pawn);
            UnregisterSanitizedPawn(pawn);
        }

        public static void RestoreSanitizerEffectsFromSource(Pawn pawn, string source)
        {
            if (pawn == null || pawn.Destroyed || source == null)
            {
                return;
            }

            if (!sanitizerStateRegistry.TryGetValue(pawn, out var history) || !history.sources.Contains(source))
            {
                return;
            }

            RestoreNeeds(pawn, history);
            RestoreHediffs(pawn, history);
            RestoreInspirations(pawn, history);
            RestoreMentalStates(pawn, history);

            history.sources.Remove(source);

            if (history.sources.Count == 0)
            {
                sanitizerStateRegistry.Remove(pawn);
                UnregisterSanitizedPawn(pawn);
            }
        }

        private static void RestoreNeeds(Pawn pawn, SanitizerHistory history)
        {
            if (pawn.needs == null || pawn.needs?.AllNeeds == null)
            {
                return;
            }

            foreach (var def in history.removedNeeds)
            {
                if (pawn.needs != null && pawn.needs.AllNeeds.All(n => n.def != def))
                {
                    try
                    {
                        var newNeed = Activator.CreateInstance(def.needClass, pawn) as Need;
                        newNeed?.SetInitialLevel();
                        pawn.needs.AllNeeds.Add(newNeed);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"[Sanitizer] Failed to restore need {def.defName} on {pawn.NameShortColored}: {ex.Message}");
                    }
                }
            }
        }

        private static void RestoreHediffs(Pawn pawn, SanitizerHistory history)
        {
            if (pawn.health?.hediffSet == null)
            {
                return;
            }

            foreach (var def in history.removedHediffs)
            {
                if (!pawn.health.hediffSet.HasHediff(def))
                {
                    pawn.health.AddHediff(def);
                }
            }
        }

        private static void RestoreInspirations(Pawn pawn, SanitizerHistory history)
        {
            foreach (var def in history.blockedInspirations)
            {
                blockedInspirations.Remove(def);
            }
        }

        private static void RestoreMentalStates(Pawn pawn, SanitizerHistory history)
        {
            foreach (var def in history.blockedMentalStates)
            {
                blockedMentalStates.Remove(def);
            }
        }

        //Helper for Apparels
        public static bool HasAlreadyApplied(Pawn pawn, Thing apparel)
        {
            return appliedApparelTracker.Contains((pawn, apparel));
        }

        public static void RegisterApparelApplied(Pawn pawn, Thing apparel)
        {
            appliedApparelTracker.Add((pawn, apparel));
        }

        public static void UnregisterApparelApplied(Pawn pawn, Thing apparel)
        {
            appliedApparelTracker.Remove((pawn, apparel));
        }

        //Helper for Consumables
        public static bool HasUsedConsumable(Pawn pawn, ThingDef consumableDef)
        {
            return usedConsumableTracker.Contains((pawn, consumableDef));
        }

        public static void RegisterConsumableUse(Pawn pawn, ThingDef consumableDef)
        {
            usedConsumableTracker.Add((pawn, consumableDef));
        }

        public static void UnregisterConsumableUse(Pawn pawn, ThingDef consumableDef)
        {
            usedConsumableTracker.Remove((pawn, consumableDef));
        }
        public static void LogIfDebug(string message)
        {
            if (DebugMode)
            {
                Log.Warning("[Sanitizer] " + message);
            }
        }
    }
}