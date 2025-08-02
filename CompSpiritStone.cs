using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class CompSpiritStone : ThingComp
    {
        public string pawnNameFull;
        public FactionDef originalFactionDef;
        public BackstoryDef adulthoodBackstory;
        public BackstoryDef childhoodBackstory;
        public List<TraitDef> traitDefs;
        public Dictionary<SkillDef, float> skillLevels;
        public List<HediffDef> hediffDefs;
        public string lastPathOrAspect;
        public List<AbilityDef> psycastsDefs;
        public List<StoredRelationInfo> relations;

        public bool alreadyHonored = false;

        private Pawn cachedOriginalPawn = null;

        public Pawn GetOriginalPawn() => cachedOriginalPawn;

        public CompSpiritStone()
        {
            traitDefs = new List<TraitDef>();
            skillLevels = new Dictionary<SkillDef, float>();
            hediffDefs = new List<HediffDef>();
            psycastsDefs = new List<AbilityDef>();
            relations = new List<StoredRelationInfo>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref pawnNameFull, "pawnNameFull");
            Scribe_Defs.Look(ref originalFactionDef, "originalFactionDef");

            Scribe_Defs.Look(ref adulthoodBackstory, "adulthoodBackstory");
            Scribe_Defs.Look(ref childhoodBackstory, "childhoodBackstory");

            Scribe_Collections.Look(ref traitDefs, "traitDefs", LookMode.Def);
            Scribe_Collections.Look(ref hediffDefs, "hediffDefs", LookMode.Def);
            Scribe_Collections.Look(ref psycastsDefs, "psychicPowerDefs", LookMode.Def);

            Scribe_Values.Look(ref lastPathOrAspect, "lastPathOrAspect");

            Scribe_Collections.Look(ref skillLevels, "skillLevels", LookMode.Def, LookMode.Value);

            Scribe_Collections.Look(ref relations, "relations", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && relations == null)
            {
                relations = new List<StoredRelationInfo>();
            }

            Scribe_Values.Look(ref alreadyHonored, "alreadyHonored", false);
        }

        public void StorePawnData(Pawn deceasedPawn)
        {
            cachedOriginalPawn = deceasedPawn;
            pawnNameFull = deceasedPawn.Name?.ToStringFull ?? "EMAE_UnknownSoul".Translate();

            if (deceasedPawn.Faction != null)
                originalFactionDef = deceasedPawn.Faction.def;

            adulthoodBackstory = deceasedPawn.story?.Adulthood;
            childhoodBackstory = deceasedPawn.story?.Childhood;

            traitDefs = new List<TraitDef>();
            if (deceasedPawn.story?.traits?.allTraits != null)
            {
                foreach (var trait in deceasedPawn.story.traits.allTraits)
                    if (trait != null) traitDefs.Add(trait.def);
            }

            skillLevels = new Dictionary<SkillDef, float>();
            if (deceasedPawn.skills?.skills != null)
            {
                foreach (SkillRecord skill in deceasedPawn.skills.skills)
                    if (!skillLevels.ContainsKey(skill.def))
                        skillLevels.Add(skill.def, skill.levelInt);
            }

            hediffDefs = new List<HediffDef>();
            if (deceasedPawn.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in deceasedPawn.health.hediffSet.hediffs)
                    if (hediff != null) hediffDefs.Add(hediff.def);
            }

            psycastsDefs = new List<AbilityDef>();
            if (deceasedPawn.abilities?.abilities != null)
            {
                foreach (var psycast in deceasedPawn.abilities.abilities)
                    if (psycast.def.IsPsycast) psycastsDefs.Add(psycast.def);
            }

            relations = deceasedPawn.relations?.DirectRelations
                .Where(r => r.otherPawn != null)
                .Select(r => new StoredRelationInfo
                {
                    otherPawnName = r.otherPawn.Name?.ToStringFull ?? "Unknown",
                    otherPawnGender = r.otherPawn.gender,
                    otherPawnKindDefName = r.otherPawn.kindDef?.defName,
                    otherPawnFactionDef = r.otherPawn.Faction?.def,
                    relationDef = r.def
                }).ToList();
        }

        public override string CompInspectStringExtra()
        {
            // Using StringBuilder for more efficient string concatenation in multiple lines
            StringBuilder sb = new StringBuilder();

            if (pawnNameFull != null)
            {
                // Keyed string for "Contains the soul of: {0}"
                sb.Append("EMAE_SoulContains".Translate(pawnNameFull));

                if (originalFactionDef != null)
                {
                    // Keyed string for "\nOrigin: {0}"
                    sb.AppendLine(); // Add newline before appending new info
                    sb.Append("EMAE_SoulOrigin".Translate(originalFactionDef.label));
                }

                // Append other existing info, converted to keyed strings
                if (childhoodBackstory != null)
                {
                    sb.AppendLine();
                    sb.Append("EMAE_SoulChildhood".Translate(childhoodBackstory.titleShort));
                }
                if (adulthoodBackstory != null)
                {
                    sb.AppendLine();
                    sb.Append("EMAE_SoulAdulthood".Translate(adulthoodBackstory.titleShort));
                }
                if (skillLevels != null && skillLevels.Any())
                {
                    var primarySkill = skillLevels.OrderByDescending(kv => kv.Value).FirstOrDefault();
                    if (primarySkill.Key != null)
                    {
                        sb.AppendLine();
                        sb.Append("EMAE_SoulPrimarySkill".Translate(primarySkill.Key.label, primarySkill.Value));
                    }
                }

                return sb.ToString();
            }
            return base.CompInspectStringExtra();
        }
    }
}
