using EntityStates;

namespace SummonerSurvivor.Skills.Summoner
{
    public class Summon : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Log.LogInfo("Entering Summon state");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Log.LogInfo("FixedUpdate Summon state");
        }

        public override void OnExit()
        {
            Log.LogInfo("Exiting Summon state");
            base.OnExit();
        }
    }
}
