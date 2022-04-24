using EntityStates;
using RoR2;
using UnityEngine;

namespace SummonerSurvivor.Skills.Summoner
{
    class BeginSummon : BaseSkillState
    {

        private float blinkDuration = 0.3f; //The "distance" is predetermined from blinkVector, but this is the ""speed"" you blink at. The t in D*t=V
        private bool beginBlink = false;
        private float basePrepDuration = 0f; //Divided by attack speed to be how fast you start blinking. Kinda null for this ability, methinks
        public float jumpCoefficient = 2.5f; // SUPER FUNNY, only this modifies the distance you jump. Setting this to 1 doesn't move you at all -- regardless of blinkVector's size.
        private float prepDuration; //Decided later on, see the comment by basePrepDuration

        //IMPORTANT: THIS IS IN RELATION TO THE CAMERA, NOT THE DIRECTION THE CHARACTER FACES
        private Vector3 blinkVector = new Vector3(0, 0, 1f); // Literally don't touch it. Changing the first and second move you horizontally, and leave the third at 1f.
        
        public static GameObject blinkPrefab; //We'll need to make an effect for this to actually appear, BUT it exists and is refered to, so don't comment it out
        protected CameraTargetParams.AimRequest aimRequest; //This is used in determining where to teleport to, and what "up" constitutes.

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
            direction.y = 25f;
            direction.Normalize();
            Vector3 up = Vector3.up;
            worldBlinkVector = Matrix4x4.TRS(base.transform.position, Util.QuaternionSafeLookRotation(direction, up), new Vector3(1f, 1f, 1f)).MultiplyPoint3x4(blinkVector) - base.transform.position;
            worldBlinkVector.Normalize();
        }
        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(worldBlinkVector);
            effectData.origin = origin;
            EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
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
        public override void OnExit() //Purely the post-blink sfx, nothing particular to do with fixing the jump -Honk
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
            if ((bool)characterModel)
            {
                characterModel.invisibilityCount--;
            }
            if ((bool)hurtboxGroup)
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
