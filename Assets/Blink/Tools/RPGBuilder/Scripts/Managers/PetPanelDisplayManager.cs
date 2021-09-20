using System;
using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class PetPanelDisplayManager : MonoBehaviour
    {
        public CanvasGroup thisCG;
        private bool showing;

        public Color nonSelectedColor, selectedColor;
        public Image stayButton, followButton, defendButton, aggroButton, resetButton, attackButton;
        public Image petsHealthBar;
        public TextMeshProUGUI summonCountText;

        public static PetPanelDisplayManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void InitPetPanel()
        {
            InitButtonsSelection(CombatManager.playerCombatNode.currentPetsMovementActionType,
                CombatManager.playerCombatNode.currentPetsCombatActionType);
            UpdateHealthBar();
            UpdateSummonCountText();
        }

        public void UpdateSummonCountText()
        {
            var maxSummons = CombatManager.Instance.getCurrentSummonCount(CombatManager.playerCombatNode);
            summonCountText.text = "Summons: " + CombatManager.playerCombatNode.currentPets.Count + " / " + maxSummons;
        }

        public void UpdateHealthBar()
        {
            float totalCurrentPetsHealt = 0, totalmaxPetsHealth = 0;
            foreach (var t in CombatManager.playerCombatNode.currentPets)
            {
                totalCurrentPetsHealt += t.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name);
                totalmaxPetsHealth += t.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            }

            petsHealthBar.fillAmount = totalCurrentPetsHealt / totalmaxPetsHealth;
        }

        private void InitButtonsSelection(CombatNode.PET_MOVEMENT_ACTION_TYPES movementAction,
            CombatNode.PET_COMBAT_ACTION_TYPES combatAction)
        {
            resetAllButtons();
            switch (movementAction)
            {
                case CombatNode.PET_MOVEMENT_ACTION_TYPES.stay:
                    stayButton.color = selectedColor;
                    break;
                case CombatNode.PET_MOVEMENT_ACTION_TYPES.follow:
                    followButton.color = selectedColor;
                    break;
            }

            switch (combatAction)
            {
                case CombatNode.PET_COMBAT_ACTION_TYPES.defend:
                    defendButton.color = selectedColor;
                    break;
                case CombatNode.PET_COMBAT_ACTION_TYPES.aggro:
                    aggroButton.color = selectedColor;
                    break;
            }
        }

        public void selectMovementActionButton(string action)
        {
            var actionEnum =
                (CombatNode.PET_MOVEMENT_ACTION_TYPES) Enum.Parse(typeof(CombatNode.PET_MOVEMENT_ACTION_TYPES), action);
            InitButtonsSelection(actionEnum, CombatManager.playerCombatNode.currentPetsCombatActionType);
            CombatManager.playerCombatNode.currentPetsMovementActionType = actionEnum;
        }

        public void selectCombatActionButton(string action)
        {
            var actionEnum =
                (CombatNode.PET_COMBAT_ACTION_TYPES) Enum.Parse(typeof(CombatNode.PET_COMBAT_ACTION_TYPES), action);
            InitButtonsSelection(CombatManager.playerCombatNode.currentPetsMovementActionType, actionEnum);
            CombatManager.playerCombatNode.currentPetsCombatActionType = actionEnum;
        }

        public void resetPetsActions()
        {
            selectCombatActionButton("defend");
            foreach (var pet in CombatManager.playerCombatNode.currentPets)
            {
                pet.agentREF.ClearThreatTable();
                pet.agentREF.ResetTarget();
            }
        }
        public void requestPetsAttack()
        {
            if (CombatManager.Instance.PlayerTargetData.currentTarget == null) return;
            foreach (var pet in CombatManager.playerCombatNode.currentPets)
            {
                pet.agentREF.SetTarget(CombatManager.Instance.PlayerTargetData.currentTarget);
            }
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitPetPanel();
        }

        private void resetAllButtons()
        {
            stayButton.color = nonSelectedColor;
            followButton.color = nonSelectedColor;
            defendButton.color = nonSelectedColor;
            aggroButton.color = nonSelectedColor;
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }
    }
}