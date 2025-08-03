using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public static class Utility_AsuryaniPath
    {
        public static bool IsAeldariCoreActive()
        {
            try
            {
                var type = Type.GetType("MIM40kFactions.Utility_DependencyManager, MIM40kFactions1.6");
                if (type == null) return false;

                var method = type.GetMethod("IsAeldariCoreActive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (method == null) return false;

                var result = method.Invoke(null, null);
                return result is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        public static Hediff_AsuryaniPath GetAsuryaniPathHediff(this Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            if (pawn == null || pawn.health.hediffSet == null)
            {
                Log.Warning("[MIM Asuryani] GetAsuryaniPathHediff: Pawn or its health is null.");
                return null;
            }

            return pawn.health.hediffSet.hediffs.OfType<Hediff_AsuryaniPath>().FirstOrDefault();
        }

        public static AsuryaniPathDef GetCurrentAsuryaniPath(this Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            if (GetAsuryaniPathHediff(pawn) == null)
                return null;

            return GetAsuryaniPathHediff(pawn).currentPathDef;
        }

        public static AsuryaniPathData GetCurrentPathData(this Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff == null || hediff.currentPathDef == null)
                return null;

            return hediff.pathStates.TryGetValue(hediff.currentPathDef, out var data) ? data : null;
        }

        public static AsuryaniPathData GetOrCreatePathData(this Pawn pawn, AsuryaniPathDef pathDef)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff == null) return null;

            if (!hediff.pathStates.TryGetValue(pathDef, out var data))
            {
                data = new AsuryaniPathData();
                hediff.pathStates[pathDef] = data;
            }
            return data;
        }

        public static bool CanSafelyChangePath(this Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return false;
            }

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff == null) return false;

            bool isConsumed = hediff.CurrentImmersionLevel == ImmersionLevel.Consumed;

            return !isConsumed &&
                   pawn.mindState?.mentalBreaker != null &&
                   !pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
        }

        public static void GivePathTransitionThought(this Pawn pawn, float immersionProgress, AsuryaniPathExtension modExtension)
        {
            if (!IsAeldariCoreActive())
            {
                return;
            }

            ThoughtDef thought = null;

            if (immersionProgress >= modExtension.scoreToConsumed)
                thought = modExtension?.consumedLeftThoughtDef;
            else if (immersionProgress >= modExtension.scoreToDisciplined)
                thought = modExtension?.disciplinedLeftThoughtDef;
            else
                thought = modExtension?.noviceLeftThoughtDef;

            if (thought != null && pawn.needs?.mood != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
            else if (modExtension?.debugMode == true)
            {
                Log.Warning($"[MIM Aeldari] GivePathTransitionThought: No thought defined for immersionProgress {immersionProgress} or modExtension is null.");
            }
        }

        public static List<AsuryaniPathTransitionEntry> GetAvailableTransitionEntries(Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff?.currentPathDef == null) return new List<AsuryaniPathTransitionEntry>();

            return hediff.currentPathDef.transitionOptions
                ?.Where(entry => entry != null && entry.toDef != null && entry.MeetsRequirements(pawn))
                .ToList()
                ?? new List<AsuryaniPathTransitionEntry>();
        }

        public static List<AsuryaniPathData> GetAllPathData(Pawn pawn)
        {
            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff == null) return new List<AsuryaniPathData>();

            return hediff.pathStates?.Values?.ToList() ?? new List<AsuryaniPathData>();
        }

        public static AsuryaniPathDef OfferNextPathChoice(Pawn pawn)
        {
            if (!IsAeldariCoreActive())
                return null;

            var completedDefs = GetAllPathData(pawn)
                .Where(data => data.completed)
                .Select(data => data.pathDef)
                .ToHashSet();

            List<AsuryaniPathTransitionEntry> eligibleOptions = GetAvailableTransitionEntries(pawn)
                .Where(entry =>
                    !completedDefs.Contains(entry.toDef) &&
                    CanTransitionTo(pawn, entry)
                ).ToList();

            if (eligibleOptions.NullOrEmpty())
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"[MIM Asuryani] No eligible path transitions found for {pawn.NameShortColored}");
                }
                return null;
            }

            return eligibleOptions.RandomElementByWeight(entry =>
            {
                return entry.weight > 0f ? entry.weight : entry.toDef.weight;
            })?.toDef;
        }

        public static bool CanBecomeExarch(this Pawn pawn)
        {
            if (!IsAeldariCoreActive()) return false;

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff?.currentPathDef == null) return false;

            var pathDef = hediff.currentPathDef;
            var data = pawn.GetCurrentPathData();
            if (data == null || data.isExarch) return false;

            var modExtension = pathDef.GetModExtension<AsuryaniPathExtension>();
            float exarchThreshold = modExtension?.scoreToExarch ?? 0.95f;

            return pathDef.canBeExarch &&
                   (hediff.CurrentImmersionLevel == ImmersionLevel.Disciplined || hediff.CurrentImmersionLevel == ImmersionLevel.Consumed) &&
                   hediff.currentImmersionProgress >= exarchThreshold &&
                   data.completed &&
                   !data.isExarch;
        }

        public static List<string> GetRequirementDescriptions(this AsuryaniPathTransitionEntry entry, Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            List<string> descriptions = new List<string>();

            if (!entry.requiredTraits.NullOrEmpty())
            {
                descriptions.Add("EMAE_RequiredTraits".Translate(entry.requiredTraits.Select(t => t.def.label).ToCommaList()));
            }

            if (!entry.requiredSkills.NullOrEmpty())
            {
                foreach (var skillReq in entry.requiredSkills)
                {
                    int currentLevel = pawn.skills?.GetSkill(skillReq.skill)?.Level ?? 0;
                    string skillText = "EMAE_RequiredSkillWithCurrent".Translate(
                        skillReq.skill.skillLabel.CapitalizeFirst(),
                        skillReq.minLevel,
                        currentLevel
                    );
                    descriptions.Add(skillText);
                }
            }

            // TODO: Backstory, Faction, etc if needed
            return descriptions;
        }

        public static void ShowConsumedChoiceDialog(Pawn pawn, Hediff_AsuryaniPath hediff)
        {
            if (!IsAeldariCoreActive())
            {
                return;
            }

            if (pawn == null || hediff?.PathProps == null || hediff.currentPathDef == null) return;

            string dialogTitle = "EMAE_PathMessage_ChoiceTitle".Translate(pawn.NameShortColored);
            string dialogText = "EMAE_PathMessage_ChoiceText".Translate(
                pawn.Named("PAWN"),
                hediff.currentPathDef.label,
                hediff.PathProps.scoreToConsumed.ToStringPercent(),
                hediff.currentImmersionProgress.ToStringPercent()
            );

            ImmersionLevel oldLevel = hediff.CurrentImmersionLevel;

            Action acceptActionInternal = delegate
            {
                hediff.SetLevelTo((int)ImmersionLevel.Consumed);
                hediff.ApplyLevelEffects(ImmersionLevel.Consumed, oldLevel);

                if (hediff.debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} chose to embrace Consumed Path: {hediff.currentPathDef.label}.");

                if (hediff.currentPathDef.canBeExarch)
                {
                    var data = pawn.GetOrCreatePathData(hediff.currentPathDef);
                    if (data != null) data.isExarch = true;

                    if (hediff.debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} automatically becomes an Exarch!");
                }
            };

            Action declineActionInternal = delegate
            {
                hediff.currentImmersionProgress = hediff.PathProps.scoreToConsumed - 0.001f;
                hediff.SetLevelTo((int)ImmersionLevel.Disciplined);
                hediff.ApplyLevelEffects(ImmersionLevel.Disciplined, oldLevel);

                var data = pawn.GetOrCreatePathData(hediff.currentPathDef);
                if (data != null) data.MarkComplete();

                GivePathTransitionThought(pawn, hediff.currentImmersionProgress, hediff.PathProps);

                var nextPathDef = OfferNextPathChoice(pawn);
                if (nextPathDef != null)
                {
                    hediff.ChangePath(nextPathDef, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    hediff.EnterOutcastPath();
                }

                if (hediff.debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} chose to end Path {hediff.currentPathDef.label} at Disciplined level.");
            };

            Find.WindowStack.Add(new Dialog_MessageBox(
                dialogText,
                "EMAE_PathMessage_ChoiceAccept".Translate(),
                acceptActionInternal,
                "EMAE_PathMessage_ChoiceDecline".Translate(),
                declineActionInternal,
                dialogTitle,
                false,
                null,
                null,
                WindowLayer.Dialog
            ));
        }

        public static List<AsuryaniPathData> GetAllPathHistory(Pawn pawn)
        {
            if (!IsAeldariCoreActive())
            {
                return null;
            }

            if (pawn == null) return new List<AsuryaniPathData>();

            var comp = pawn.health.hediffSet.hediffs
                .OfType<Hediff_AsuryaniPath>()
                .FirstOrDefault();

            if (comp == null) return new List<AsuryaniPathData>();

            return comp.pathStates?
                .Values?
                .OrderBy(p => p.ticksEntered)
                .ToList()
                ?? new List<AsuryaniPathData>();
        }

        public static AsuryaniPathDef FindBestStartingPath(Pawn pawn)
        {
            if (!IsAeldariCoreActive()) return null;

            var allStartingPaths = DefDatabase<AsuryaniPathDef>.AllDefsListForReading
                .Where(p => p.isStartingPath && p.entryConditions != null)
                .ToList();

            AsuryaniPathDef bestPath = null;
            int bestSkillLevel = -1;

            foreach (var path in allStartingPaths)
            {
                foreach (var entry in path.entryConditions)
                {
                    if (!entry.MeetsRequirements(pawn)) continue;

                    foreach (var req in entry.requiredSkills)
                    {
                        var skill = pawn.skills?.GetSkill(req.skill);
                        if (skill != null && skill.Level >= req.minLevel)
                        {
                            if (skill.Level > bestSkillLevel)
                            {
                                bestPath = path;
                                bestSkillLevel = skill.Level;
                            }
                            else if (skill.Level == bestSkillLevel)
                            {
                                if (bestPath != null)
                                {
                                    int oldMin = bestPath.entryConditions.SelectMany(e => e.requiredSkills).Max(s => s.minLevel);
                                    int newMin = path.entryConditions.SelectMany(e => e.requiredSkills).Max(s => s.minLevel);
                                    if (newMin > oldMin)
                                    {
                                        bestPath = path;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return bestPath;
        }

        public static bool CanTransitionTo(Pawn pawn, AsuryaniPathTransitionEntry option)
        {
            if (!IsAeldariCoreActive()) return false;

            var toDef = option.toDef;

            if (toDef == null || (toDef.isFemaleOnly && pawn.gender != Gender.Female) || (toDef.isMaleOnly && pawn.gender != Gender.Male))
                return false;

            if (!option.MeetsRequirements(pawn)) return false;

            if (!toDef.entryConditions.NullOrEmpty() &&
                !toDef.entryConditions.Any(e => e.MeetsRequirements(pawn)))
            {
                return false;
            }

            return true;
        }

        public static ImmersionLevel GetImmersionLevelFor(Pawn pawn, AsuryaniPathDef def)
        {
            if (pawn == null || def == null) return ImmersionLevel.Novice;

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff != null && hediff.currentPathDef == def)
                return hediff.CurrentImmersionLevel;

            var data = pawn.GetPathData(def);
            if (data == null) return ImmersionLevel.Novice;

            if (data.completed) return ImmersionLevel.Consumed;
            if (data.ticksEntered > 0) return ImmersionLevel.Disciplined;

            return ImmersionLevel.Novice;
        }

        public static AsuryaniPathData GetPathData(this Pawn pawn, AsuryaniPathDef def)
        {
            if (pawn == null || def == null) return null;

            var hediff = pawn.GetAsuryaniPathHediff();
            if (hediff?.pathStates == null) return null;

            hediff.pathStates.TryGetValue(def, out var data);
            return data;
        }
    }
}
