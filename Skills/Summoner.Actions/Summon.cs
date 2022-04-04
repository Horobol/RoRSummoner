using R2API.Utils;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SummonerSurvivor.Skills.Summoner.Actions;

namespace SummonerSurvivor.Skills.Summoner
{
    
    public class Summon : BaseSkillState
    {
        public struct PlacementInfo
        {
            public bool ok;

            public Vector3 position;

            public Quaternion rotation;
            public void Serialize(NetworkWriter writer)
            {
                writer.Write(ok);
                writer.Write(position);
                writer.Write(rotation);
            }

            public void Deserialize(NetworkReader reader)
            {
                ok = reader.ReadBoolean();
                position = reader.ReadVector3();
                rotation = reader.ReadQuaternion();
            }
        }

        public static GameObject areaIndicatorPrefab;
        private GameObject areaIndicatorInstance; //Same deal as the blink prefab in BeginSummon, invisible in gameplay but exists

        public static float maxPlacementDistance = 1000f; // Using huntress value, though it seems to be way further than we need.
        public static float normalYThreshold;

        private SummonBlueprintController summonBlueprints;

        private bool exitPending;
        private float entryCountdown;
        private float exitCountdown;

        private PlacementInfo currentPlacementInfo;

        public static float hoverVelocity = 0f; //This stuff is to affect how slow Summoner moves down while in the air.
        public static float hoverAcceleration = 100f;
        

        //private GenericSkill originalPrimarySkill; // Defines a way to remember the primary skill prior to replacing it with a summon

        public override void OnEnter()
        {
            base.OnEnter();
            Log.LogInfo("Entering Summon state");

            summonBlueprints = Object.Instantiate(areaIndicatorPrefab, currentPlacementInfo.position, currentPlacementInfo.rotation).GetComponent<SummonBlueprintController>();

            ChatMessage.SendColored("Entered Summonstate", "#ffffff");

            entryCountdown = 0f;
            exitCountdown = 0f;
            exitPending = false;

        }

        public static PlacementInfo GetPlacementInfo(Ray aimRay, GameObject gameObject)
        {
            float extraRaycastDistance = 0f;
            CameraRigController.ModifyAimRayIfApplicable(aimRay, gameObject, out extraRaycastDistance);
            Vector3 vector = -aimRay.direction;
            Vector3 vector2 = Vector3.up;
            Vector3 lhs = Vector3.Cross(vector2, vector);
            PlacementInfo result = default(PlacementInfo);
            result.ok = false;
            if (Physics.Raycast(aimRay, out var hitInfo, maxPlacementDistance, LayerIndex.world.mask) && hitInfo.normal.y > normalYThreshold)
            {
                vector2 = hitInfo.normal;
                vector = Vector3.Cross(lhs, vector2);
                result.ok = true;
            }
            result.rotation = Util.QuaternionSafeLookRotation(vector, vector2);
            Vector3 vector3 = (result.position = hitInfo.point);
            return result;
        }

        private void DestroySummonIndicator()
        {
            if ((bool)summonBlueprints)
            {
                EntityState.Destroy(summonBlueprints.gameObject);
                summonBlueprints = null;
            }
        }

        public override void Update()
        {
            base.Update();
            currentPlacementInfo = GetPlacementInfo(GetAimRay(), base.gameObject);
            if ((bool)areaIndicatorInstance)
            {
                summonBlueprints.PushState(currentPlacementInfo.position, currentPlacementInfo.rotation, currentPlacementInfo.ok);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority) //this is the sorta "hover in place while summoning" stuff
            {
                float y = base.characterMotor.velocity.y;
                y = Mathf.MoveTowards(y, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, y, base.characterMotor.velocity.z);
            }

            if (!base.isAuthority) //Here's the ACTUAL summon stuff. After m1 or m2 is pressed, exitPending is set to true and it exits the state next update
            {
                return;
            }
            entryCountdown -= Time.fixedDeltaTime;
            if (exitPending)
            {
                exitCountdown -= Time.fixedDeltaTime;
                if (exitCountdown <= 0f)
                {
                    outer.SetNextStateToMain();
                }
            }
            else
            {
                if (!base.inputBank || !(entryCountdown <= 0f))
                {
                    return;
                }
                if ((base.inputBank.skill1.down || base.inputBank.skill4.justPressed && currentPlacementInfo.ok))
                {
                    ChatMessage.SendColored("Summoned!", "#88cc99");
                    // Use Elder Lemurian for now
                    SpawnFriendlyMonster(Monsters.LemurianBruiser, currentPlacementInfo.position);
                    exitPending = true;
                }
                if (base.inputBank.skill2.justPressed)
                {
                    ChatMessage.SendColored("Summon Canceled!", "#cc8899");
                    DestroySummonIndicator();
                    exitPending = true;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Log.LogInfo("Exiting Summon state");
            //base.skillLocator.primary = originalPrimarySkill; // Resets the primary to its original skill
            DestroySummonIndicator();


            ChatMessage.SendColored("Properly Exited the Summonstate", "#ffffff");

            
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private void SpawnFriendlyMonster(string monster, Vector3 position)
        {
            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                position = position
            };

            SpawnCard card = LegacyResourcesAPI.Load<SpawnCard>(monster);
            card.directorCreditCost = 0;
            SpawnFinalizer finalizer = new SpawnFinalizer()
            {
                CombatSquad = null, //TODO: Add a global CombatGroup per Summoner for summon management
                EliteIndex = EliteIndex.None,
                summonerCharacterBody = base.characterBody
            };

            DirectorSpawnRequest request = new DirectorSpawnRequest(card, placementRule, RoR2Application.rng)
            {
                ignoreTeamMemberLimit = true,
                teamIndexOverride = TeamIndex.Player,
                onSpawnedServer = finalizer.OnCardSpawned,
                summonerBodyObject = base.gameObject
            };

            GameObject obj = DirectorCore.instance.TrySpawnObject(request);

            if (!obj)
            {
                // Fallback if monster is not summonable in given position?
                Debug.LogError("Couldn't spawn any monster!");
            } else
            {

            }
        }
    }

}
