using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using TMPro;

namespace BLINK.RPGBuilder.Managers
{
    public class StatAllocationDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;
        private bool showing;

        public GameObject statSlotPrefab;
        public Transform statSlotsParent;
        public List<StatAllocationSlotDataHolder> curStatSlots = new List<StatAllocationSlotDataHolder>();

        public TextMeshProUGUI currentPointsText;
        public static StatAllocationDisplayManager Instance { get; private set; }
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void ClearAllStatSlots()
        {
            foreach (var t in curStatSlots)
                Destroy(t.gameObject);

            curStatSlots.Clear();
        }

        private void InitStatList()
        {
            ClearAllStatSlots();
            
            List<CharacterData.AllocatedStatEntry> allStats = getAllStats();
            
            foreach (var statAllocationEntry in allStats)
            {
                StatAllocationManager.Instance.SpawnStatAllocationSlot(statAllocationEntry, statSlotPrefab,
                    statSlotsParent, curStatSlots, StatAllocationSlotDataHolder.SlotType.Game);
            }
            
            
            foreach (var allocatedStatSlot in curStatSlots)
            {
                float currentValue = StatAllocationManager.Instance.getAllocatedStatValue(allocatedStatSlot.thisStat.ID, StatAllocationSlotDataHolder.SlotType.Game);
                
                float max = StatAllocationManager.Instance.getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                allocatedStatSlot.curValueText.text = max > 0 ? currentValue + " / " + max :
                    currentValue.ToString();
            }
            
            UpdateCurrentPointText();
            
            StatAllocationManager.Instance.HandleStatAllocationButtons(CharacterData.Instance.getTreePointsAmountByPoint(RPGBuilderEssentials.Instance.combatSettings.pointID), 0, curStatSlots, StatAllocationSlotDataHolder.SlotType.Game);
        }

        public void UpdateCurrentPointText()
        {
            currentPointsText.text = "Points: " + CharacterData.Instance.getTreePointsAmountByPoint(RPGBuilderEssentials.Instance.combatSettings.pointID);
        }

        private List<CharacterData.AllocatedStatEntry> getAllStats()
        {
            List<CharacterData.AllocatedStatEntry> allStats = new List<CharacterData.AllocatedStatEntry>();

            foreach (var skill in CharacterData.Instance.skillsDATA)
            {
                RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(skill.skillID);
                foreach (var stat in skillREF.allocatedStatsEntriesGame)
                {
                    if(stat.statID != -1) allStats.Add(stat);
                }
            }
            foreach (var weaponTemplate in CharacterData.Instance.weaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(weaponTemplate.weaponTemplateID);
                foreach (var stat in weaponTemplateREF.allocatedStatsEntriesGame)
                {
                    if(stat.statID != -1) allStats.Add(stat);
                }
            }
            
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                foreach (var stat in classREF.allocatedStatsEntriesGame)
                {
                    if(stat.statID != -1) allStats.Add(stat);
                }
            }

            return allStats;
        }
        
        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitStatList();
            
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
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
