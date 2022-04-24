using RoR2;
using UnityEngine;

namespace SummonerSurvivor.Modules
{
    class SummonerManager : MonoBehaviour
    {

        public CombatSquad summonerCombatSquad;

        public void Awake()
        {
            Log.LogInfo("Summoner Manager - Awake");
            summonerCombatSquad = gameObject.AddComponent<CombatSquad>();
        }
    }
}
