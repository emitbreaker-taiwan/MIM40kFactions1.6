using HarmonyLib;
using MIM40kFactions.Aeldari;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public class Dialog_ChoosePath : Window
    {
        private Pawn pawn;
        private Hediff_AsuryaniPath hediffPath;
        private List<AsuryaniPathTransitionEntry> availableTransitions = new List<AsuryaniPathTransitionEntry>();

        public override Vector2 InitialSize => new Vector2(700f, 500f);

        public Dialog_ChoosePath(Pawn pawn, Hediff_AsuryaniPath hediffPath)
        {
            this.pawn = pawn;
            this.hediffPath = hediffPath;

            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            forcePause = true;

            if (hediffPath.PathProps == null)
            {
                Log.Error($"[MIM Asuryani] HediffDef {hediffPath.def.defName} is missing AsuryaniPathExtension (PathProps). Dialog may not function correctly.");
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            availableTransitions = Utility_AsuryaniPath.GetAvailableTransitionEntries(pawn);

            if (availableTransitions.NullOrEmpty())
            {
                Messages.Message("MIM40k_EMAE_NoPathOptionsAvailable".Translate(pawn.Named("PAWN")), MessageTypeDefOf.RejectInput, historical: false);
                Close();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            float historyPaneWidth = 260f;
            Rect leftRect = new Rect(inRect.x, inRect.y, inRect.width - historyPaneWidth - 10f, inRect.height);
            Rect rightRect = new Rect(leftRect.xMax + 10f, inRect.y, historyPaneWidth, inRect.height);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(leftRect);

            listing.Label("EMAE_ChoosePathTitle".Translate(pawn.Named("PAWN")));
            listing.Gap(20f);

            listing.Label("EMAE_CurrentPathInfo".Translate(
                hediffPath.currentPathDef?.label ?? "N/A",
                hediffPath.CurrentImmersionLevel.ToString(),
                hediffPath.currentImmersionProgress.ToStringPercent()
            ));
            listing.Gap(10f);

            listing.Label("EMAE_ChoosePathDescription".Translate());
            listing.Gap(20f);

            if (availableTransitions.NullOrEmpty())
            {
                listing.Label("EMAE_NoPathOptionsAvailableFull".Translate());
            }
            else
            {
                var completedDefs = Utility_AsuryaniPath.GetAllPathData(pawn)
                    .Where(d => d.completed)
                    .Select(d => d.pathDef)
                    .ToHashSet();

                foreach (var entry in availableTransitions.OrderBy(e => e.toDef.label))
                {
                    var toDef = entry.toDef;

                    // 완료 여부 / 전환 가능 여부 확인
                    bool alreadyCompleted = completedDefs.Contains(toDef);
                    bool canTransition = Utility_AsuryaniPath.CanTransitionTo(pawn, entry);
                    bool canChoose = entry.MeetsRequirements(pawn) && canTransition && !alreadyCompleted;

                    string buttonLabel = toDef.label;
                    string tooltipText = $"<b>{toDef.label}</b>\n";

                    if (alreadyCompleted)
                        tooltipText += "• " + "EMAE_PathAlreadyCompleted".Translate() + "\n";
                    if (!canTransition)
                        tooltipText += "• " + "EMAE_PathTransitionNotAllowed".Translate() + "\n";

                    if (toDef.canBeExarch)
                        tooltipText += "• " + "EMAE_PathCanBeExarch".Translate() + "\n";

                    List<string> requirementStrings = entry.GetRequirementDescriptions(pawn);
                    if (!requirementStrings.NullOrEmpty())
                    {
                        tooltipText += string.Join("\n", requirementStrings);
                        buttonLabel += $" ({string.Join(", ", requirementStrings)})";
                    }

                    GUI.color = canChoose ? Color.white : Color.grey;
                    Rect rowRect = listing.GetRect(32f);
                    TooltipHandler.TipRegion(rowRect, tooltipText);

                    if (Widgets.ButtonText(rowRect, buttonLabel) && canChoose)
                    {
                        var data = pawn.GetOrCreatePathData(hediffPath.currentPathDef);
                        if (data != null)
                        {
                            data.RequestLeave();
                            data.nextPlannedPath = toDef;
                            hediffPath.CheckPathEvents();
                        }
                        Close();
                    }

                    GUI.color = Color.white;
                    listing.Gap(4f);
                }

                listing.Gap(12f);
                if (listing.ButtonText("Cancel".Translate()))
                {
                    Close();
                }
            }

            if (Prefs.DevMode)
            {
                listing.GapLine();
                listing.Label($"[Dev] Immersion Progress: {hediffPath.currentImmersionProgress:P0}");

                float newProgress = listing.Slider(hediffPath.currentImmersionProgress, 0f, 1f);
                if (!Mathf.Approximately(newProgress, hediffPath.currentImmersionProgress))
                {
                    hediffPath.currentImmersionProgress = newProgress;
                    Log.Message($"[MIM Asuryani] Dev: Set immersion progress to {newProgress:P2}");
                }

                listing.Gap(8f);
                listing.Label($"[Dev] Force Immersion Level:");

                if (listing.ButtonText("[Dev] Set to Novice"))
                {
                    hediffPath.SetLevelTo((int)ImmersionLevel.Novice);
                    Log.Message("[MIM Asuryani] Dev: Forced level to Novice.");
                }
                if (listing.ButtonText("[Dev] Set to Disciplined"))
                {
                    hediffPath.SetLevelTo((int)ImmersionLevel.Disciplined);
                    Log.Message("[MIM Asuryani] Dev: Forced level to Disciplined.");
                }
                if (listing.ButtonText("[Dev] Set to Consumed"))
                {
                    hediffPath.SetLevelTo((int)ImmersionLevel.Consumed);
                    Log.Message("[MIM Asuryani] Dev: Forced level to Consumed.");
                }
            }

            listing.End();

            DrawHistoryPane(rightRect);
        }

        private Vector2 historyScrollPos;

        private void DrawHistoryPane(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect innerRect = new Rect(0f, 0f, rect.width - 16f, 9999f); // 高さは後で自動調整

            Widgets.BeginScrollView(rect, ref historyScrollPos, innerRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(innerRect);

            Text.Font = GameFont.Medium;
            listing.Label("Path History");
            Text.Font = GameFont.Small;
            listing.Gap(10f);

            var history = Utility_AsuryaniPath.GetAllPathHistory(pawn);

            if (history.NullOrEmpty())
            {
                listing.Label("No previous Paths found.");
            }
            else
            {
                foreach (var path in history.OrderBy(p => p.ticksEntered))
                {
                    string label = path.pathDef?.label ?? "Unknown";
                    string status = path.completed ? "✓ Completed" : path.isLost ? "✕ Lost" : "→ Active";

                    string info = $"{label} ({status})\nEntered: {TicksToDaysStr(path.ticksEntered)}";

                    if (path.ticksExited > 0)
                    {
                        info += $"\nExited: {TicksToDaysStr(path.ticksExited)}";
                    }

                    listing.Label(info);
                    listing.GapLine();
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private string TicksToDaysStr(int ticks)
        {
            if (ticks < 0) return "N/A";
            int days = ticks / 60000;
            return $"Day {days}";
        }
    }
}
