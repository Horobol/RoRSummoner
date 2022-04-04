using RoR2;
using UnityEngine;

namespace SummonerSurvivor.Skills.Summoner.Actions
{
    class SpawnFinalizer
    {
        public CombatSquad CombatSquad { get; set; }

        public EliteIndex EliteIndex { get; set; }

        public CharacterBody summonerCharacterBody { get; set; }

        public void OnCardSpawned(SpawnCard.SpawnResult spawnResult)
        {
            if (!spawnResult.success)
            {
                return;
            }

            CharacterMaster monster = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            if (CombatSquad)
            {
                CombatSquad.AddMember(monster);
            }
            EliteDef eliteDef = EliteCatalog.GetEliteDef(EliteIndex);
            EquipmentIndex equipmentIndex = (eliteDef != null) ? eliteDef.eliteEquipmentDef.equipmentIndex : EquipmentIndex.None;
            if (equipmentIndex != EquipmentIndex.None)
            {
                monster.inventory.SetEquipmentIndex(equipmentIndex);
            }
            float healthBoostCoefficient = 1f;
            float damageBoostCoefficient = 1f;
            healthBoostCoefficient += Run.instance.difficultyCoefficient / 1.5f;
            damageBoostCoefficient += Run.instance.difficultyCoefficient / 15f;
            int numberOfPlayers = Mathf.Max(1, Run.instance.livingPlayerCount);
            healthBoostCoefficient *= Mathf.Pow(numberOfPlayers, 0.75f);
            monster.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt(Mathf.RoundToInt(healthBoostCoefficient - 1f) * 10f));
            monster.inventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt(Mathf.RoundToInt(damageBoostCoefficient - 1f) * 10f));

            //TODO: Replace this with something a little more cool/fitting for the character
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
            {
                origin = summonerCharacterBody.corePosition,
                rotation = Quaternion.identity,
                scale = summonerCharacterBody.bestFitRadius * 2f,
                color = new Color(255f, 228f, 181f)
            }, true);

        }
    }
}
