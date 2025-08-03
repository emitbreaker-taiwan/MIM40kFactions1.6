using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class Hediff_AsuryaniPath : Hediff_Level
    {
        public AsuryaniPathDef currentPathDef;

        public AsuryaniPathExtension PathProps => def.GetModExtension<AsuryaniPathExtension>();

        public bool debugMode => PathProps.debugMode;

        public Dictionary<AsuryaniPathDef, AsuryaniPathData> pathStates = new Dictionary<AsuryaniPathDef, AsuryaniPathData>();

        public float currentImmersionProgress = 0f;

        private bool hasTriggeredConsumedPrompt = false;

        public ImmersionLevel CurrentImmersionLevel
        {
            get
            {
                if (PathProps == null) return ImmersionLevel.Novice;

                if (currentImmersionProgress >= PathProps.scoreToConsumed)
                {
                    return ImmersionLevel.Consumed;
                }
                else if (currentImmersionProgress >= PathProps.scoreToDisciplined)
                {
                    return ImmersionLevel.Disciplined;
                }
                else
                {
                    return ImmersionLevel.Novice;
                }
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (currentPathDef != null) return;

            var startingPath = Utility_AsuryaniPath.FindBestStartingPath(pawn);
            if (startingPath != null)
            {
                ChangePath(startingPath, null, null);
            }
            else
            {
                if (debugMode)
                {
                    Log.Warning($"[MIM Asuryani] No valid starting path found for {pawn.NameShortColored}, assigning fallback path.");
                }

                var fallbackPath = DefDatabase<AsuryaniPathDef>.GetNamedSilentFail("EMAE_PathOfTheOutcast");

                if (fallbackPath != null)
                {
                    ChangePath(fallbackPath, null, null);
                }
                else if (debugMode)
                {
                    Log.Error($"[MIM Asuryani] Fallback path 'EMAE_PathOfTheOutcast' not found.");
                }
            }
        }

        public override void TickInterval(int delta)
        {
            if (!Utility_AsuryaniPath.IsAeldariCoreActive())
            {
                return;
            }

            base.TickInterval(delta);

            if (pawn.IsHashIntervalTick(60000, delta))
            {
                if (PathProps == null)
                {
                    Log.ErrorOnce($"[MIM Asuryani] HediffDef {def.defName} is missing AsuryaniPathExtension (PathProps). Cannot update Path. This will only be logged once per def.", def.GetHashCode());
                    return;
                }
                if (currentPathDef == null) // Path가 초기화되지 않은 경우 방지
                {
                    Log.Warning($"[MIM Asuryani] Pawn {pawn.LabelCap} has Hediff_AsuryaniPath but currentPathDef is null. Initializing to Novice or removing.");
                    // TODO: 기본 Path (Novice)로 강제 초기화하거나, Path 헤디프를 제거하는 로직 추가
                    // 예를 들어, ChangePath(DefDatabase<AsuryaniPathDef>.GetNamed("EMAE_Path_Novice"));
                    return;
                }

                UpdateImmersionProgress();
            }
        }        

        private void UpdateImmersionProgress()
        {
            var data = pawn.GetOrCreatePathData(currentPathDef);
            if (data == null || currentPathDef.relevantRecords.NullOrEmpty()) return;

            float gain = 0f;

            foreach (var entry in currentPathDef.relevantRecords)
            {
                float currentRecordValue = pawn.records.GetValue(entry.record);
                float prevRecordValue = data.lastRecordCounts.TryGetValue(entry.record, out var stored) ? stored : 0f;
                float delta = currentRecordValue - prevRecordValue;

                if (delta > 0f)
                {
                    gain += delta * entry.gainPerCount;
                    data.lastRecordCounts[entry.record] = currentRecordValue;
                }
            }

            if (gain > 0f)
            {
                currentImmersionProgress = Mathf.Clamp01(currentImmersionProgress + gain);

                if (debugMode)
                {
                    Log.Message($"[MIM Asuryani] {pawn.NameShortColored} gained {gain:0.000} progress in {currentPathDef.label} (new total: {currentImmersionProgress:0.000})");
                }
            }

            CheckLevelUp();

            if (!hasTriggeredConsumedPrompt && CurrentImmersionLevel == ImmersionLevel.Consumed)
            {
                hasTriggeredConsumedPrompt = true;

                if (pawn.IsColonistPlayerControlled)
                {
                    Utility_AsuryaniPath.ShowConsumedChoiceDialog(pawn, this);
                }
                else
                {
                    CheckPathEvents();
                }
            }

            if (pawn.CanBecomeExarch())
            {
                data.isExarch = true;
                currentPathDef?.levelEffects_Exarch?.ApplyEffects(pawn);

                if (pawn.IsColonistPlayerControlled)
                {
                    bool isFarseer = currentPathDef.category == AsuryaniPathCategory.Psyker;

                    string key = isFarseer ? "EMAE_FarseerPromotion_Message" : "EMAE_ExarchPromotion_Message";

                    Messages.Message(
                        key.Translate(pawn.Named("PAWN"), currentPathDef.label),
                        pawn,
                        MessageTypeDefOf.PositiveEvent
                    );
                }

                if (debugMode)
                {
                    Log.Message($"[MIM Asuryani] {pawn.NameShortColored} has become an Exarch of {currentPathDef?.label}.");
                }
            }
        }

        private void CheckLevelUp()
        {
            ImmersionLevel oldCalculatedLevel = CurrentImmersionLevel;

            ImmersionLevel targetLevel = ImmersionLevel.Novice;
            if (currentImmersionProgress >= PathProps.scoreToConsumed)
            {
                targetLevel = ImmersionLevel.Consumed;
            }
            else if (currentImmersionProgress >= PathProps.scoreToDisciplined)
            {
                targetLevel = ImmersionLevel.Disciplined;
            }
            else
            {
                targetLevel = ImmersionLevel.Novice;
            }

            if ((int)targetLevel != base.level)
            {
                SetLevelTo((int)targetLevel);

                if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored}'s Path level in {currentPathDef.label} changed from {oldCalculatedLevel} to {targetLevel}.");
                ApplyLevelEffects(targetLevel, oldCalculatedLevel);

                if (targetLevel == ImmersionLevel.Consumed && oldCalculatedLevel != ImmersionLevel.Consumed)
                {
                    if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} has reached Consumed level range ({currentImmersionProgress:P0}) for {currentPathDef.label}.");

                    float npcConsumedChance = 0.5f;
                    if (currentPathDef.canBeExarch)
                    {
                        npcConsumedChance += 0.2f; // 예시
                    }

                    if (Rand.Chance(npcConsumedChance))
                    {
                        if (debugMode) Log.Message($"[MIM Asuryani] NPC {pawn.NameShortColored} entered Consumed Path: {currentPathDef.label}.");

                        if (currentPathDef.canBeExarch)
                        {
                            var data = pawn.GetOrCreatePathData(currentPathDef);
                            if (data != null) data.isExarch = true;

                            if (debugMode) Log.Message($"[MIM Asuryani] NPC {pawn.NameShortColored} automatically becomes an Exarch!");
                        }
                    }
                    else
                    {
                        currentImmersionProgress = PathProps.scoreToConsumed - 0.001f;
                        SetLevelTo((int)ImmersionLevel.Disciplined);
                        ApplyLevelEffects(ImmersionLevel.Disciplined, ImmersionLevel.Consumed);

                        var data = pawn.GetOrCreatePathData(currentPathDef);
                        if (data != null) data.MarkComplete();

                        Utility_AsuryaniPath.GivePathTransitionThought(pawn, this.currentImmersionProgress, PathProps);
                        var nextPathDef = Utility_AsuryaniPath.OfferNextPathChoice(pawn);
                        if (nextPathDef != null)
                        {
                            ChangePath(nextPathDef, MessageTypeDefOf.PositiveEvent);
                        }
                        else
                        {
                            EnterOutcastPath();
                        }
                        if (debugMode) Log.Message($"[MIM Asuryani] NPC {pawn.NameShortColored} ended Path {currentPathDef.label} at Disciplined level.");
                    }
                }
            }
            else if ((int)targetLevel != level)
            {
                SetLevelTo((int)targetLevel);

                if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored}'s Path level in {currentPathDef.label} changed from {oldCalculatedLevel} to {targetLevel}.");
                ApplyLevelEffects(targetLevel, oldCalculatedLevel);
            }
        }

        public void ApplyLevelEffects(ImmersionLevel newLevel, ImmersionLevel oldLevel)
        {
            // Remove old PathLevelEffects
            if (oldLevel == ImmersionLevel.Novice)
                currentPathDef.levelEffects_Novice?.RemoveEffects(pawn);
            else if (oldLevel == ImmersionLevel.Disciplined)
                currentPathDef.levelEffects_Disciplined?.RemoveEffects(pawn);
            else if (oldLevel == ImmersionLevel.Consumed)
            {
                var data = pawn.GetOrCreatePathData(currentPathDef);
                if (data != null && data.isExarch)
                    currentPathDef.levelEffects_Exarch?.RemoveEffects(pawn);
                else
                    currentPathDef.levelEffects_Consumed?.RemoveEffects(pawn);
            }

            // Apply new PathLevelEffects
            if (newLevel == ImmersionLevel.Novice)
                currentPathDef.levelEffects_Novice?.ApplyEffects(pawn);
            else if (newLevel == ImmersionLevel.Disciplined)
                currentPathDef.levelEffects_Disciplined?.ApplyEffects(pawn);
            else if (newLevel == ImmersionLevel.Consumed)
            {
                var data = pawn.GetOrCreatePathData(currentPathDef);
                if (data != null && data.isExarch)
                    currentPathDef.levelEffects_Exarch?.ApplyEffects(pawn);
                else
                    currentPathDef.levelEffects_Consumed?.ApplyEffects(pawn);
            }
        }

        public void CheckPathEvents()
        {
            var data = pawn.GetOrCreatePathData(currentPathDef);
            if (data == null) return;

            if (!data.requestedToLeave)
            {
                if (CurrentImmersionLevel == ImmersionLevel.Consumed)
                {
                    if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} is stuck in Consumed state of {currentPathDef.label} (no exit requested). Applying penalties.");
                    // TODO: 정기적인 페널티 처리
                }
                return;
            }

            data.CancelLeave();
            var nextPlanned = data.nextPlannedPath;

            if (CurrentImmersionLevel == ImmersionLevel.Disciplined &&
                currentImmersionProgress >= PathProps.scoreToDisciplined &&
                currentImmersionProgress < PathProps.scoreToConsumed)
            {
                if (Utility_AsuryaniPath.CanSafelyChangePath(pawn))
                {
                    if (pawn.CanBecomeExarch())
                    {
                        if (debugMode)
                            Log.Message($"[MIM Asuryani] {pawn.NameShortColored} is eligible for Exarch and will not auto-change path.");
                        return;
                    }

                    AllowPathChange(data);
                }
                else
                {
                    if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} tried to exit but is in unsafe condition.");
                    if (!pawn.InMentalState && PathProps.defaultMentalStateFailed != null)
                    {
                        if (ModsConfig.AnomalyActive && PathProps.defaultMentalStateFailedAnomaly != null)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(PathProps.defaultMentalStateFailedAnomaly);
                        }
                        else
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(PathProps.defaultMentalStateFailed);
                        }
                    }
                    Utility_AsuryaniPath.GivePathTransitionThought(pawn, currentImmersionProgress, PathProps);
                }
                return;
            }

            if (CurrentImmersionLevel == ImmersionLevel.Consumed)
            {
                if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} attempting to leave from Consumed state. Severe penalties will apply.");
                Utility_AsuryaniPath.GivePathTransitionThought(pawn, currentImmersionProgress, PathProps);

                if (Rand.Chance(PathProps.damnationPathChanceOnEarlyExit + 0.5f))
                {
                    EnterDamnationPath();
                }
                else if (Rand.Chance(PathProps.outcastPathChanceOnEarlyExit - 0.5f))
                {
                    EnterOutcastPath();
                }
                else
                {
                    pawn.health.RemoveHediff(this);
                    if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} lost the Path entirely while leaving Consumed state.");
                }
                return;
            }

            if (CurrentImmersionLevel == ImmersionLevel.Novice)
            {
                if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} attempting to leave from Novice state (early exit).");
                Utility_AsuryaniPath.GivePathTransitionThought(pawn, currentImmersionProgress, PathProps);

                if (Rand.Chance(PathProps.outcastPathChanceOnEarlyExit))
                {
                    EnterOutcastPath();
                }
                else if (Rand.Chance(PathProps.damnationPathChanceOnEarlyExit))
                {
                    EnterDamnationPath();
                }
                else
                {
                    pawn.health.RemoveHediff(this);
                    if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} prematurely left {currentPathDef.label} at Progress {currentImmersionProgress:0.00} and lost the path.");
                }
                return;
            }

            Utility_AsuryaniPath.GivePathTransitionThought(pawn, currentImmersionProgress, PathProps);
            if (debugMode)
            {
                Log.Message($"[MIM Asuryani] {pawn.NameShortColored} requested to leave {currentPathDef.label} from {CurrentImmersionLevel} at {currentImmersionProgress:P0} (not completed). No change.");
            }
            data.nextPlannedPath = null;
        }

        private void AllowPathChange(AsuryaniPathData completedPathData)
        {
            if (pawn.GetCurrentPathData()?.isExarch == true)
            {
                if (debugMode)
                    Log.Warning($"[MIM Asuryani] {pawn.NameShortColored} is an Exarch and cannot leave the path {currentPathDef.label}.");
                Messages.Message("EMAE_PathChangeDenied_Exarch".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput);
                return;
            }

            if (CurrentImmersionLevel != ImmersionLevel.Disciplined)
            {
                if (debugMode) Log.Warning($"[MIM Asuryani] Attempted to call AllowPathChange on {pawn.NameShortColored} at level {CurrentImmersionLevel}. Aborting.");
                return;
            }

            Utility_AsuryaniPath.GivePathTransitionThought(pawn, currentImmersionProgress, PathProps);

            completedPathData.CancelLeave();
            completedPathData.MarkComplete();

            if (pawn.IsColonistPlayerControlled)
            {
                Find.WindowStack.Add(new Dialog_ChoosePath(pawn, this));
                if (debugMode) Log.Message($"[MIM Asuryani] PC {pawn.NameShortColored} completed path {currentPathDef.label} and is choosing next path.");
            }
            else
            {
                var nextPathDef = Utility_AsuryaniPath.OfferNextPathChoice(pawn);
                if (nextPathDef != null)
                {
                    ChangePath(nextPathDef, MessageTypeDefOf.PositiveEvent);
                    if (debugMode) Log.Message($"[MIM Asuryani] NPC {pawn.NameShortColored} completed path {currentPathDef.label} and automatically changed to {nextPathDef.label}.");
                }
                else
                {
                    if (debugMode) Log.Warning($"[MIM Asuryani] NPC {pawn.NameShortColored} completed path {currentPathDef.label} but no next path could be offered. Forcing Outcast Path.");
                    EnterOutcastPath();
                }
            }
        }

        private void ChangePathInternal(AsuryaniPathDef newPathDef)
        {
            currentPathDef = newPathDef;

            var data = pawn.GetOrCreatePathData(newPathDef);

            currentImmersionProgress = 0f;
            level = 0;

            currentPathDef.levelEffects_Novice?.ApplyEffects(pawn);

            if (debugMode) Log.Message($"[MIM Asuryani] {pawn.NameShortColored} started new path {newPathDef.label}. Progress and Level reset to 0.");
        }

        public void ChangePath(AsuryaniPathDef newPathDef, MessageTypeDef msgType = null, string messageKey = null)
        {
            var oldPathDef = currentPathDef;

            if (oldPathDef != null && !pathStates.ContainsKey(oldPathDef))
            {
                var prevData = new AsuryaniPathData(oldPathDef);
                prevData.MarkComplete();
                pathStates[oldPathDef] = prevData;
            }
            else if (oldPathDef != null && pathStates.TryGetValue(oldPathDef, out var existing))
            {
                existing.MarkComplete();
            }

            oldPathDef?.levelEffects_Novice?.RemoveEffects(pawn);
            oldPathDef?.levelEffects_Disciplined?.RemoveEffects(pawn);
            oldPathDef?.levelEffects_Consumed?.RemoveEffects(pawn);

            ChangePathInternal(newPathDef);

            if (!pathStates.ContainsKey(newPathDef))
            {
                pathStates[newPathDef] = new AsuryaniPathData(newPathDef);
            }

            var message = messageKey != null
                ? messageKey.Translate(pawn.Named("PAWN"), newPathDef.label)
                : "EMAE_PathMessage_Begin".Translate(pawn.Named("PAWN"), newPathDef.label);

            Messages.Message(message, pawn, msgType ?? MessageTypeDefOf.PositiveEvent);

            if (debugMode)
            {
                Log.Message($"[MIM Asuryani] {pawn.NameShortColored} changed path from {oldPathDef?.label ?? "none"} to {newPathDef.label}.");
            }
        }

        public void EnterOutcastPath()
        {
            var outcastDef = GetSpecialPathDef("EMAE_Path_Outcast");
            if (outcastDef != null)
                ChangePath(outcastDef, MessageTypeDefOf.NegativeEvent, "EMAE_PathMessage_Outcast".Translate(pawn.Named("PAWN")));
        }

        public void EnterDamnationPath()
        {
            var outcastDef = GetSpecialPathDef("EMAE_Path_Damnation");
            if (outcastDef != null)
                ChangePath(outcastDef, MessageTypeDefOf.NegativeEvent, "EMAE_PathMessage_Damnation".Translate(pawn.Named("PAWN")));
        }

        private AsuryaniPathDef GetSpecialPathDef(string defName)
        {
            return DefDatabase<AsuryaniPathDef>.GetNamedSilentFail(defName);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Utility_AsuryaniPath.IsAeldariCoreActive())
            {
                yield break;
            }

            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (!pawn.IsColonistPlayerControlled) yield break;

            if (currentPathDef == null) yield break;
            var data = pawn.GetOrCreatePathData(currentPathDef);
            if (data == null) yield break;

            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/EMWH_Change", true),
                defaultLabel = "EMAE_CommandPathChangeLabel".Translate(),
                defaultDesc = "EMAE_CommandPathChangeDesc".Translate() + "\n\n" +
                              ((PathProps != null && currentImmersionProgress >= PathProps.scoreToConsumed - 0.001f)
                                  ? "EMAE_CommandPathChangeDesc_ReadyForCompletion".Translate()
                                  : "EMAE_CommandPathChangeDesc_EarlyExitWarning".Translate()),
                action = () =>
                {
                    if (debugMode)
                        Log.Message($"[MIM Asuryani] {pawn.NameShortColored} opened path change dialog.");

                    Find.WindowStack.Add(new Dialog_ChoosePath(pawn, this));
                },
                Disabled = false,
                disabledReason = null
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref currentPathDef, "currentPathDef");
            Scribe_Collections.Look(ref pathStates, "pathStates", LookMode.Def, LookMode.Deep);
            Scribe_Values.Look(ref currentImmersionProgress, "currentImmersionProgress", 0f);
        }

        public override string Label => $"{base.Label} ({CurrentImmersionLevel})";
    }
}
