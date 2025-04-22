using System.Collections.Generic;
using System.Xml;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class NonEdibleIngredientExtension : DefModExtension
    {
        public bool isNonEdibleIngredient = true;
        public float nutritionValue = 0f;
    }
}
