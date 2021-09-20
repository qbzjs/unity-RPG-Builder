using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.Character
{
    public class PlayerAppearanceHandler : MonoBehaviour
    {

        public bool isShapeshifted;
        public GameObject shapeshiftModel;
        public List<CharacterData.ActionBarSlotDATA> previousActionBarSlotsDATA = new List<CharacterData.ActionBarSlotDATA>();
        public NodeSockets shapeshiftNodeSocketsREF;
        private Animator anim;
        public bool hasActiveWeaponAnimatorOverride;
        
        public RuntimeAnimatorController cachedAnimatorController;
        public Avatar cachedAnimatorAvatar;
        public bool cachedAnimatorUseRootMotion;
        public AnimatorUpdateMode cachedAnimatorUpdateMode = AnimatorUpdateMode.Normal;
        public AnimatorCullingMode cachedAnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;
        public GameObject cachedBodyParent;
        public GameObject cachedArmorsParent;
        
        public GameObject[] armorPieces;
        public Transform OneHandWeapon1CombatSlot, OneHandWeapon1RestSlot, OneHandWeapon2CombatSlot, OneHandWeapon2RestSlot, TwoHandWeaponCombatSlot, TwoHandWeaponRestSlot, ShieldRestSlot, ShieldCombatSlot;
        public GameObject weapon1GO, weapon2GO;
        public bool useBodyParts = true;
        public Transform HeadTransformSocket;
        private RPGItem weapon1RPGItem, weapon2RPGItem;
        
        public List<GameObject> armatureReferences = new List<GameObject>();
        public GameObject armatureParentGO;
        public Vector3 armatureParentOffset;
        
        [Serializable]
        public class ArmorRenderer
        {
            public string armorSlotType;
            public SkinnedMeshRenderer armorRenderer;
        }

        public List<ArmorRenderer> armorRenderers = new List<ArmorRenderer>();
        
        
        [Serializable]
        public class BodyRenderer
        {
            public string armorSlotType;
            public Renderer bodyRenderer;
        }

        public List<BodyRenderer> bodyRenderers = new List<BodyRenderer>();

        private void Awake()
        {
            anim = GetComponent<Animator>();
            cachedAnimatorController = anim.runtimeAnimatorController;
            cachedAnimatorAvatar = anim.avatar;
            cachedAnimatorUseRootMotion = anim.applyRootMotion;
            cachedAnimatorUpdateMode = anim.updateMode;
            cachedAnimatorCullingMode = anim.cullingMode;
        }

        public void ShowArmor(RPGItem item, GameObject meshManager)
        {
            switch (item.armorPieceType)
            {
                case RPGItem.ArmorPieceType.Name:
                {
                    foreach (var t in armorPieces)
                    {
                        if (t.name != item.itemModelName) continue;
                        t.SetActive(true);
                        if(useBodyParts) HideBodyPart(item.equipmentSlot);
                        if (item.modelMaterial != null)
                        {
                            foreach (var Renderer in t.GetComponentsInChildren<Renderer>())
                            {
                                Renderer.material = item.modelMaterial;
                            }
                        }
                        if (meshManager != null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, t);
                    }
                    break;
                }
                case RPGItem.ArmorPieceType.Mesh:
                    foreach (var armorRenderer in armorRenderers)
                    {
                        if(armorRenderer.armorSlotType != item.equipmentSlot) continue;
                        armorRenderer.armorRenderer.sharedMesh = item.armorMesh;
                        if (item.modelMaterial != null) armorRenderer.armorRenderer.GetComponent<Renderer>().material = item.modelMaterial;
                        armorRenderer.armorRenderer.enabled = true;
                        if(useBodyParts) HideBodyPart(item.equipmentSlot);
                    }
                    break;
            }
        }

        public void HideArmor(RPGItem item)
        {
            
            switch (item.armorPieceType)
            {
                case RPGItem.ArmorPieceType.Name:
                {
                    foreach (var t in armorPieces)
                    {
                        if (t.name != item.itemModelName) continue;
                        MeshParticleManager[] meshManagers = t.GetComponentsInChildren<MeshParticleManager>();
                        foreach (var v in meshManagers)
                        {
                            Destroy(v.gameObject);
                        }
                        t.SetActive(false);
                    }
                    break;
                }
                case RPGItem.ArmorPieceType.Mesh:
                    foreach (var armorRenderer in armorRenderers)
                    {
                        if(armorRenderer.armorSlotType != item.equipmentSlot) continue;
                        armorRenderer.armorRenderer.sharedMesh = null;
                        if (item.modelMaterial != null) armorRenderer.armorRenderer.GetComponent<Renderer>().material = item.modelMaterial;
                        armorRenderer.armorRenderer.enabled = false;
                    }
                    break;
            }

            if(useBodyParts) ShowBodyPart(item.equipmentSlot);
        }
        
        
        private void HideBodyPart(string slotType)
        {
            foreach (var bodyRenderer in bodyRenderers.Where(bodyRenderer => bodyRenderer.armorSlotType == slotType))
            {
                bodyRenderer.bodyRenderer.enabled = false;
            }
        }

        private void ShowBodyPart(string slotType)
        {
            foreach (var bodyRenderer in bodyRenderers.Where(bodyRenderer => bodyRenderer.armorSlotType == slotType))
            {
                bodyRenderer.bodyRenderer.enabled = true;
            }
        }

        public void HideWeapon(int weaponID)
        {
            switch (weaponID)
            {
                case 1:
                {
                    if (weapon1GO != null)
                    {
                        Destroy(weapon1GO);
                        weapon1RPGItem = null;
                    }

                    break;
                }
                case 2:
                {
                    if (weapon2GO != null)
                    {
                        Destroy(weapon2GO);
                        weapon2RPGItem = null;
                    }

                    break;
                }
            }
        }

        public void UpdateWeaponStates(bool inCombat)
        {
            if (weapon1GO != null)
            {
                if (inCombat)
                {
                    if (weapon1RPGItem.weaponType == "SHIELD")
                    {
                        weapon1GO.transform.SetParent(ShieldCombatSlot);
                    }
                    else
                    {
                        weapon1GO.transform.SetParent(weapon1RPGItem.slotType == "TWO HAND"
                            ? TwoHandWeaponCombatSlot
                            : OneHandWeapon1CombatSlot);
                    }
                }
                else
                {
                    if (weapon1RPGItem.weaponType == "SHIELD")
                    {
                        weapon1GO.transform.SetParent(ShieldRestSlot);
                    }
                    else
                    {
                        weapon1GO.transform.SetParent(weapon1RPGItem.slotType == "TWO HAND"
                            ? TwoHandWeaponRestSlot
                            : OneHandWeapon1RestSlot);
                    }
                }
                SetWeaponPosition(weapon1GO, weapon1RPGItem, inCombat, true);
            }
            
            if (weapon2GO != null)
            {
                if (weapon2RPGItem.weaponType == "SHIELD")
                {
                    weapon2GO.transform.SetParent(inCombat ? ShieldCombatSlot : ShieldRestSlot);
                }
                else
                {
                    weapon2GO.transform.SetParent(inCombat ? OneHandWeapon2CombatSlot : OneHandWeapon2RestSlot);
                }

                SetWeaponPosition(weapon2GO, weapon2RPGItem, inCombat, false);
                
            }
        }

        void SetWeaponPosition(GameObject go, RPGItem weaponItem, bool inCombat, bool mainHand)
        {
            Vector3[] weaponPositionData = getWeaponPositionData(weaponItem, inCombat, mainHand);
            go.transform.localPosition = weaponPositionData[0];
            go.transform.localRotation = Quaternion.Euler(weaponPositionData[1]);
            go.transform.localScale = weaponPositionData[2];
        }

        Vector3[] getWeaponPositionData(RPGItem weaponItem, bool inCombat, bool mainHand)
        {
            Vector3[] weaponPositionData = new Vector3[3];
            weaponPositionData[2] = Vector3.one;

            foreach (var t in weaponItem.weaponPositionDatas)
            {
                if (t.raceID != CharacterData.Instance.raceID) continue;
                foreach (var t1 in t.genderPositionDatas)
                {
                    if (t1.gender != CharacterData.Instance.gender) continue;
                    if (inCombat)
                    {
                        weaponPositionData[0] = mainHand ? t1.CombatPositionInSlot : t1.CombatPositionInSlot2;
                        weaponPositionData[1] = mainHand ? t1.CombatRotationInSlot : t1.CombatRotationInSlot2;
                        weaponPositionData[2] = mainHand ? t1.CombatScaleInSlot : t1.CombatScaleInSlot2;
                    }
                    else
                    {
                        weaponPositionData[0] = mainHand ? t1.RestPositionInSlot : t1.RestPositionInSlot2;
                        weaponPositionData[1] = mainHand ? t1.RestRotationInSlot : t1.RestRotationInSlot2;
                        weaponPositionData[2] = mainHand ? t1.RestScaleInSlot : t1.RestScaleInSlot2;
                    }
                }
            }

            return weaponPositionData;
        }

        private void ParentWeaponToSlot(GameObject weaponGO, int weaponID, string slotType, RPGItem weaponItem)
        {
            if (CombatManager.playerCombatNode == null)
            {
                if (weaponID == 1)
                {
                    if (weaponItem.weaponType == "SHIELD")
                    {
                        weaponGO.transform.SetParent(ShieldRestSlot);
                    }
                    else
                    {
                        weaponGO.transform.SetParent(slotType == "TWO HAND"
                            ? TwoHandWeaponRestSlot
                            : OneHandWeapon1RestSlot);
                    }

                    SetWeaponPosition(weaponGO, weaponItem, false, true);
                }
                else
                {
                    weaponGO.transform.SetParent(weaponItem.weaponType == "SHIELD"
                        ? ShieldRestSlot
                        : OneHandWeapon2RestSlot);

                    SetWeaponPosition(weaponGO, weaponItem, false, false);
                }
            }
            else
            {
                if (CombatManager.playerCombatNode.inCombat)
                {
                    if (weaponID == 1)
                    {
                        if (weaponItem.weaponType == "SHIELD")
                        {
                            weaponGO.transform.SetParent(ShieldRestSlot);
                        }
                        else
                        {
                            weaponGO.transform.SetParent(slotType == "TWO HAND"
                                ? TwoHandWeaponCombatSlot
                                : OneHandWeapon1CombatSlot);
                        }

                        SetWeaponPosition(weaponGO, weaponItem, true, true);
                    }
                    else
                    {
                        weaponGO.transform.SetParent(OneHandWeapon2CombatSlot);

                        SetWeaponPosition(weaponGO, weaponItem, true, false);
                    }
                }
                else
                {
                    if (weaponID == 1)
                    {
                        if (weaponItem.weaponType == "SHIELD")
                        {
                            weaponGO.transform.SetParent(ShieldRestSlot);
                        }
                        else
                        {
                            weaponGO.transform.SetParent(slotType == "TWO HAND"
                                ? TwoHandWeaponRestSlot
                                : OneHandWeapon1RestSlot);
                        }

                        SetWeaponPosition(weaponGO, weaponItem, false, true);
                    }
                    else
                    {
                        weaponGO.transform.SetParent(weaponItem.weaponType == "SHIELD"
                            ? ShieldRestSlot
                            : OneHandWeapon2RestSlot);

                        SetWeaponPosition(weaponGO, weaponItem, false, false);
                    }
                }
            }
        }

        private Coroutine temporaryWeaponCoroutine;

        public void ShowWeaponsTemporarily(float duration)
        {
            if (temporaryWeaponCoroutine != null)
            {
                StopCoroutine(temporaryWeaponCoroutine);
            }
            
            temporaryWeaponCoroutine = StartCoroutine(HandleTemporaryWeapons(duration));
        }

        private IEnumerator HandleTemporaryWeapons(float duration)
        {
            UpdateWeaponStates(true);
            yield return new WaitForSeconds(duration);
            if(!CombatManager.playerCombatNode.inCombat) UpdateWeaponStates(false);
        }

        public void HandleAnimatorOverride()
        {
            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            if (raceREF.dynamicAnimator)
            {
                anim.runtimeAnimatorController = CombatManager.playerCombatNode.inCombat
                    ? raceREF.combatAnimatorController
                    : raceREF.restAnimatorController;
            }
            else
            {
                anim.runtimeAnimatorController = cachedAnimatorController;
            }

            RuntimeAnimatorController newAnimatorController = RPGBuilderUtilities.getNewWeaponAnimatorOverride();
            if (newAnimatorController != null)
            {
                hasActiveWeaponAnimatorOverride = true;
                anim.runtimeAnimatorController = newAnimatorController;
            }

            //anim.Rebind();
        }

        public void ShowWeapon(RPGItem weaponItem, int weaponID, GameObject meshManager)
        {
            if (weaponItem == null) return;
            switch (weaponID)
            {
                case 1:
                {
                    var newWeaponGO = Instantiate(weaponItem.weaponModel, transform.position, Quaternion.identity);
                    ParentWeaponToSlot(newWeaponGO, weaponID, weaponItem.slotType, weaponItem);
                    if (weapon1GO != null) Destroy(weapon1GO);

                    weapon1GO = newWeaponGO;
                    weapon1RPGItem = weaponItem;
                    if (weaponItem.modelMaterial != null)
                    {
                        foreach (var Renderer in newWeaponGO.GetComponentsInChildren<Renderer>())
                        {
                            Renderer.material = weaponItem.modelMaterial;
                        }
                    }
                    if(meshManager !=null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, newWeaponGO);
                    break;
                }
                case 2:
                {
                    var newWeaponGO = Instantiate(weaponItem.weaponModel, transform.position, Quaternion.identity);
                    ParentWeaponToSlot(newWeaponGO, weaponID, weaponItem.slotType, weaponItem);
                    if (weapon2GO != null) Destroy(weapon2GO);

                    weapon2GO = newWeaponGO;
                    weapon2RPGItem = weaponItem;
                    if (weaponItem.modelMaterial != null)
                    {
                        foreach (var Renderer in newWeaponGO.GetComponentsInChildren<Renderer>())
                        {
                            Renderer.material = weaponItem.modelMaterial;
                        }
                    }
                    if(meshManager !=null) EnchantingManager.Instance.ApplyEnchantParticle(meshManager, newWeaponGO);
                    break;
                }
            }
        }
        
        public void HandleBodyScaleFromStats()
        {
            float bodyScaleModifier = 1;

            foreach (var stat in CombatManager.playerCombatNode.nodeStats)
            {
                if(stat.stat==null) continue;
                foreach (var bonus in stat.stat.statBonuses)
                {
                    if(bonus.statType != RPGStat.STAT_TYPE.BODY_SCALE) continue;
                    bodyScaleModifier += bonus.modifyValue * stat.curValue;
                }
            }
            InitBodyScale(bodyScaleModifier);
        }

        public void InitBodyScale(float bodyScaleModifier)
        {
            if (armatureParentGO == null) return;
            foreach (var armatureRef in armatureReferences)
            {
                float bodyScale = 1 * bodyScaleModifier;
                armatureRef.transform.localScale = new Vector3(bodyScale, bodyScale, bodyScale);
            }

            armatureParentGO.transform.localPosition = SceneManager.GetActiveScene().name == RPGBuilderEssentials.Instance.generalSettings.mainMenuSceneName ? new Vector3(0, bodyScaleModifier - 1, 0) : new Vector3(0+armatureParentOffset.x, (bodyScaleModifier - 1)+armatureParentOffset.y, 0+armatureParentOffset.z);
        }

    }
}
