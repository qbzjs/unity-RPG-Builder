using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UI
{
    public class AbilityTooltip : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public RectTransform canvasRect, thisRect, contentRect;

        public TextMeshProUGUI abilityNameText;
        public Image icon;
        private RPGAbility lastAbility;

        public Transform contentParent;
        private readonly List<GameObject> curElementsGO = new List<GameObject>();

        public GameObject CenteredTitleElementPrefab, DescriptionElementPrefab, SeparationElementPrefan;
        public Color physicalDamageColor, magicalDamageColor, healingColor, durationColor;
        private string endColorString = "</color>";
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void Update()
        {
            if (thisCG.alpha != 1) return;
            HandleTooltipPosition();
        }

        private void HandleTooltipPosition()
        {
            Vector2 anchoredPos = Input.mousePosition / canvasRect.localScale.x;
            if (cursorIsRightSide())
            {
                if (anchoredPos.x + (thisRect.rect.width+100f) > canvasRect.rect.width)
                    anchoredPos.x -= thisRect.rect.width + 10f;
                else
                    anchoredPos.x += 10f;
            }
            else
            {
                anchoredPos.x += 10f;
            }

            anchoredPos.y += contentRect.sizeDelta.y + 10f;

            if (anchoredPos.y + thisRect.rect.height > canvasRect.rect.height)
            {
                anchoredPos.y = canvasRect.rect.height - thisRect.rect.height;
            }

            thisRect.anchoredPosition = anchoredPos;
        }
        

        private bool cursorIsRightSide()
        {
            return Input.mousePosition.x > Screen.width / 2.0f;
        }
        
        public void Show(RPGAbility ability)
        {
            lastAbility = ability;
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = ability.displayName;
            icon.sprite = ability.icon;

            GenerateTooltip(ability);
        }
        public void ShowBonus(RPGBonus bonus)
        {
            //lastAbility = ability;
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = bonus.displayName;
            icon.sprite = bonus.icon;

            GenerateBonusTooltip(bonus);
        }
        public void ShowEffect(RPGEffect effect, int effectRank)
        {
            //lastAbility = ability;
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = effect.displayName;
            icon.sprite = effect.icon;

            GenerateEffectTooltip(effect, effectRank);
        }

        private void GenerateBonusTooltip(RPGBonus bonus)
        {
            ClearAllAbilityTooltipElements();
            var curRank = RPGBuilderUtilities.getBonusRank(bonus.ID);
            if (curRank == -1) curRank = 0;
            var bonusRank = bonus.ranks[curRank];

            var description = "Permanently:";

            foreach (var t in bonusRank.statEffectsData)
            {
                string modifierText = "";
                float statValue = t.statEffectModification;
                if (t.statEffectModification > 0)
                {
                    modifierText = "Increased";
                }
                else
                {
                    modifierText = "Reduced";
                    statValue = Mathf.Abs(statValue);
                }

                string addText = $"{statValue}";
                RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t.isPercent || statREF.isPercentStat)
                {
                    addText += " %";
                }

                description += $"\n{statREF.displayName} {modifierText} by {addText}";
            }

            SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description, description);
        }
        
        private void GenerateEffectTooltip(RPGEffect effect, int effectRank)
        {
            ClearAllAbilityTooltipElements();
            var description = generateEffectDescription(effect, effectRank, false, RPGCombatDATA.TARGET_TYPE.Caster);
            SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description, description);
        }

        public void Hide()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        private void Awake()
        {
            Hide();
        }


        private void ClearAllAbilityTooltipElements()
        {
            foreach (var t in curElementsGO)
                Destroy(t);

            curElementsGO.Clear();
        }

        string getColorHEX(Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
        }

        private void GenerateTooltip(RPGAbility ab)
        {
            ClearAllAbilityTooltipElements();
            var curRank = RPGBuilderUtilities.getAbilityRank(ab.ID);
            if (curRank == -1) curRank = 0;
            if (ab.ranks.Count == 0) return;
            var rankREF = ab.ranks[curRank];
            
            var description = "";

            switch (rankREF.targetType)
            {
                case RPGAbility.TARGET_TYPES.SELF:
                    description += RPGBuilderUtilities.addLineBreak("Type: Self");
                    break;
                case RPGAbility.TARGET_TYPES.CONE:
                    description += RPGBuilderUtilities.addLineBreak("Type: Cone (" + rankREF.coneDegree + "°)");
                    break;
                case RPGAbility.TARGET_TYPES.AOE:
                    description += RPGBuilderUtilities.addLineBreak("Type: AoE (" + rankREF.AOERadius + "m Radius)");
                    break;
                case RPGAbility.TARGET_TYPES.LINEAR:
                    description += RPGBuilderUtilities.addLineBreak("Type: Linear");
                    break;
                case RPGAbility.TARGET_TYPES.PROJECTILE:
                    description += RPGBuilderUtilities.addLineBreak("Type: Projectile");
                    break;
                case RPGAbility.TARGET_TYPES.GROUND:
                    description += RPGBuilderUtilities.addLineBreak("Type: Ground Target");
                    break;
                case RPGAbility.TARGET_TYPES.GROUND_LEAP:
                    description += RPGBuilderUtilities.addLineBreak("Type: Ground Leap");
                    break;
                case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                    description += RPGBuilderUtilities.addLineBreak("Type: Targetted Projectile");
                    break;
                case RPGAbility.TARGET_TYPES.TARGET_INSTANT:
                    description += RPGBuilderUtilities.addLineBreak("Type: Target");
                    break;
            }

            foreach (var req in rankREF.useRequirements)
            {
                switch (req.requirementType)
                {
                    case RequirementsManager.AbilityUseRequirementType.item:
                        RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(req.itemRequiredID);
                        description +=
                            RPGBuilderUtilities.addLineBreak("Require " + req.itemRequiredCount + " " +
                                                             itemREF.displayName);
                        break;
                    case RequirementsManager.AbilityUseRequirementType.weaponTypeEquipped:
                        description +=
                            RPGBuilderUtilities.addLineBreak("Require " + req.weaponRequired + " equipped");
                        break;
                    case RequirementsManager.AbilityUseRequirementType.statCost:
                        RPGStat statREF = RPGBuilderUtilities.GetStatFromID(req.statCostID);
                        string costTypeText = "";
                        switch (req.costType)
                        {
                            case RPGAbility.COST_TYPES.FLAT:
                                costTypeText = req.useCost + " " + statREF.displayName;
                                break;
                            case RPGAbility.COST_TYPES.PERCENT_OF_MAX:
                                costTypeText = req.useCost + "% of the maximum " + statREF.displayName;
                                break;
                            case RPGAbility.COST_TYPES.PERCENT_OF_CURRENT:
                                costTypeText = req.useCost + "% of the current " + statREF.displayName;
                                break;
                        }
                        description +=
                            RPGBuilderUtilities.addLineBreak("Cost " + costTypeText);
                        break;
                }
            }

            switch (rankREF.activationType)
            {
                case RPGAbility.AbilityActivationType.Instant:
                    break;
                case RPGAbility.AbilityActivationType.Casted:
                    if (rankREF.castBarVisible)
                    {
                        description += RPGBuilderUtilities.addLineBreak(rankREF.castTime + " sec cast");
                        if (rankREF.castInRun)
                        {
                            description += RPGBuilderUtilities.addLineBreak("Can be casted while moving");
                        }
                    }
                    break;
                case RPGAbility.AbilityActivationType.Channeled:
                    description += RPGBuilderUtilities.addLineBreak(rankREF.channelTime + " sec channel");
                    break;
                case RPGAbility.AbilityActivationType.Charged:
                    break;
            }
            
            if (rankREF.standTimeDuration > 0)
            {
                description += RPGBuilderUtilities.addLineBreak(rankREF.standTimeDuration + " sec stand time");
            }

            if (rankREF.cooldown > 0)
            {
                description += RPGBuilderUtilities.addLineBreak(rankREF.cooldown + " sec cooldown");
            }

            foreach (var req in rankREF.effectsRequirements)
            {

                RPGEffect REF = RPGBuilderUtilities.GetEffectFromID(req.effectRequiredID);
                string targetTypeText = getTargetText(req.target);
                description +=
                    RPGBuilderUtilities.addLineBreak("Require " + REF.displayName + " active on the " + targetTypeText);

            }

            if (rankREF.targetType != RPGAbility.TARGET_TYPES.SELF &&
                rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_INSTANT &&
                rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
            {
                
                description += RPGBuilderUtilities.addLineBreak("Can hit up to " + rankREF.MaxUnitHit + " units");
            }

            foreach (var t in rankREF.effectsApplied)
            {
                RPGEffect REF = RPGBuilderUtilities.GetEffectFromID(t.effectID);
                if (t.chance < 100)
                {
                    description += RPGBuilderUtilities.addLineBreak(t.chance + "% Chance to apply " + REF.displayName + " on the " + getTargetText(t.target));
                }
                description += generateEffectDescription(REF, t.effectRank, false, t.target);
                if (t.isSpread)
                {
                    description += RPGBuilderUtilities.addLineBreak("This effect spreads to " + t.spreadUnitMax +
                                                                   " nearby valid units, within " + t.spreadDistanceMax + "m");
                }
            }

            SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description, description);
        }

        string getTargetText(RPGCombatDATA.TARGET_TYPE targetType)
        {
            switch (targetType)
            {
                case RPGCombatDATA.TARGET_TYPE.Target:
                    return "target";
                case RPGCombatDATA.TARGET_TYPE.Caster:
                    return "caster";
            }

            return "";
        }

        private void SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE elementType, string Text)
        {
            switch (elementType)
            {
                case AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.CenteredTitle:

                    break;

                case AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description:
                    var newElement = Instantiate(DescriptionElementPrefab, contentParent);
                    var elementRef = newElement.GetComponent<AbilityTooltipElement>();
                    elementRef.InitDescription(Text, Color.white);
                    curElementsGO.Add(newElement);
                    break;
            }
        }

        private string getBaseDamageType(RPGEffect effect, int effectRank)
        {
            var baseDamageType = "";
            switch (effect.ranks[effectRank].mainDamageType)
            {
                case RPGEffect.MAIN_DAMAGE_TYPE.PHYSICAL_DAMAGE:
                    baseDamageType = getColorHEX(physicalDamageColor);
                    break;
                case RPGEffect.MAIN_DAMAGE_TYPE.MAGICAL_DAMAGE:
                    baseDamageType = getColorHEX(magicalDamageColor);
                    break;
            }
            return baseDamageType;
        }

        private string getSecondaryDamageType(RPGEffect effect, int effectRank)
        {
            var secondaryDamageType = "";
            if (effect.ranks[effectRank].secondaryDamageType != "NONE") secondaryDamageType = effect.ranks[effectRank].secondaryDamageType;

            foreach (var t in RPGBuilderEssentials.Instance.allStats)
                if (t._name == effect.ranks[effectRank].secondaryDamageType)
                    secondaryDamageType = t.displayName;

            return secondaryDamageType;
        }

        private string breakLine(string text)
        {
            return text + "\n";
        }


        string handleExtraEffectActions(RPGEffect.RPGEffectRankData rankREF)
        {
            string effectDescription = "";
            if (rankREF.weaponDamageModifier > 0)
            {
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.weaponDamageModifier +
                                     "% of the weapon's damage is added.");
            }

            if (rankREF.lifesteal > 0)
            {
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.lifesteal +
                                                                      "% of the total damage is gained back as health.");
            }

            if (rankREF.maxHealthModifier > 0)
            {
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.maxHealthModifier +
                                                                      "% of the total caster's health is dealt as extra damage.");
            }

            if (rankREF.missingHealthModifier > 0)
            {
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.missingHealthModifier +
                                                                      "% of extra damage is dealt for each missing percent of health");
            }

            if (rankREF.requiredEffectID != -1)
            {
                RPGEffect eff = RPGBuilderUtilities.GetEffectFromID(rankREF.requiredEffectID);
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.requiredEffectDamageModifier +
                                                                      "% extra damage is " + eff.displayName + " is active on the target");
            }

            if (rankREF.damageStatID != -1)
            {
                RPGStat eff = RPGBuilderUtilities.GetStatFromID(rankREF.damageStatID);
                effectDescription += RPGBuilderUtilities.addLineBreak(rankREF.damageStatModifier +
                                                                      " extra damage per point of " + eff.displayName);
            }

            return effectDescription;
        }

        private string generateEffectDescription(RPGEffect effect, int effectRank, bool isBonus, RPGCombatDATA.TARGET_TYPE target)
        {
            var effectDescription = "";
            var baseDamageType = "";
            var secondaryDamageType = "";
            var pulseDmg = 0;
            float delay = 0;
            
            switch (effect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.InstantDamage:
                    baseDamageType = getBaseDamageType(effect, effectRank);
                    secondaryDamageType = getSecondaryDamageType(effect, effectRank);

                    effectDescription = $"Deal {baseDamageType}{effect.ranks[effectRank].Damage}{endColorString} damage. ";
                    if (secondaryDamageType != "") effectDescription += $"({secondaryDamageType})";
                    effectDescription += "\n" + handleExtraEffectActions(effect.ranks[effectRank]);
                    
                    break;
                case RPGEffect.EFFECT_TYPE.InstantHeal:
                    secondaryDamageType = getSecondaryDamageType(effect, effectRank);

                    effectDescription = $"Heal for {effect.ranks[effectRank].Damage} {baseDamageType}. ";
                    if (secondaryDamageType != "") effectDescription += $"({secondaryDamageType})";
                    break;
                case RPGEffect.EFFECT_TYPE.DamageOverTime:
                    secondaryDamageType = getSecondaryDamageType(effect, effectRank);
                    baseDamageType = getBaseDamageType(effect, effectRank);
                    pulseDmg = effect.ranks[effectRank].Damage / effect.pulses;
                    delay = effect.duration / effect.pulses;
                    effectDescription =
                        $"Deal {baseDamageType}{pulseDmg}{endColorString} damage every {getColorHEX(durationColor)}{delay:F1}s{endColorString} for {getColorHEX(durationColor)}{effect.duration}s{endColorString}. ";
                    if (secondaryDamageType != "") effectDescription += $"({secondaryDamageType})";
                    effectDescription += "\n" + handleExtraEffectActions(effect.ranks[effectRank]);
                    break;
                case RPGEffect.EFFECT_TYPE.HealOverTime:
                    secondaryDamageType = getSecondaryDamageType(effect, effectRank);
                    pulseDmg = effect.ranks[effectRank].Damage / effect.pulses;
                    delay = effect.duration / effect.pulses;
                    effectDescription = $"Heal for {pulseDmg} every {delay:F1}s for {effect.duration}s. ";
                    if (secondaryDamageType != "") effectDescription += $"({secondaryDamageType})";
                    break;
                case RPGEffect.EFFECT_TYPE.Stun:
                    effectDescription = $"Stun the target for {effect.duration}s. ";
                    break;
                case RPGEffect.EFFECT_TYPE.Stat:
                    effectDescription = isBonus ? "Permanently:" : effect.endless ? "" : $"For the next {effect.duration}s:";
                    
                    foreach (var t in effect.ranks[effectRank].statEffectsData)
                    {
                        string modifierText = "";
                        float statValue = t.statEffectModification;
                        if (t.statEffectModification > 0)
                        {
                            modifierText = "Increased";
                        }
                        else
                        {
                            modifierText = "Reduced";
                            statValue = Mathf.Abs(statValue);
                        }

                        string addText = $"{statValue}";
                        RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                        if (t.isPercent || statREF.isPercentStat)
                        {
                            addText += " %";
                        }

                        string targetText = " the " + getTargetText(target) + "'s ";

                        effectDescription += $"\n{statREF.displayName} {modifierText} by {addText}";
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Sleep:
                    effectDescription = $"Sleep the target for {effect.duration}s. ";
                    break;
                case RPGEffect.EFFECT_TYPE.Teleport:
                    switch (effect.ranks[effectRank].teleportType)
                    {
                        case RPGEffect.TELEPORT_TYPE.gameScene:
                            effectDescription =
                                $"Teleportation to {RPGBuilderUtilities.GetGameSceneFromID(effect.ranks[effectRank].gameSceneID).displayName}. ";
                            break;
                        case RPGEffect.TELEPORT_TYPE.position:
                            effectDescription = "Teleportation to a coordinate in the map. ";
                            break;
                        case RPGEffect.TELEPORT_TYPE.target:
                            effectDescription = "Teleport to the target. ";
                            break;
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Pet:
                    string durationText = $" for {effect.ranks[effectRank].petDuration}.";
                    if (effect.ranks[effectRank].petDuration == 0)
                    {
                        durationText = ".";
                    }

                    RPGNpc npcref = RPGBuilderUtilities.GetNPCFromID(effect.ranks[effectRank].petNPCDataID);
                    effectDescription =
                        $"Summon {effect.ranks[effectRank].petSPawnCount} {npcref._name}{durationText}";
                    break;
                case RPGEffect.EFFECT_TYPE.Immune:
                    effectDescription = $"Becomes immune for {effect.duration}s{endColorString}";
                    break;
            }

            effectDescription = breakLine(effectDescription);
            return effectDescription;
        }

        public static AbilityTooltip Instance { get; private set; }
    }
}