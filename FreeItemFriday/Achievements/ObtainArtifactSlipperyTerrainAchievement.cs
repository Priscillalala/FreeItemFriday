using System;
using Unity;
using UnityEngine;
using GrooveSharedUtils;
using GrooveSharedUtils.Frames;
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.Interfaces;
using RoR2;
using RoR2.Items;
using static GrooveSharedUtils.Common;
using R2API;
using System.Collections;
using HG;
using RoR2.Achievements;
using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2.Achievements.Artifacts;

namespace FreeItemFriday.Achievements
{
    public class ObtainArtifactSlipperyTerrainAchievement : BaseObtainArtifactAchievement
    {
        public override ArtifactDef artifactDef => Assets.Artifacts.SlipperyTerrain;
    }
}

