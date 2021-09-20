using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class NameplatesDATAHolder : MonoBehaviour
    {
        public Sprite merchantIcon,
            questGiverAvailableIcon,
            questGiverCompletedIcon,
            questGiverOnGoingIcon,
            bossIcon,
            rareMobIcon;

        public Image HBBorder, HBFill, HBFillDelay, HBOverlay, HBBackground, InteractionIcon;
        public TextMeshProUGUI NameText;

        public CanvasGroup BarCG, PlayerNameCG, EffectsCG, CastBarCG;
        public Transform statesParent;

        public float defaultWaitBeforeStart;

        public float InterpolateSpeed;

        private Coroutine hpfilldelayCoroutine;

        public Color AllyColor,
            AllyDelayColor,
            NeutralColor,
            NeutralDelayColor,
            EnemyColor,
            EnemyDelayColor,
            FocusedBorderColor,
            UnfocusedBorderColor;

        public Sprite AllyBar, NeutralBar, EnemyBar;

        public float FocusedAlpha, UnfocusedAlpha;

        public float FocusedScale, UnfocusedScale;

        private CombatNode thisNode;

        public bool IsFocused;

        public RectTransform TargetUnitAreaRectREF;

        public bool isUser;

        public enum NameplateUnitType
        {
            Enemy,
            Neutral,
            Ally
        }

        public NameplateUnitType thisUnitType;


        private GameObject TemporaryTargetCombatData;

        [Serializable]
        public class STATES_DATA
        {
            public ScreenSpaceStateDisplay statesDisplay;
            public CombatNode.NodeStatesDATA stateData;
        }

        public List<STATES_DATA> statesList = new List<STATES_DATA>();

        public GameObject[] ArchetypesIconsGO;
        public Image[] ArchetypesIconsImage;

        public Image castBarFill;
        public TextMeshProUGUI castBarAbilityNameText, castBarDurationText;

        private float curCastTime, MaxCastTime, curChannelTime, maxChannelTime;

        public CombatNode GetThisCombatNode()
        {
            return thisNode;
        }

        public void InitializeThisNameplate(CombatNode unitOID)
        {
            thisNode = unitOID;

            SetColors();
            UpdateTexts();
            UpdateBar();
            SetInteractionIcon();
            SetScale();
        }

        public void AddState(CombatNode.NodeStatesDATA nodeStateData, bool newState)
        {
            if (EffectsCG == null) return;
            EffectsCG.gameObject.SetActive(true);
            EffectsCG.alpha = 1;

            ScreenSpaceStateDisplay stateREF = null;

            if (!newState)
            {
                foreach (var state in statesList)
                {
                    if (state.stateData == nodeStateData) stateREF = state.statesDisplay;
                }
            }
            
            if (newState || stateREF == null)
            {
                var statedisplay = Instantiate(ScreenSpaceNameplates.Instance.statePrefab, statesParent);
                stateREF = statedisplay.GetComponent<ScreenSpaceStateDisplay>();

                var newStateData = new STATES_DATA
                {
                    statesDisplay = stateREF,
                    stateData = nodeStateData
                };
                statesList.Add(newStateData);
            }

            var bordercolor = Color.white;

            RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(thisNode.npcDATA.factionID);
            
            switch (nodeStateData.stateEffect.isBuffOnSelf)
            {
                case true when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                    bordercolor = AllyColor;
                    break;
                case true when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    bordercolor = EnemyColor;
                    break;
                case true when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                    bordercolor = EnemyColor;
                    break;
                case false when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    bordercolor = AllyColor;
                    break;
                case false when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                    bordercolor = AllyColor;
                    break;
            }
            
            stateREF.InitializeState(bordercolor, nodeStateData, this,
                newState ? nodeStateData.stateMaxDuration : (nodeStateData.stateMaxDuration-nodeStateData.stateCurDuration));
        }

        public void SetColors()
        {
            if (thisNode == CombatManager.playerCombatNode) return;
            RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(thisNode.npcDATA.factionID);
            
            switch (thisNodeAlignment)
            {
                case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    thisUnitType = NameplateUnitType.Enemy;

                    HBFill.sprite = EnemyBar;
                    HBFillDelay.sprite = AllyBar;
                    HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                    HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                        UnfocusedAlpha);

                    NameText.color = new Color(EnemyColor.r, EnemyColor.g, EnemyColor.b, 1);
                    break;
                case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                {
                    thisUnitType = NameplateUnitType.Neutral;

                    if (IsFocused)
                    {
                        HBFill.sprite = NeutralBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, FocusedAlpha);
                        HBBorder.color = new Color(FocusedBorderColor.r, FocusedBorderColor.g, FocusedBorderColor.b,
                            FocusedAlpha);

                        NameText.color = new Color(NeutralColor.r, NeutralColor.g, NeutralColor.b, 1);
                    }
                    else
                    {
                        HBFill.sprite = NeutralBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                        HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                            UnfocusedAlpha);

                        NameText.color = new Color(NeutralColor.r, NeutralColor.g, NeutralColor.b, 1);
                    }

                    break;
                }
                case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                {
                    thisUnitType = NameplateUnitType.Ally;
                    if (IsFocused)
                    {
                        HBFill.sprite = AllyBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, FocusedAlpha);
                        HBBorder.color = new Color(FocusedBorderColor.r, FocusedBorderColor.g, FocusedBorderColor.b,
                            FocusedAlpha);

                        NameText.color = new Color(AllyColor.r, AllyColor.g, AllyColor.b, 1);
                    }
                    else
                    {
                        HBFill.sprite = AllyBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                        HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                            UnfocusedAlpha);

                        NameText.color = new Color(AllyColor.r, AllyColor.g, AllyColor.b, 1);
                    }

                    break;
                }
            }
        }

        public void SetScale()
        {
            transform.localScale = IsFocused ? new Vector3(FocusedScale, FocusedScale, FocusedScale) : new Vector3(UnfocusedScale, UnfocusedScale, UnfocusedScale);
        }

        public void SetInteractionIcon()
        {
            InteractionIcon.enabled = false;
            if (thisNode == CombatManager.playerCombatNode) return;
            switch (thisNode.npcDATA.npcType)
            {
                case RPGNpc.NPC_TYPE.BOSS:
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = bossIcon;
                    break;
                case RPGNpc.NPC_TYPE.MERCHANT:
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = merchantIcon;
                    break;
                case RPGNpc.NPC_TYPE.QUEST_GIVER when npcHasCompletedQuest(thisNode.npcDATA):
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = questGiverCompletedIcon;
                    break;
                case RPGNpc.NPC_TYPE.QUEST_GIVER when npcHasAvailableQuest(thisNode.npcDATA):
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = questGiverAvailableIcon;
                    break;
                case RPGNpc.NPC_TYPE.QUEST_GIVER:
                {
                    if (npcHasOnGoingQuest(thisNode.npcDATA))
                    {
                        InteractionIcon.enabled = true;
                        InteractionIcon.sprite = questGiverOnGoingIcon;
                    }

                    break;
                }
                case RPGNpc.NPC_TYPE.RARE:
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = rareMobIcon;
                    break;
            }
        }

        private bool npcHasCompletedQuest(RPGNpc npc)
        {
            var currentCompletedQuests = getAllCompletedQuests();

            foreach (var t in npc.questCompleted)
                if (currentCompletedQuests.Contains(RPGBuilderUtilities.GetQuestFromID(t.questID)))
                    return true;

            return false;
        }

        private bool npcHasAvailableQuest(RPGNpc npc)
        {
            var currentAvailableQuests = getAllNeverTakenQuest(npc);

            foreach (var t in npc.questGiven)
                if (currentAvailableQuests.Contains(RPGBuilderUtilities.GetQuestFromID(t.questID)))
                    return true;

            return false;
        }

        private bool npcHasOnGoingQuest(RPGNpc npc)
        {
            var currentOnGoingQuests = getAllOnGoingQuests();

            foreach (var t in npc.questGiven)
                if (currentOnGoingQuests.Contains(RPGBuilderUtilities.GetQuestFromID(t.questID)))
                    return true;

            return false;
        }

        private List<RPGQuest> getAllCompletedQuests()
        {
            var currentCompletedQuests = new List<RPGQuest>();
            foreach (var t in CharacterData.Instance.questsData)
                if (t.state == QuestManager.questState.completed)
                    currentCompletedQuests.Add(
                        RPGBuilderUtilities.GetQuestFromID(t.questID));

            return currentCompletedQuests;
        }

        private List<RPGQuest> getAllOnGoingQuests()
        {
            var currentOnGoingQuests = new List<RPGQuest>();
            foreach (var t in CharacterData.Instance.questsData)
                if (t.state == QuestManager.questState.onGoing)
                    currentOnGoingQuests.Add(
                        RPGBuilderUtilities.GetQuestFromID(t.questID));

            return currentOnGoingQuests;
        }

        private List<RPGQuest> getAllNeverTakenQuest(RPGNpc npc)
        {
            var currentNeverTakenQuests = new List<RPGQuest>();
            var currentQuests = new List<RPGQuest>();
            foreach (var t in CharacterData.Instance.questsData)
                currentQuests.Add(RPGBuilderUtilities.GetQuestFromID(t.questID));

            foreach (var t in npc.questGiven)
            {
                var questREF = RPGBuilderUtilities.GetQuestFromID(t.questID);
                if (!QuestManager.Instance.CheckQuestRequirements(questREF)) continue;
                if (!currentQuests.Contains(questREF))
                    currentNeverTakenQuests.Add(questREF);
                else if (CharacterData.Instance.getQuestDATA(questREF).state == QuestManager.questState.abandonned)
                {
                    currentNeverTakenQuests.Add(questREF);
                }
            }

            return currentNeverTakenQuests;
        }


        public void UpdateTexts()
        {
            NameText.text = thisNode.npcDATA.displayName;
        }

        public void ResetThisNameplate()
        {
            ScreenSpaceNameplates.Instance.ResetThisNP(thisNode);
        }


        public void InitCasting(RPGAbility thisAbility)
        {
            var curRank = 0;
            if (thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(thisAbility.ID);
            var rankREF = thisAbility.ranks[curRank];

            if (thisAbility != null)
            {
                curCastTime = 0;
                MaxCastTime = rankREF.castTime;
                InitCastBar(thisAbility.displayName);
            }
            else
            {
                curCastTime = 0;
                MaxCastTime = rankREF.castTime;
                InitCastBar("");
            }
        }

        public void InitChanneling(RPGAbility thisAbility)
        {
            var curRank = 0;
            if (thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(thisAbility.ID);
            var rankREF = thisAbility.ranks[curRank];

            if (thisAbility != null)
            {
                curChannelTime = rankREF.channelTime;
                maxChannelTime = rankREF.channelTime;
                InitCastBar(thisAbility.displayName);
            }
            else
            {
                curChannelTime = rankREF.channelTime;
                maxChannelTime = rankREF.channelTime;
                InitCastBar("");
            }
        }

        public void ResetCastBar()
        {
            curCastTime = 0;
            MaxCastTime = 0;
            curChannelTime = 0;
            maxChannelTime = 0;
            castBarDurationText.text = "";
            castBarAbilityNameText.text = "";
            castBarFill.fillAmount = 0;
            CastBarCG.alpha = 0;
            CastBarCG.gameObject.SetActive(false);
        }


        private void Update()
        {
            if (MaxCastTime > 0)
            {
                curCastTime += Time.deltaTime;
                castBarFill.fillAmount = curCastTime / MaxCastTime;
                castBarDurationText.text = (MaxCastTime - curCastTime).ToString("F1");

                if (curCastTime >= MaxCastTime) ResetCastBar();
            }

            if (!(maxChannelTime > 0)) return;
            curChannelTime -= Time.deltaTime;
            castBarFill.fillAmount = curChannelTime / maxChannelTime;
            castBarDurationText.text = (maxChannelTime - curChannelTime).ToString("F1");

            if (curChannelTime <= 0) ResetCastBar();
        }

        private void InitCastBar(string AbilityName)
        {
            CastBarCG.gameObject.SetActive(true);
            CastBarCG.alpha = 1;
            castBarFill.fillAmount = 0;
            castBarAbilityNameText.text = AbilityName;
        }


        public void UpdateBar()
        {
            if (HBFill == null) return;
            var value = (int) thisNode.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            var valueMax = (int) thisNode.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);

            HBFill.fillAmount = value / (float) valueMax;

            if (value / (float) valueMax > HBFillDelay.fillAmount)
            {
                HBFillDelay.fillAmount = value / (float) valueMax;
            }
            else
            {
                if (hpfilldelayCoroutine == null)
                {
                    if (gameObject == null) return;
                    if (!gameObject.activeInHierarchy) return;
                    hpfilldelayCoroutine = StartCoroutine(FillDelay(defaultWaitBeforeStart, value / (float) valueMax, value, valueMax));
                }
                else
                {
                    StopCoroutine(hpfilldelayCoroutine);
                    hpfilldelayCoroutine = null;
                    if (gameObject == null) return;
                    if (!gameObject.activeInHierarchy) return;
                    hpfilldelayCoroutine = StartCoroutine(FillDelay(0f, value / (float) valueMax, value, valueMax));
                }
            }

            if (value <= 0) ResetThisNameplate();
        }

        private IEnumerator FillDelay(float WaitBeforeStart, float hptarget, int value, int valueMax)
        {
            yield return new WaitForSeconds(WaitBeforeStart);

            while (HBFillDelay.fillAmount > hptarget)
            {
                var amounttoreduce = InterpolateSpeed * (HBFillDelay.fillAmount - value / (float) valueMax);

                if (amounttoreduce < 0.001f) amounttoreduce = 0.001f;
                HBFillDelay.fillAmount -= amounttoreduce;

                if (HBFillDelay.fillAmount < hptarget) HBFillDelay.fillAmount = hptarget;

                if (HBFillDelay.fillAmount - hptarget < 0.003f) HBFillDelay.fillAmount = hptarget;

                yield return new WaitForSeconds(0.001f);
            }
        }
    }
}