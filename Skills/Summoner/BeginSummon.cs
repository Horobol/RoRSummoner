using EntityStates;
using RoR2;
using UnityEngine;

namespace SummonerSurvivor.Skills.Summoner
{
    class BeginSummon : BaseSkillState
    {

        private float blinkDuration = 0.3f;
        private bool beginBlink = false;
        private float basePrepDuration = 0.5f;
        public float jumpCoefficient = 25f;
        private float prepDuration;

        private Vector3 blinkVector = Vector3.zero; // Potentially fix

        protected CameraTargetParams.AimRequest aimRequest;

        private Transform modelTransform;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        private Vector3 worldBlinkVector;

        public override void OnEnter()
        {
            base.OnEnter();
            Log.LogInfo("Entering BeginSummon state");
            //Util.PlaySound(blinkSoundString, base.gameObject);
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            prepDuration = basePrepDuration / attackSpeedStat;
            base.PlayAnimation("FullBody, Override", "BeginArrowRain", "BeginArrowRain.playbackRate", prepDuration);
            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
            }
            if (base.cameraTargetParams)
            {
                aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            Vector3 direction = GetAimRay().direction;
            direction.y = 0f;
            direction.Normalize();
            Vector3 up = Vector3.up;
            worldBlinkVector = Matrix4x4.TRS(base.transform.position, Util.QuaternionSafeLookRotation(direction, up), new Vector3(1f, 1f, 1f)).MultiplyPoint3x4(blinkVector) - base.transform.position;
            worldBlinkVector.Normalize();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Log.LogInfo("FixedUpdate BeginSummon state");
            if (base.fixedAge >= prepDuration && !beginBlink)
            {
                beginBlink = true;
                CreateBlinkEffect(base.transform.position);
                if (characterModel)
                {
                    characterModel.invisibilityCount++;
                }
                if (hurtboxGroup)
                {
                    hurtboxGroup.hurtBoxesDeactivatorCounter++;
                }
            }
            if (beginBlink && base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterMotor.rootMotion += worldBlinkVector * (base.characterBody.jumpPower * jumpCoefficient * Time.fixedDeltaTime);
            }
            if (base.fixedAge >= blinkDuration + prepDuration && base.isAuthority)
            {
                outer.SetNextState(InstantiateNextState());
            }
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(worldBlinkVector);
            effectData.origin = origin;
            //EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
        }

        public override void OnExit()
        {
            CreateBlinkEffect(base.transform.position);
            modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.6f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
                TemporaryOverlay temporaryOverlay2 = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay2.duration = 0.7f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
                temporaryOverlay2.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }
            if (hurtboxGroup)
            {
                hurtboxGroup.hurtBoxesDeactivatorCounter--;
            }
            aimRequest?.Dispose();
            Log.LogInfo("Exiting BeginSummon state");
            base.OnExit();
        }

        private EntityState InstantiateNextState()
        {
            return new Summon();
        }
    }
}
