using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2.Skills;
using UnityEngine;
using System;

namespace SummonerSurvivor
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(LanguageAPI), nameof(SoundAPI))]
    public class SummonerSurvivor : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Honkobol";
        public const string PluginName = "SummonerSurvivor";
        public const string PluginVersion = "0.0.3";

        //We need our item definition to persist through our functions, and therefore make it a class field.
        private static SkillDef skillDefSummon;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            // Add language tokens
            AddTokens();

            skillDefSummon = ScriptableObject.CreateInstance<SkillDef>();
            skillDefSummon.activationState = new SerializableEntityStateType(typeof(Skills.Summoner.BeginSummon));
            skillDefSummon.activationStateMachineName = "Body";
            skillDefSummon.baseMaxStock = 1;
            skillDefSummon.baseRechargeInterval = 0f;
            skillDefSummon.beginSkillCooldownOnSkillEnd = true;
            skillDefSummon.canceledFromSprinting = true;
            skillDefSummon.cancelSprintingOnActivation = true;
            skillDefSummon.fullRestockOnAssign = true;
            skillDefSummon.interruptPriority = InterruptPriority.Any;
            skillDefSummon.isCombatSkill = false;
            skillDefSummon.mustKeyPress = false;
            skillDefSummon.rechargeStock = 1;
            skillDefSummon.requiredStock = 1;
            skillDefSummon.stockToConsume = 1;
            skillDefSummon.icon = Resources.Load<Sprite>("NotAnActualPath");
            skillDefSummon.skillDescriptionToken = "SUMMONER_SKILLSLOT_SUMMON_DESCRIPTION";
            skillDefSummon.skillName = "SUMMONER_SKILLSLOT_SUMMON_NAME";
            skillDefSummon.skillNameToken = "SUMMONER_SKILLSLOT_SUMMON_NAME";

            // Load the skill
            LoadoutAPI.AddSkillDef(skillDefSummon);

            // Replace Huntress skill for test
            var huntress = RoR2.LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/HuntressBody");
            var skillLocator = huntress.GetComponent<RoR2.SkillLocator>();
            var skillFamily = skillLocator.special.skillFamily;
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDefSummon,
                unlockableName = "",
                viewableNode = new RoR2.ViewablesCatalog.Node(skillDefSummon.skillNameToken, false, null)

            };

            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        //This function adds the tokens from the item using LanguageAPI, the comments in here are a style guide, but is very opiniated. Make your own judgements!
        private void AddTokens()
        {
            LanguageAPI.Add("SUMMONER_SKILLSLOT_SUMMON_NAME", "Summon");
            LanguageAPI.Add("SUMMONER_SKILLSLOT_SUMMON_DESCRIPTION", "Summon a friendly monster");
        }

        //The Update() method is run on every frame of the game.
        private void Update()
        {
        }
    }
}
