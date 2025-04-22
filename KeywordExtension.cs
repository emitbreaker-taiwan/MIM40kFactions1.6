using System.Collections.Generic;
using System.Xml;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class KeywordExtension : DefModExtension
    {
        public bool isVehicle = false;
        public bool isMonster = false;
        public bool isPsychic = false;
        public bool isPsyker = false;
        public bool isCharacter = false;
        public bool isAstartes = false;
        public bool isInfantry = false;
        public bool isWalker = false;
        public bool isLeader = false;
        public bool isFly = false;
        public bool isAircraft = false;
        public bool isChaos = false;
        public bool isDaemon = false;
        public bool isDestroyerCult = false;
        public List<string> keywords = new List<string>();

        public List<KeywordwithDegree> keywordwithDegrees = new List<KeywordwithDegree>();
    }

    public class KeywordwithDegree
    {
        public string keyword;

        public int? degree;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.FirstChild.Value);
                return;
            }

            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
            if (xmlRoot.HasChildNodes)
            {
                degree = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
            }
        }
    }
}
