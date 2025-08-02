using Verse;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;


namespace MIM40kFactions
{
    public class BodySnatcherExtension : DefModExtension
    {
        [MayRequireBiotech]
        public GeneDef requiredGene;
        [NoTranslate]
        public string pathBody = "";
        public bool usepathBodywithBodyDef = false;
        public bool invisibleHead = false;
        public bool invisibleNeck = true;
        public bool invisibleSkin = false;

        public BodyTypeDef bodyTypeDef;
        public List<GenderBodyTypeSet> genderBodyTypeSets = new List<GenderBodyTypeSet>();

        public HeadTypeDef headTypeDef;
        public List<GenderHeadTypeSet> genderHeadTypeSets = new List<GenderHeadTypeSet>();

        public Vector2 drawSize = new Vector2(1f, 1f);
        public Vector2 headdrawSize = new Vector2(1f, 1f);
        
        public bool useVEFCoreScaler = false;

        public bool noFaceTattoo = true;
        public bool noBodyTattoo = true;
        public bool noHair = true;
        public bool noBeard = true;

        public bool useClothing = true;

        public bool useGeneSkinColor = false;

        //For Animal Carry Mass
        public float animalCarryMassModifier = -1f;
    }
}
