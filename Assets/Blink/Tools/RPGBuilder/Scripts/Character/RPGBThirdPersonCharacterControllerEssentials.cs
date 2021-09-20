using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.Controller
{
    public class RPGBThirdPersonCharacterControllerEssentials : RPGBCharacterControllerEssentials
    {
        public RPGBThirdPersonController controller;

        private static readonly int moveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

        /*
        -- EVENT FUNCTIONS --
        */
        public override void MovementSpeedChange(float newSpeed)
        {
            controller.SetSpeed(newSpeed);
            //float speedMod = newSpeed / 5;
            //anim.SetFloat("MoveSpeedModifier", speedMod);
        }

        /*
        -- INIT --
        */
        public override void Awake()
        {
            anim = GetComponent<Animator>();
            controller = GetComponent<RPGBThirdPersonController>();

            charController = GetComponent<CharacterController>();
        }

        public override IEnumerator InitControllers()
        {
            yield return new WaitForFixedUpdate();
            controller._playerCamera.Rig.transform.localRotation = transform.localRotation;
            controller._playerCamera.InitCameraPosition(new Vector2(15, transform.eulerAngles.y));
            controller.SetControlRotation(new Vector2(15, transform.eulerAngles.y));
            SetCameraMouseLook(true);
            controllerIsReady = true;
        }

        /*
        -- DEATH --
        */
        public override void InitDeath()
        {
            anim.Rebind();
            anim.SetBool("Dead", true);
            SetCameraAiming(false);
            SetCameraMouseLook(false);
            playerIsDead = true;
        }

        public override void CancelDeath()
        {
            playerIsDead = false;
            anim.Rebind();
            anim.SetBool("Dead", false);
            SetCameraMouseLook(true);
        }


        /*
        -- GROUND LEAP FUNCTIONS --
        Ground leaps are mobility abilities. Configurable inside the editor under Combat > Abilities > Ability Type=Ground Leap
        They allow to quickly dash or leap to a certain ground location.
        */
        public override void InitGroundLeap()
        {
            isLeaping = true;
        }

        public override void EndGroundLeap()
        {
            isLeaping = false;
        }
        
        /*
        -- FLYING FUNCTIONS --
        */
        
        public override void InitFlying()
        {
            isFlying = true;
            controller.isFlying = true;
            anim.SetBool("isFlying", true);
        }

        public override void EndFlying()
        {
            isFlying = false;
            controller.isFlying = false;
            anim.SetBool("isFlying", false);
        }

        /*
        -- STAND TIME FUNCTIONS --
        Stand time is an optional mechanic for abilities. It allows to root the caster for a certain duration after using the ability.
        */
        public override void InitStandTime(float max)
        {
            standTimeActive = true;
            currentStandTimeDur = 0;
            maxStandTimeDur = max;

        }

        protected override void HandleStandTime()
        {
            currentStandTimeDur += Time.deltaTime;
            if (currentStandTimeDur >= maxStandTimeDur) ResetStandTime();
        }

        protected override void ResetStandTime()
        {
            standTimeActive = false;
            currentStandTimeDur = 0;
            maxStandTimeDur = 0;
        }

        /* KNOCKBACK FUNCTIONS
         */
        public bool knockbackActive;
        private Vector3 knockBackTarget;
        private float cachedKnockbackDistance;

        public override void InitKnockback(float knockbackDistance, Transform attacker)
        {
            knockbackDistance *= 5;
            cachedKnockbackDistance = knockbackDistance;
            knockBackTarget = (transform.position - attacker.position).normalized * knockbackDistance;
            knockbackActive = true;
        }

        protected override void HandleKnockback()
        {
            if (knockBackTarget.magnitude > (cachedKnockbackDistance * 0.15f))
            {
                controller.getCharController().Move(knockBackTarget * Time.deltaTime);
                knockBackTarget = Vector3.Lerp(knockBackTarget, Vector3.zero, 5 * Time.deltaTime);
            }
            else
            {
                ResetKnockback();
            }
        }

        protected override void ResetKnockback()
        {
            knockbackActive = false;
            cachedKnockbackDistance = 0;
            knockBackTarget = Vector3.zero;
        }
        
        /* MOTION FUNCTIONS
         */
        private float curMotionSpeed;
        public override void InitMotion(float motionDistance, Vector3 motionDirection, float motionSpeed, bool immune)
        {
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted) return;
            if (knockbackActive) return;
            cachedMotionSpeed = motionSpeed;
            curMotionSpeed = cachedMotionSpeed;
            cachedPositionBeforeMotion = transform.position;
            cachedMotionDistance = motionDistance;
            motionTarget = transform.TransformDirection(motionDirection) * motionDistance;
            CombatManager.playerCombatNode.isMotionImmune = immune;
            motionActive = true;
        }

        protected override void HandleMotion()
        {
            float distance = Vector3.Distance(cachedPositionBeforeMotion, transform.position);
            if (distance < cachedMotionDistance)
            {
                lastPosition = transform.position;
                controller.getCharController().Move(motionTarget * (Time.deltaTime * curMotionSpeed));
                
                if (IsInMotionWithoutProgress(0.05f))
                {
                    ResetMotion();
                    return;
                }
                
                if (!(distance < cachedMotionDistance * 0.75f)) return;
                curMotionSpeed = Mathf.Lerp(curMotionSpeed, 0, Time.deltaTime * 5f);
                if(curMotionSpeed < (cachedMotionSpeed * 0.2f))
                {
                    curMotionSpeed = cachedMotionSpeed * 0.2f;
                }
            }
            else
            {
                ResetMotion();
            }
        }

        public override bool IsInMotionWithoutProgress(float treshold)
        {
            float speed = (transform.position - lastPosition).magnitude;
            return speed > -treshold && speed < treshold;
        }

        protected override void ResetMotion()
        {
            motionActive = false;
            CombatManager.playerCombatNode.isMotionImmune = false;
            cachedMotionDistance = 0;
            motionTarget = Vector3.zero;
        }
        
        /* CAMERA AIMING
         * 
         */

        public bool isAimingTransition;
        protected override void SetCameraAiming(bool isAiming)
        {
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted &&
                !RPGBuilderUtilities.canActiveShapeshiftCameraAim(CombatManager.playerCombatNode)) return;
            controller.CameraSettings.isAiming = isAiming;
            controller.RotationSettings.UseControlRotation = isAiming;
            controller.RotationSettings.OrientRotationToMovement = !isAiming;
            isAimingTransition = true;
            
            anim.SetBool("isAiming", isAiming);
            
            if (isAiming)
                CrosshairDisplayManager.Instance.ShowCrosshair();
            else
                CrosshairDisplayManager.Instance.HideCrosshair();
        }

        /*
        -- CAST SLOWED FUNCTIONS --
        Cast slow is an optional mechanic for abilities. It allows the player to be temporarily slowed while
        casting an ability. I personally use it to increase the risk of certain ability use, to increase the chance of being hit
        by enemies attacks while casting it. Of course this is targetting abilities that can be casted while moving.
        */
        public override void InitCastMoveSlow(float speedPercent, float castSlowDuration, float castSlowRate)
        {
            curSpeedPercentage = 1;
            speedPercentageTarget = speedPercent;
            currentCastSlowDur = 0;
            maxCastSlowDur = castSlowDuration;
            speedCastSlowRate = castSlowRate;
            isCastingSlowed = true;
        }

        protected override void HandleCastSlowed()
        {
            curSpeedPercentage -= speedCastSlowRate;
            if (curSpeedPercentage < speedPercentageTarget) curSpeedPercentage = speedPercentageTarget;

            currentCastSlowDur += Time.deltaTime;

            float newMoveSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode);
            newMoveSpeed *= curSpeedPercentage;
            newMoveSpeed = (float) Math.Round(newMoveSpeed, 2);
            MovementSpeedChange(newMoveSpeed);

            if (currentCastSlowDur >= maxCastSlowDur) ResetCastSlow();
        }

        protected override void ResetCastSlow()
        {
            isCastingSlowed = false;
            curSpeedPercentage = 1;
            speedPercentageTarget = 1;
            currentCastSlowDur = 0;
            maxCastSlowDur = 0;
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController.anim.SetFloat(moveSpeedModifier, curSpeedPercentage);
            }

            MovementSpeedChange(RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode));
        }

        /*
        -- LOGIC UPDATES --
        */
        public override void FixedUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;
            if (CombatManager.playerCombatNode.dead) return;

            HandleCombatStates();

            if (knockbackActive)
                HandleKnockback();
            
            if (motionActive)
                HandleMotion();

            if (isTeleporting)
                HandleTeleporting();
            
            if(controller.isSprinting)
                HandleSprint();


            if (isResetingSprintCamFOV)
                HandleSprintCamFOVReset();

        }

        private void HandleSprintCamFOVReset()
        {
            controller._playerCamera.Camera.fieldOfView = Mathf.Lerp(controller._playerCamera.Camera.fieldOfView,
                controller.CameraSettings.NormalFOV, Time.deltaTime * controller.CameraSettings.FOVLerpSpeed);

            if (Mathf.Abs(controller._playerCamera.Camera.fieldOfView - controller.CameraSettings.NormalFOV) < 0.25f)
            {
                controller._playerCamera.Camera.fieldOfView = controller.CameraSettings.NormalFOV;
                isResetingSprintCamFOV = false;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("TOGGLE_CAMERA_AIM_MODE")))
            {
                SetCameraAiming(!controller.CameraSettings.isAiming);
            }

            if (isAimingTransition)
            {
                HandleAimingTransition();
            }
        }

        private void HandleAimingTransition()
        {
            if (controller.CameraSettings.isAiming)
            {
                if (controller._playerCamera.Pivot.transform.localPosition != controller.CameraSettings.aimingPivot)
                {
                    controller._playerCamera.Pivot.transform.localPosition = Vector3.Lerp(
                        controller._playerCamera.Pivot.transform.localPosition, controller.CameraSettings.aimingPivot
                        , controller.CameraSettings.aimInSpeed * Time.deltaTime);
                }
                else
                {
                    isAimingTransition = false;
                }
            }
            else
            {
                if (controller._playerCamera.Pivot.transform.localPosition != controller.CameraSettings.normalPivot)
                {
                    controller._playerCamera.Pivot.transform.localPosition = Vector3.Lerp(
                        controller._playerCamera.Pivot.transform.localPosition, controller.CameraSettings.normalPivot
                        , controller.CameraSettings.aimOutSpeed * Time.deltaTime);
                }
                else
                {
                    isAimingTransition = false;
                }
            }
        }

        protected override void HandleTeleporting()
        {
            transform.position = teleportTargetPos;
            isTeleporting = false;
        }

        protected override void HandleCombatStates()
        {
            if (isCastingSlowed) HandleCastSlowed();
            if (standTimeActive) HandleStandTime();
        }

        /*
        -- TELEPORT FUNCTIONS --
        Easy way to instantly teleport the player to a certain location.
        Called by DevUIManager and CombatManager
        */
        public override void TeleportToTarget(Vector3 pos) // Teleport to the Vector3 Coordinates
        {
            isTeleporting = true;
            teleportTargetPos = pos;
        }

        public override void TeleportToTarget(CombatNode target) // Teleport to the CombatNode Coordinates
        {
            isTeleporting = true;
            teleportTargetPos = target.transform.position;
        }

        /*
        -- CHECKING CONDITIONAL FUNCTIONS --
        */
        public override bool HasMovementRestrictions()
        {
            return CombatManager.playerCombatNode.dead ||
                   !canMove ||
                   isTeleporting ||
                   standTimeActive ||
                   knockbackActive ||
                   motionActive ||
                   isLeaping ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        public override bool HasRotationRestrictions()
        {
            return CombatManager.playerCombatNode.dead ||
                   isLeaping ||
                   knockbackActive ||
                   motionActive ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        /*
        -- UI --
        */
        public override void GameUIPanelAction(bool opened)
        {
            SetCameraMouseLook(!opened);
        }

        /*
        -- CAMERA --
        */
        protected override void SetCameraMouseLook(bool state)
        {
            if (playerIsDead) return;
            controller.cameraCanRotate = state;
            Cursor.visible = !state;
            Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        }

        public override void ToggleCameraMouseLook()
        {
            if (playerIsDead) return;
            var state = !controller.cameraCanRotate;
            SetCameraMouseLook(state);
        }

        /*
         *
         * MOVEMENT
         */

        public override void StartSprint()
        {
            controller.isSprinting = true;
            
        }
        public override void EndSprint()
        {
            controller.isSprinting = false;
            isResetingSprintCamFOV = true;
        }


        public override void HandleSprint()
        {
            if (controller.CameraSettings.NormalFOV != controller.CameraSettings.SprintFOV)
            {
                controller._playerCamera.Camera.fieldOfView = Mathf.Lerp(controller._playerCamera.Camera.fieldOfView,
                    controller.CameraSettings.SprintFOV, Time.deltaTime * controller.CameraSettings.FOVLerpSpeed);
            }
            
            
            if(RPGBuilderEssentials.Instance.sprintStatDrainReference == null) return;

            if (!(Time.time >= nextSprintStatDrain)) return;
            nextSprintStatDrain = Time.time + RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainInterval;
            CombatManager.playerCombatNode.AlterVitalityStat(RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainAmount, 
                RPGBuilderEssentials.Instance.combatSettings.sprintStatDrainID);
        }

        public override bool isSprinting()
        {
            return controller.isSprinting;
        }

        /*
        -- CONDITIONS --
        */
        public override bool ShouldCancelCasting()
        {
            return !IsGrounded() || IsMoving();
        }

        public override bool IsGrounded()
        {
            return charController.isGrounded;
        }

        public override bool IsMoving()
        {
            return charController.velocity != Vector3.zero;
        }

        public override bool IsThirdPersonShooter()
        {
            return controller.CameraSettings.isAiming;
        }

        public override RPGGeneralDATA.ControllerTypes GETControllerType()
        {
            return controller.CameraSettings.isAiming ? RPGGeneralDATA.ControllerTypes.ThirdPersonShooter : RPGGeneralDATA.ControllerTypes.ThirdPerson;
        }

        public override void MainMenuInit()
        {
            Destroy(GetComponent<RPGBThirdPersonController>());
            Destroy(GetComponent<CharacterAnimator>());
            Destroy(GetComponent<RPGBThirdPersonCharacterControllerEssentials>());
            Destroy(GetComponent<RPGBCharacterWorldInteraction>());
            Destroy(charController);
        }
        
        public override void AbilityInitActions(RPGAbility.RPGAbilityRankData rankREF)
        {
        }
        public override void AbilityEndCastActions(RPGAbility.RPGAbilityRankData rankREF)
        {
        }
    }
}
