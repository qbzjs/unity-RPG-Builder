using System;
using System.Collections;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Character
{
    public class RPGBCharacterControllerEssentials : MonoBehaviour
    {
        // Logic to Integrate for Custom Controllers

        public RPGBCharacterController builtInController;
        public CharacterController charController;
        public Animator anim;
        
        protected float currentStandTimeDur, maxStandTimeDur;
        protected float nextSprintStatDrain;
        protected bool isResetingSprintCamFOV;
        public bool standTimeActive, isLeaping, isFlying;

        public bool isTeleporting;
        protected Vector3 teleportTargetPos;
        public bool canMove = true;

        protected bool isCastingSlowed;
        public float curSpeedPercentage = 1;
        protected float speedPercentageTarget;
        protected float speedCastSlowRate;
        protected float currentCastSlowDur, maxCastSlowDur;
        private static readonly int moveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");
        
        public bool motionActive;
        protected Vector3 motionTarget;
        protected float cachedMotionDistance;
        protected float cachedMotionSpeed;
        protected Vector3 cachedPositionBeforeMotion;

        public bool controllerIsReady = false;
        
        public virtual void Awake()
        {
            anim = GetComponent<Animator>();
            charController = GetComponent<CharacterController>();

            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController = GetComponent<RPGBCharacterController>();
            }
        }

        public static bool IsUsingBuiltInControllers()
        {
            return RPGBuilderEssentials.Instance.generalSettings.useOldController;
        }
        
        public virtual IEnumerator InitControllers()
        {
            yield return new WaitForSeconds(0f);
            controllerIsReady = true;
        }

        public virtual bool IsInMotionWithoutProgress(float treshold)
        {
            return true;
        }
        
        /*
        -- GROUND LEAP FUNCTIONS --
        Ground leaps are mobility abilities. Configurable inside the editor under Combat > Abilities > Ability Type=Ground Leap
        They allow to quickly dash or leap to a certain ground location.
        */
        public Vector3 lastPosition = Vector3.zero;
        public virtual void InitGroundLeap()
        {
            isLeaping = true;

            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                ResetNav();
            }
        }

        public virtual void EndGroundLeap()
        {
            isLeaping = false;

            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                ResetNav();
            }
        }
        
        /*
        -- FLYING FUNCTIONS --
        */
        
        public virtual void InitFlying()
        {
            isFlying = true;
        }

        public virtual void EndFlying()
        {
            isFlying = false;
        }

        /*
        -- STAND TIME FUNCTIONS --
        Stand time is an optional mechanic for abilities. It allows to root the caster for a certain duration after using the ability.
        */
        public virtual void InitStandTime(float max)
        {
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                ResetNav();
                builtInController.anim.SetFloat(moveSpeed, 0);
            }

            standTimeActive = true;
            currentStandTimeDur = 0;
            maxStandTimeDur = max;
        }

        protected virtual void HandleStandTime()
        {
            currentStandTimeDur += Time.deltaTime;
            if (currentStandTimeDur >= maxStandTimeDur) ResetStandTime();
        }

        protected virtual void ResetStandTime()
        {
            standTimeActive = false;
            currentStandTimeDur = 0;
            maxStandTimeDur = 0;

            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                ResetNav();
            }
        }
        
        
        /* KNOCKBACK FUNCTIONS
         *
         * 
         */
        
        public virtual void InitKnockback(float knockbackDistance, Transform attacker)
        {
        }

        protected virtual void HandleKnockback()
        {
        
        }

        protected virtual void ResetKnockback()
        {
        }
        
        /* MOTION FUNCTIONS
         *
         * 
         */
        
        public virtual void InitMotion(float motionDistance, Vector3 motionDirection, float motionSpeed, bool immune)
        {
        }

        protected virtual void HandleMotion()
        {
        
        }

        protected virtual void ResetMotion()
        {
        }
        
        /* AIMING CAMERA
         *
         * 
         */

        protected virtual void SetCameraAiming(bool isAiming)
        {
        }

        /*
        -- CAST SLOWED FUNCTIONS --
        Cast slow is an optional mechanic for abilities. It allows the player to be temporarily slowed while
        casting an ability. I personally use it to increase the risk of certain ability use, to increase the chance of being hit
        by enemies attacks while casting it. Of course this is targeting abilities that can be casted while moving.
        */
        public virtual void InitCastMoveSlow(float speedPercent, float castSlowDuration, float castSlowRate)
        {
            curSpeedPercentage = 1;
            speedPercentageTarget = speedPercent;
            currentCastSlowDur = 0;
            maxCastSlowDur = castSlowDuration;
            speedCastSlowRate = castSlowRate;
            isCastingSlowed = true;
        }

        protected virtual void HandleCastSlowed()
        {
            curSpeedPercentage -= speedCastSlowRate;
            if (curSpeedPercentage < speedPercentageTarget) curSpeedPercentage = speedPercentageTarget;
            
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController.anim.SetFloat(moveSpeedModifier, curSpeedPercentage);
            }

            currentCastSlowDur += Time.deltaTime;

            float newMoveSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode);
            newMoveSpeed *= curSpeedPercentage;
            newMoveSpeed = (float) Math.Round(newMoveSpeed, 2);
            MovementSpeedChange(newMoveSpeed);
            if (currentCastSlowDur >= maxCastSlowDur) ResetCastSlow();
        }

        protected virtual void ResetCastSlow()
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
        public virtual void FixedUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;
            if (CombatManager.playerCombatNode.dead) return;

            HandleCombatStates();

            if (isTeleporting)
                HandleTeleporting();
            
        }

        protected virtual void HandleTeleporting()
        {
            transform.position = teleportTargetPos;
            isTeleporting = false;
        }

        protected virtual void HandleCombatStates()
        {
            if (isCastingSlowed) HandleCastSlowed();
            if (standTimeActive) HandleStandTime();
        }

        /*
        -- TELEPORT FUNCTIONS --
        Easy way to instantly teleport the player to a certain location.
        Called by DevUIManager and CombatManager
        */
        public virtual void TeleportToTarget(Vector3 pos) // Teleport to the Vector3 Coordinates
        {
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController.SetPlayerVelocity(Vector3.zero);
            }

            isTeleporting = true;
            teleportTargetPos = pos;
        }

        public virtual void TeleportToTarget(CombatNode target) // Teleport to the CombatNode Coordinates
        {
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController.SetPlayerVelocity(Vector3.zero);
            }

            isTeleporting = true;
            teleportTargetPos = target.transform.position;
        }

        /*
        -- BUILT IN CONTROLLER FUNCTIONS --
        */
        protected virtual void ResetNav()
        {
            if (builtInController.currentController != RPGBCharacterController.ControllerType.ClickMove) return;
            builtInController.navAgent.enabled = true;
            builtInController.navAgent.isStopped = false;
        }

        /*
        -- EVENT FUNCTIONS --
        */
        public virtual void MovementSpeedChange(float newSpeed)
        {
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                builtInController.SetSpeed(newSpeed);
            }
        }

        /*
        -- DEATH --
        */
        public bool playerIsDead;
        private static readonly int direction = Animator.StringToHash("direction");
        private static readonly int strafeDir = Animator.StringToHash("strafeDir");
        private static readonly int dead = Animator.StringToHash("Dead");

        public virtual void InitDeath()
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useOldController) return;
            builtInController.SetPlayerVelocity(Vector3.zero);
            anim.Rebind();
            builtInController.SetDesiredSpeed(0);
            anim.SetFloat(moveSpeed, 0);
            anim.SetFloat(direction, 0);
            anim.SetFloat(strafeDir, 0);
            anim.SetBool(dead, true);
            SetCameraMouseLook(true);
            playerIsDead = true;
        }

        public virtual void CancelDeath()
        {
            playerIsDead = false;
            if (!RPGBuilderEssentials.Instance.generalSettings.useOldController) return;
            builtInController.SetPlayerVelocity(Vector3.zero);
            anim.Rebind();
            builtInController.SetDesiredSpeed(0);
            anim.SetFloat(moveSpeed, 0);
            anim.SetFloat(direction, 0);
            anim.SetFloat(strafeDir, 0);
            anim.SetBool(dead, false);
        }

        /*
        -- RESTRICTIONS --
        */
        public virtual bool HasMovementRestrictions()
        {
            return CombatManager.playerCombatNode.dead ||
                   !canMove ||
                   isTeleporting ||
                   standTimeActive ||
                   isLeaping ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        public virtual bool HasRotationRestrictions()
        {
            return CombatManager.playerCombatNode.dead ||
                   isLeaping ||
                   CombatManager.playerCombatNode.isStunned() ||
                   CombatManager.playerCombatNode.isSleeping();
        }

        /*
        -- UI --
        */
        public virtual void GameUIPanelAction(bool opened)
        {
            if (!IsUsingBuiltInControllers()) return;
            if (builtInController.CurrentController == RPGBCharacterController.ControllerType.ThirdPerson &&
                !builtInController.ClickToRotate)
            {
                SetCameraMouseLook(!opened);
            }
        }

        /*
        -- CAMERA --
        */
        protected virtual void SetCameraMouseLook(bool state)
        {
            if (RPGBuilderEssentials.Instance.generalSettings.useOldController)
            {
                if (!builtInController.ClickToRotate && playerIsDead) return;
                builtInController.ClickToRotate = state;
                Cursor.lockState = !state ? CursorLockMode.Locked : CursorLockMode.Confined;
                Cursor.visible = state;
            }

            if (!state)
            {
                CrosshairDisplayManager.Instance.ShowCrosshair();
            }
            else
            {
                CrosshairDisplayManager.Instance.HideCrosshair();
            }
        }

        public virtual void ToggleCameraMouseLook()
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useOldController) return;
            var state = !builtInController.ClickToRotate;
            SetCameraMouseLook(state);
        }
        
        /*
         *
         * MOVEMENT
         */

        public virtual void StartSprint()
        {
            
        }
        public virtual void EndSprint()
        {
            
        }
        
        public virtual void HandleSprint()
        {
        }

        public virtual bool isSprinting()
        {
            return false;
        }
        
        /*
        -- CONDITIONAL --
        */
        public virtual bool ShouldCancelCasting()
        {
            return !IsGrounded() || IsMoving();
        }

        public virtual bool IsGrounded()
        {
            return RPGBuilderEssentials.Instance.generalSettings.useOldController &&
                   builtInController.GetIsGrounded();
        }

        public virtual bool IsMoving()
        {
            return builtInController.GetDesiredSpeed() != 0;
        }

        public virtual bool IsThirdPersonShooter()
        {
            return !builtInController.ClickToRotate;
        }

        public virtual RPGGeneralDATA.ControllerTypes GETControllerType()
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useOldController) return RPGGeneralDATA.ControllerTypes.ThirdPerson;
            switch (builtInController.currentController)
            {
                case RPGBCharacterController.ControllerType.TopDown:
                    return RPGGeneralDATA.ControllerTypes.TopDownWASD;
                case RPGBCharacterController.ControllerType.ClickMove:
                    return RPGGeneralDATA.ControllerTypes.TopDownClickToMove;
                case RPGBCharacterController.ControllerType.FirstPerson:
                    return RPGGeneralDATA.ControllerTypes.FirstPerson;
                case RPGBCharacterController.ControllerType.ThirdPerson when builtInController.ClickToRotate:
                    return RPGGeneralDATA.ControllerTypes.ThirdPerson;
                case RPGBCharacterController.ControllerType.ThirdPerson when !builtInController.ClickToRotate:
                    return RPGGeneralDATA.ControllerTypes.ThirdPersonShooter;
            }
            return RPGGeneralDATA.ControllerTypes.ThirdPerson;
        }
        
        public virtual void MainMenuInit()
        {
            Destroy(charController);
            Destroy(builtInController);
        }

        public virtual void AbilityInitActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            if (builtInController.CurrentController == RPGBCharacterController.ControllerType.ClickMove
             && (rankREF.targetType == RPGAbility.TARGET_TYPES.CONE ||
                 rankREF.targetType == RPGAbility.TARGET_TYPES.LINEAR ||
                 rankREF.targetType == RPGAbility.TARGET_TYPES.PROJECTILE
                 || rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT ||
                 rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE))
            {
                builtInController.PlayerLookAtCursor();
            }
        }
        public virtual void AbilityEndCastActions(RPGAbility.RPGAbilityRankData rankREF)
        {
        }
        
    }
}
