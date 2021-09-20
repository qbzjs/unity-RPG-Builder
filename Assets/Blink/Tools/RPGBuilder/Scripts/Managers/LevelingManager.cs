using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class LevelingManager : MonoBehaviour
    {
        public static LevelingManager Instance { get; private set; }
        public GameObject levelUpGO;
        public GameObject weaponTemplateLevelUpGO;
        
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void HandleSpellbookAfterLevelUp()
        {
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .spellbooks)
                {
                    foreach (var node in RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID).nodeList)
                    {
                        if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
                        {
                            if ((int)GameModifierManager.Instance.GetValueAfterGameModifier(
                                RPGGameModifier.CategoryType.Combat + "+" +
                                RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel, spellbook.spellbookID, node.abilityID)
                            > CharacterData.Instance.classDATA.currentClassLevel) continue;
                            if (!RPGBuilderUtilities.isAbilityKnown(node.abilityID))
                            {
                                RPGBuilderUtilities.setAbilityData(node.abilityID, 1, true);
                            }
                        }
                        else
                        {
                            if ((int)GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel, spellbook.spellbookID, node.bonusID)
                                > CharacterData.Instance.classDATA.currentClassLevel) continue;
                            if (!RPGBuilderUtilities.isBonusKnown(node.bonusID))
                            {
                                RPGBuilderUtilities.setBonusData(node.bonusID, 1, true);
                                BonusManager.Instance.InitBonus(RPGBuilderUtilities.GetBonusFromID(node.bonusID));
                            }
                        }
                    }
                }
            }

            foreach (var t in CharacterData.Instance.weaponTemplates)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID).spellbooks)
                {
                    foreach (var node in RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID).nodeList)
                    {
                        if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.abilityID)
                                > RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID)) continue;
                            RPGBuilderUtilities.setAbilityData(node.abilityID, 1, true);
                        }
                        else
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.bonusID)
                                > RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID)) continue;
                            RPGBuilderUtilities.setBonusData(node.bonusID, 1, true);
                            BonusManager.Instance.InitBonus(RPGBuilderUtilities.GetBonusFromID(node.bonusID));
                        }
                    }
                }
            }
        }


        public void GenerateMobEXP(RPGNpc npcDATA, CombatNode nodeRef)
        {
            var EXP = Random.Range(npcDATA.MinEXP, npcDATA.MaxEXP);
            EXP += npcDATA.EXPBonusPerLevel * nodeRef.NPCLevel;
            EXP = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Exp,
                EXP, npcDATA.ID, -1);
            
            if(RPGBuilderEssentials.Instance.combatSettings.useClasses)AddClassXP(EXP);
            HandleWeaponTemplatesXP(EXP);
        }

        void HandleWeaponTemplatesXP(int EXP)
        {
            if (CharacterData.Instance.weaponTemplates.Count <= 0) return;
            string wp1Type = "";
            string wp2Type = "";
            if (InventoryManager.Instance.equippedWeapons[0].itemEquipped != null)
                wp1Type = InventoryManager.Instance.equippedWeapons[0].itemEquipped.weaponType;
            if (InventoryManager.Instance.equippedWeapons[1].itemEquipped != null)
                wp2Type = InventoryManager.Instance.equippedWeapons[1].itemEquipped.weaponType;
            if (wp1Type == "" && wp2Type == "") return;
            foreach (var wpTemplate in CharacterData.Instance.weaponTemplates)
            {
                RPGWeaponTemplate wpTemplateREF =
                    RPGBuilderUtilities.GetWeaponTemplateFromID(wpTemplate.weaponTemplateID);
                foreach (var weapon in wpTemplateREF.weaponList)
                {
                    int EXPAmt = (int)(EXP * weapon.weaponEXPModifier);
                    if (weapon.weaponType == wp1Type)
                    {
                        var is2Handed = InventoryManager.Instance.equippedWeapons[0].itemEquipped.slotType == "TWO HANDED";

                        if (is2Handed)
                        {
                            AddWeaponTemplateXP(wpTemplate.weaponTemplateID, EXPAmt);
                        }
                        else
                        {
                            AddWeaponTemplateXP(wpTemplate.weaponTemplateID, EXPAmt / 2);
                        }
                    }

                    if (weapon.weaponType == wp2Type)
                    {
                        AddWeaponTemplateXP(wpTemplate.weaponTemplateID, EXPAmt / 2);
                    }
                }
            }
            
            if(WeaponTemplatesDisplayManager.Instance.thisCG.alpha == 1)WeaponTemplatesDisplayManager.Instance.UpdateWeaponView();
        }


        public void GenerateSkillEXP(int skillID, int Amount)
        {
            AddSkillXP(skillID, Amount);
        }

        public void AddSkillLevel(int skillID, int _amount)
        {
            var skillREF = RPGBuilderUtilities.GetSkillFromID(skillID);
            var levelTemplateREF = RPGBuilderUtilities.GetLevelTemplateFromID(skillREF.levelTemplateID);
            foreach (var t in CharacterData.Instance.skillsDATA)
            {
                t.currentSkillXP = 0;
                t.currentSkillLevel += _amount;
                t.maxSkillXP = levelTemplateREF
                    .allLevels[t.currentSkillLevel - 1].XPRequired;

                // EXECUTE POINTS GAIN REQUIREMENTS
                CharacterEventsManager.Instance.SkillLevelUp(skillREF.ID);
            }
        }

        public void AddClassLevel(int _amount)
        {
            CharacterData.Instance.classDATA.currentClassXP = 0;
            CharacterData.Instance.classDATA.currentClassLevel += _amount;
            CharacterData.Instance.classDATA.maxClassXP = RPGBuilderUtilities
                .GetLevelTemplateFromID(RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .levelTemplateID).allLevels[CharacterData.Instance.classDATA.currentClassLevel - 1].XPRequired;

            CharacterEventsManager.Instance.ClassLevelUp();
        }

        void SpawnLevelUpGO()
        {
            GameObject lvlUpGo = Instantiate(levelUpGO, CombatManager.playerCombatNode.transform.position,
                Quaternion.identity);
            lvlUpGo.transform.SetParent(CombatManager.playerCombatNode.transform);
            Destroy(lvlUpGo, 5);
        }

        private void SpawnWeaponTemplateLevelUpGO()
        {
            GameObject lvlUpGo = Instantiate(weaponTemplateLevelUpGO, CombatManager.playerCombatNode.transform.position,
                Quaternion.identity);
            lvlUpGo.transform.SetParent(CombatManager.playerCombatNode.transform);
            Destroy(lvlUpGo, 5);
        }
        
        public void AddClassXP(int _amount)
        {
            if (CharacterData.Instance.classDATA.currentClassLevel == RPGBuilderUtilities
                .GetLevelTemplateFromID(RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .levelTemplateID).levels)
            {
                CharacterData.Instance.classDATA.currentClassXP = 0;
                CharacterData.Instance.classDATA.maxClassXP = 0;
                return;
            }
            
            float totalAmt = _amount;
            float EXPMOD = CombatManager.Instance.GetTotalOfStatType(CombatManager.playerCombatNode,
                RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
            if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);
            
            ScreenTextDisplayManager.Instance.ScreenEventHandler("EXP", "Character: " + (int)totalAmt, "",
                CombatManager.playerCombatNode.gameObject);
            
            while (totalAmt > 0)
            {
                var XPRemaining = CharacterData.Instance.classDATA.maxClassXP -
                                  CharacterData.Instance.classDATA.currentClassXP;
                if (totalAmt > XPRemaining)
                {
                    CharacterData.Instance.classDATA.currentClassXP = 0;
                    totalAmt -= XPRemaining;
                    CharacterData.Instance.classDATA.currentClassLevel++;
                    CharacterData.Instance.classDATA.maxClassXP = RPGBuilderUtilities
                        .GetLevelTemplateFromID(RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                            .levelTemplateID).allLevels[CharacterData.Instance.classDATA.currentClassLevel - 1].XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    CharacterEventsManager.Instance.ClassLevelUp();
                    if (levelUpGO != null)
                    {
                        SpawnLevelUpGO();
                    }
                }
                else
                {
                    CharacterData.Instance.classDATA.currentClassXP += (int)totalAmt;
                    totalAmt = 0;
                    if (CharacterData.Instance.classDATA.currentClassXP !=
                        CharacterData.Instance.classDATA.maxClassXP) continue;
                    CharacterData.Instance.classDATA.currentClassLevel++;
                    CharacterData.Instance.classDATA.currentClassXP = 0;
                    CharacterData.Instance.classDATA.maxClassXP = RPGBuilderUtilities
                        .GetLevelTemplateFromID(RPGBuilderUtilities
                            .GetClassFromID(CharacterData.Instance.classDATA.classID).levelTemplateID)
                        .allLevels[CharacterData.Instance.classDATA.currentClassLevel - 1].XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    CharacterEventsManager.Instance.ClassLevelUp();
                    if (levelUpGO != null)
                    {
                        SpawnLevelUpGO();
                    }
                }
            }
            
            if (CharacterData.Instance.classDATA.currentClassLevel == RPGBuilderUtilities
                .GetLevelTemplateFromID(RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .levelTemplateID).levels)
            {
                CharacterData.Instance.classDATA.currentClassXP = 0;
                CharacterData.Instance.classDATA.maxClassXP = 0;
            }
            
            CombatManager.Instance.EXPBarUpdate();
        }

        public void AddSkillXP(int skillID, int _amount)
        {
            foreach (var t in CharacterData.Instance.skillsDATA)
                if (t.skillID == skillID)
                {
                    var skillREF = RPGBuilderUtilities.GetSkillFromID(skillID);
                    float totalAmt = _amount;
                    
                    float EXPMOD = CombatManager.Instance.GetTotalOfStatType(CombatManager.playerCombatNode,
                        RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
                    if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);
                    
                    ScreenTextDisplayManager.Instance.ScreenEventHandler("EXP",
                        RPGBuilderUtilities.GetSkillFromID(skillID)._name + ": + " + (int)totalAmt, "",
                        CombatManager.playerCombatNode.gameObject);
                    
                    while (totalAmt > 0)
                    {
                        var XPRemaining = t.maxSkillXP -
                                          t.currentSkillXP;
                        var levelTemplateREF = RPGBuilderUtilities.GetLevelTemplateFromID(skillREF.levelTemplateID);
                        if (totalAmt > XPRemaining)
                        {
                            t.currentSkillXP = 0;
                            totalAmt -= XPRemaining;
                            t.currentSkillLevel++;
                            t.maxSkillXP = levelTemplateREF
                                .allLevels[t.currentSkillLevel - 1].XPRequired;

                            // EXECUTE POINTS GAIN REQUIREMENTS
                            CharacterEventsManager.Instance.SkillLevelUp(skillREF.ID);
                        }
                        else
                        {
                            t.currentSkillXP += (int)totalAmt;
                            totalAmt = 0;
                            if (t.currentSkillXP != t.maxSkillXP) continue;
                            t.currentSkillLevel++;
                            t.currentSkillXP = 0;
                            t.maxSkillXP = levelTemplateREF
                                .allLevels[t.currentSkillLevel - 1].XPRequired;

                            // EXECUTE POINTS GAIN REQUIREMENTS
                            CharacterEventsManager.Instance.SkillLevelUp(skillREF.ID);
                        }
                    }
                }
            if(SkillBookDisplayManager.Instance.thisCG.alpha == 1)SkillBookDisplayManager.Instance.ShowSkillList();
        }

        public void AddWeaponTemplateXP(int weaponTemplateID, int _amount)
        {
            RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(weaponTemplateID);
            RPGLevelsTemplate lvlTemplateREF =
                RPGBuilderUtilities.GetLevelTemplateFromID(weaponTemplateREF.levelTemplateID);
            if (RPGBuilderUtilities.getWeaponTemplateLevel(weaponTemplateID) == lvlTemplateREF.levels) return;

            float totalAmt = _amount;
            float EXPMOD = CombatManager.Instance.GetTotalOfStatType(CombatManager.playerCombatNode,
                RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
            if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);

            ScreenTextDisplayManager.Instance.ScreenEventHandler("EXP",
                weaponTemplateREF.displayName + ": +" + (int) totalAmt,  "",CombatManager.playerCombatNode.gameObject);

            int weaponTemplateIndex = RPGBuilderUtilities.getWeaponTemplateIndexFromID(weaponTemplateID);
            while (totalAmt > 0)
            {
                var XPRemaining = RPGBuilderUtilities.getWeaponTemplateMaxEXP(weaponTemplateID) -
                                  RPGBuilderUtilities.getWeaponTemplateCurEXP(weaponTemplateID);
                if (totalAmt > XPRemaining)
                {
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponXP = 0;
                    totalAmt -= XPRemaining;
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel++;
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].maxWeaponXP = lvlTemplateREF
                        .allLevels[CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    CharacterEventsManager.Instance.WeaponTemplateLevelUp(weaponTemplateID);
                    if (weaponTemplateLevelUpGO != null)
                    {
                        SpawnWeaponTemplateLevelUpGO();
                    }
                    
                    ScreenTextDisplayManager.Instance.ScreenEventHandler("LEVELUP", weaponTemplateREF.displayName + " level " +
                        CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel, "",
                        CombatManager.playerCombatNode.gameObject);
                }
                else
                {
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponXP += (int) totalAmt;
                    totalAmt = 0;
                    if (CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponXP !=
                        CharacterData.Instance.weaponTemplates[weaponTemplateIndex].maxWeaponXP) continue;
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel++;
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponXP = 0;
                    CharacterData.Instance.weaponTemplates[weaponTemplateIndex].maxWeaponXP = lvlTemplateREF
                        .allLevels[CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    CharacterEventsManager.Instance.WeaponTemplateLevelUp(weaponTemplateID);
                    if (weaponTemplateLevelUpGO != null)
                    {
                        SpawnWeaponTemplateLevelUpGO();
                    }
                    ScreenTextDisplayManager.Instance.ScreenEventHandler("LEVELUP", weaponTemplateREF.displayName + " level " + 
                        CharacterData.Instance.weaponTemplates[weaponTemplateIndex].currentWeaponLevel, "",
                        CombatManager.playerCombatNode.gameObject);
                }
            }
            
            if(WeaponTemplatesDisplayManager.Instance.thisCG.alpha == 1)WeaponTemplatesDisplayManager.Instance.UpdateWeaponView();
        }

    }
}