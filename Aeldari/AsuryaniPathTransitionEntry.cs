using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class AsuryaniPathTransitionEntry
    {
        public AsuryaniPathDef toDef;
        public float weight = 1f;
        public List<TraitRequirement> requiredTraits = new List<TraitRequirement>();
        public List<TraitRequirement> disallowedTraits = new List<TraitRequirement>();
        public List<SkillRequirement> requiredSkills = new List<SkillRequirement>();
        public List<SkillRequirement> disallowedSkills = new List<SkillRequirement>();

        public bool MeetsRequirements(Pawn pawn)
        {
            if (pawn == null) return false;

            if (!requiredTraits.NullOrEmpty())
            {
                foreach (var req in requiredTraits)
                {
                    Trait trait = pawn.story.traits.GetTrait(req.def);
                    if (trait == null) return false;

                    if (req.degree.HasValue && trait.Degree != req.degree.Value)
                    {
                        return false;
                    }
                }
            }

            if (!disallowedTraits.NullOrEmpty())
            {
                foreach (var req in disallowedTraits)
                {
                    Trait trait = pawn.story.traits.GetTrait(req.def);
                    if (trait == null) continue;

                    if (!req.degree.HasValue || trait.Degree == req.degree.Value)
                    {
                        return false;
                    }
                }
            }

            if (!requiredSkills.NullOrEmpty())
            {
                foreach (var req in requiredSkills)
                {
                    var skill = pawn.skills?.GetSkill(req.skill);
                    if (skill == null || skill.Level < req.minLevel) return false;
                }
            }

            if (!disallowedSkills.NullOrEmpty())
            {
                foreach (var req in disallowedSkills)
                {
                    var skill = pawn.skills?.GetSkill(req.skill);
                    if (skill != null && skill.Level >= req.minLevel)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
