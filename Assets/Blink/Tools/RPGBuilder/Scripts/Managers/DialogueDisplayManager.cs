using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class DialogueDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;

        public GameObject playerOptionPrefab;
        public Transform playerOptionParent;
        public List<GameObject> curPlayerOptions = new List<GameObject>();

        public TextMeshProUGUI NPCMessageText;

        private CombatNode currentNPC;
        private RPGDialogue currentDialogue;
        private RPGDialogueGraph currentGraph;
        //private int currentNPCNodeIndex;
        private RPGDialogueTextNode currentNPCNode;

        public Image npcImage, playerImage;
         
        private bool isShowing;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static DialogueDisplayManager Instance { get; private set; }

        
        private void ClearAllSlots()
        {
            foreach (var t in curPlayerOptions)
                Destroy(t);

            curPlayerOptions.Clear();
        }

        public void InitDialogue(CombatNode NPC)
        {
            currentNPC = NPC;
            currentDialogue = RPGBuilderUtilities.GetDialogueFromID(NPC.npcDATA.dialogueID);
            currentGraph = currentDialogue.dialogueGraph;
            currentNPCNode = RPGBuilderUtilities.getFirstNPCNode(currentGraph);

            UpdateDialogueView();
            Show();
        }
        
        private void UpdateDialogueView()
        {
            ClearAllSlots();
            playerImage.enabled = false;
            playerImage.sprite = null;

            if (!RPGBuilderUtilities.characterHasDialogue(currentDialogue.ID))
            {
                RPGBuilderUtilities.addDialogueToCharacter(currentDialogue.ID);
            }
            
            if (!RPGBuilderUtilities.characterDialogueHasDialogueNode(currentDialogue.ID, currentNPCNode))
            {
                RPGBuilderUtilities.addNodeToDialogue(currentDialogue.ID, currentNPCNode);
            }
            if (!currentNPCNode.viewedEndless && !RPGBuilderUtilities.dialogueNodeCanBeViewed(currentDialogue.ID, currentNPCNode,
                currentNPCNode.viewCountMax))
            {
                Hide();
            }
            CharacterEventsManager.Instance.DialogueNodeViewed(currentDialogue, currentNPCNode);
            
            HandleNodeGameActions(currentNPCNode);
            
            NPCMessageText.text = replaceKeywordsFromText(currentNPCNode.message);
            npcImage.enabled = currentNPCNode.nodeImage != null;
            if (currentNPCNode.nodeImage != null)
            {
                npcImage.sprite = currentNPCNode.nodeImage;
            }

            foreach (var playerOption in currentNPCNode.GetOutputPort("nextNodes").GetConnections())
            {
                Node playerNode = playerOption.node;
                RPGDialogueTextNode playerTextNode = (RPGDialogueTextNode) playerNode;
                
                if (!RPGBuilderUtilities.characterDialogueHasDialogueNode(currentDialogue.ID, playerTextNode))
                {
                    RPGBuilderUtilities.addNodeToDialogue(currentDialogue.ID, playerTextNode);
                }
                
                if(!isAnswerAvailable(playerTextNode)) continue;
                
                CharacterEventsManager.Instance.DialogueNodeViewed(currentDialogue, playerTextNode);
                
                GameObject newOptionGO = Instantiate(playerOptionPrefab, Vector3.zero,
                    Quaternion.identity, playerOptionParent);
                PlayerDialogueOptionSlot slotREF = newOptionGO.GetComponent<PlayerDialogueOptionSlot>();
                slotREF.Init(playerTextNode);
                curPlayerOptions.Add(newOptionGO);
            }

            if (!currentDialogue.hasExitNode) return;
            GameObject exitNode = Instantiate(playerOptionPrefab, Vector3.zero,
                Quaternion.identity, playerOptionParent);
            PlayerDialogueOptionSlot exitNodeREF = exitNode.GetComponent<PlayerDialogueOptionSlot>();
            exitNodeREF.InitExitNode(currentDialogue.exitNodeText);
            curPlayerOptions.Add(exitNode);

        }

        private bool isAnswerAvailable(RPGDialogueTextNode playerTextNode)
        {
            if (!isNodeMeetingRequirements(playerTextNode.RequirementList))
            {
                return false;
            }

            if (RPGBuilderUtilities.isDialogueLineCompleted(currentDialogue.ID, playerTextNode))
            {
                return false;
            }
            if (!playerTextNode.viewedEndless && !RPGBuilderUtilities.dialogueNodeCanBeViewed(currentDialogue.ID, playerTextNode,
                playerTextNode.viewCountMax))
            {
                return false;
            }
            if (!playerTextNode.clickedEndless && !RPGBuilderUtilities.dialogueNodeCanBeClicked(currentDialogue.ID, playerTextNode,
                playerTextNode.clickCountMax))
            {
                return false;
            }

            return true;
        }

        public void ShowPlayerImageAfterHover(Sprite image)
        {
            playerImage.enabled = image != null;
            if (image != null)
            {
                playerImage.sprite = image;
            }
        }

        bool isNodeMeetingRequirements(List<RequirementsManager.RequirementDATA> reqList)
        {
            List<bool> reqResults = new List<bool>();
            foreach (var t in reqList)
            {
                var intValue1 = 0;
                var intValue2 = 0;
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        break;
                    case RequirementsManager.RequirementType._class:
                        intValue1 = t.classRequiredID;
                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        break;
                }

                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2,false));
            }

            return !reqResults.Contains(false);
        }

        public void HandlePlayerAnswer(RPGDialogueTextNode answerNode)
        {
            currentNPCNode = RPGBuilderUtilities.getNextNPCNode(answerNode);
            HandleNodeGameActions(answerNode);
            MoveToNextNPCMessage(answerNode);
            CharacterEventsManager.Instance.DialogueNodeClicked(currentDialogue, answerNode);
        }

        private void HandleNodeGameActions(RPGDialogueTextNode answerNode)
        {
            foreach (var gameAction in answerNode.GameActionsList)
            {
                var chance = Random.Range(0, 100f);
                if (gameAction.chance != 0 && !(chance <= gameAction.chance)) continue;
                switch (gameAction.actionType)
                {
                    case ActionType.UseAbility:
                        GameActionsManager.Instance.UseAbility(currentNPC, gameAction.assetID);
                        break;
                    case ActionType.ApplyEffect:
                        GameActionsManager.Instance.ApplyEffect(gameAction.target, currentNPC, gameAction.assetID);
                        break;
                    case ActionType.GainItem:
                        GameActionsManager.Instance.GainItem(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.RemoveItem:
                        GameActionsManager.Instance.LoseItem(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LearnAbility:
                        GameActionsManager.Instance.LearnAbility(gameAction.assetID);
                        break;
                    case ActionType.LearnRecipe:
                        GameActionsManager.Instance.LearnRecipe(gameAction.assetID);
                        break;
                    case ActionType.LearnResourceNode:
                        GameActionsManager.Instance.LearnResourceNode(gameAction.assetID);
                        break;
                    case ActionType.LearnBonus:
                        GameActionsManager.Instance.LearnBonus(gameAction.assetID);
                        break;
                    case ActionType.LearnSkill:
                        Debug.LogError("Learn Skill option not implemented yet");
                        break;
                    case ActionType.LearnTalentTree:
                        Debug.LogError("Learn Talent Tree option not implemented yet");
                        break;
                    case ActionType.GainTreePoint:
                        GameActionsManager.Instance.GainTreePoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseTreePoint:
                        GameActionsManager.Instance.LoseTreePoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainEXP:
                        GameActionsManager.Instance.GainEXP(gameAction.count);
                        break;
                    case ActionType.GainSkillEXP:
                        GameActionsManager.Instance.GainSkillEXP(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainWeaponTemplateEXP:
                        GameActionsManager.Instance.GainWeaponTemplateEXP(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainLevel:
                        GameActionsManager.Instance.GainLevel(gameAction.count);
                        break;
                    case ActionType.GainSkillLevel:
                        GameActionsManager.Instance.GainSkillLevel(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.ProposeQuest:
                        GameActionsManager.Instance.ProposeQuest(gameAction.assetID);
                        break;
                    case ActionType.GainCurrency:
                        GameActionsManager.Instance.GainCurrency(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseCurrency:
                        GameActionsManager.Instance.LoseCurrency(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.SpawnGameObject:
                        GameActionsManager.Instance.SpawnGameobject(gameAction.prefab, gameAction.spawnPosition);
                        break;
                    case ActionType.DestroyGameObject:
                        GameActionsManager.Instance.DestroyGameobject(gameAction.GameObjectName);
                        break;
                    case ActionType.ActivateGameObject:
                        GameActionsManager.Instance.ActivateGameObject(gameAction.GameObjectName);
                        break;
                    case ActionType.DeactiveGameObject:
                        GameActionsManager.Instance.DeactivateGameObject(gameAction.GameObjectName);
                        break;
                    case ActionType.ToggleWorldNode:
                        Debug.LogError("Toggle World Node option not implemented yet");
                        break;
                    case ActionType.GainFactionpoints:
                        GameActionsManager.Instance.GainFactionpoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseFactionPoints:
                        GameActionsManager.Instance.LoseFactionpoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.PlaySound:
                        GameActionsManager.Instance.PlaySound(gameAction.audioClip);
                        break;
                    case ActionType.Teleport:
                        GameActionsManager.Instance.TeleportPlayer(gameAction.assetID, gameAction.spawnPosition, gameAction.teleportType);
                        break;
                    case ActionType.SaveCharacterData:
                        GameActionsManager.Instance.SaveCharacterData();
                        break;
                    case ActionType.RemoveEffect:
                        GameActionsManager.Instance.RemoveCharacterEffect(gameAction.assetID);
                        break;
                    case ActionType.PlayAnimation:
                        GameActionsManager.Instance.PlayAnimation(currentNPC, answerNode.identityType, gameAction.animationName);
                        break;
                    case ActionType.CompleteDialogueLine:
                        GameActionsManager.Instance.CompleteDialogueLine(currentDialogue.ID, gameAction.textNodeREF);
                        break;
                }
            }
        }

        private void MoveToNextNPCMessage(RPGDialogueTextNode answerNode)
        {
            if (doesAnswerHasNextNPCMessage(answerNode))
            {
                UpdateDialogueView();
            }
        }

        public void ExitDialogue()
        {
            Hide();
        }

        private bool doesAnswerHasNextNPCMessage(RPGDialogueTextNode answerNode)
        {
            if (answerNode.GetOutputPort("nextNodes").ConnectionCount != 0) return true;
            Hide();
            return false;

        }

        private string replaceKeywordsFromText(string message)
        {
            foreach (var keyword in RPGBuilderEssentials.Instance.generalSettings.dialogueKeywords)
            {
                switch (keyword)
                {
                    case "[player_name]":
                        message = message.Replace( keyword, CharacterData.Instance.CharacterName);
                        break;
                    case "[player_level]":
                        message = message.Replace( keyword, CharacterData.Instance.classDATA.currentClassLevel.ToString());
                        break;
                }
            }

            return message;
        }

        public void Show()
        {
            isShowing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(true);
        }

        public void Hide()
        {
            isShowing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!isShowing || currentNPC == null) return;
            if(Vector3.Distance(currentNPC.transform.position, CombatManager.playerCombatNode.transform.position) > 4) Hide();
        }

        public void IncreaseNodeViewCount(int dialogueID, RPGDialogueTextNode textNode)
        {
            foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
            {
                if(dialogue.ID != dialogueID) continue;
                foreach (var node in dialogue.nodesData)
                {
                    if(node.textNode != textNode) continue;
                    node.currentlyViewedCount++;
                }
            }   
        }
        public void IncreaseNodeClickCount(int dialogueID, RPGDialogueTextNode textNode)
        {
            foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
            {
                if(dialogue.ID != dialogueID) continue;
                foreach (var node in dialogue.nodesData)
                {
                    if(node.textNode != textNode) continue;
                    node.currentlyClickedCount++;
                }
            }   
        }
        
    }
}
