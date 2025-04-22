using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class ChangeBodyTypeExtension : DefModExtension
    {
        public Gender? gender;

        public BodyTypeDef originalbodyTypeDef;

        public Vector2 drawSize;
        public Vector2 headdrawSize;

        public Vector2 drawSizeOriginal;
        public Vector2 headdrawSizeOriginal;

        public Vector2 drawSizeMale;
        public Vector2 headdrawSizeMale;

        public Vector2 drawSizeFemale;
        public Vector2 headdrawSizeFemale;

        public Vector2 drawSizeFat;
        public Vector2 headdrawSizeFat;

        public Vector2 drawSizeHulk;
        public Vector2 headdrawSizeHulk;

        public Vector2 drawSizeThin;
        public Vector2 headdrawSizeThin;

        public bool useVEFCoreScaler = true;
    }
}
