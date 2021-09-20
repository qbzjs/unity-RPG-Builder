using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class ScreenTextObject : MonoBehaviour
    {
        public ScreenTextDisplayManager FloatingPanelController;
        public int ThisPanelID;


        public TextMeshProUGUI combatTextTMP;
        public TMP_FontAsset CombatTextFont;
        public float renderDistance = 50f;

        public Color HealColor;
        public Color MagicalDamageColor;
        public Color MagicalCriticalDamageColor;
        public Color PhysicalDamageColor;
        public Color PhysicalCriticalDamageColor;
        public Color SelfDamageColor;
        public Color NoDamageTypeColor;
        public Color ThornDamageColor;
        public Color EXPColor;
        public Color LevelUpColor;
        public Color FactionColor;
        public Color ImmuneColor;

        public float CriticalSizeUpRate;
        public float CriticalSizeDownRate;

        private GameObject MobNodeToFollow;
        private float stopDisplay;
        private float stopChatDisplay;

        public float NormalDamageDisplayTime, CriticalDmgDisplayTime;

        private readonly float initialScaleFactor = 3f;
        public float initialAlphaFactor = 3f;
        public float combatTextSpeed = 50f;
        public float combatColorSpeed = 1f;
        public float combatTextScaleDownSpeed = 3f;

        //Set automatically
        private Vector3 startPosition;
        private Vector3 startScale;
        private float currentScale = 1f;
        private float currentAlpha = 1f;
        private float currentOffset;

        public bool IsCritical, MaxCritSizedReached;
        public float ThisTextTime;

        private void Awake()
        {
            startPosition = combatTextTMP.rectTransform.localPosition;
            startScale = combatTextTMP.rectTransform.localScale;

            currentScale = initialScaleFactor;
            currentAlpha = initialAlphaFactor;
        }

        public void RunUpdate(Camera cam)
        {
            if (CombatManager.playerCombatNode == null) return;
            if (MobNodeToFollow != CombatManager.playerCombatNode.gameObject)
            {
                var worldPos = MobNodeToFollow.transform.position;
                var screenPos = cam.WorldToScreenPoint(worldPos);
                transform.position = screenPos;
            }

            UpdateCombatText();
        }

        public void SetMobDetails(GameObject mobNode)
        {
            MobNodeToFollow = mobNode;
        }

        private void ResetCombatText()
        {
            currentOffset = 0;
            currentAlpha = initialAlphaFactor;
            combatTextTMP.text = "";

            currentScale = initialScaleFactor;
            combatTextTMP.rectTransform.localScale = startScale;

            var color = combatTextTMP.color;
            color.a = currentAlpha;
            combatTextTMP.color = color;

            combatTextTMP.rectTransform.localScale = startScale * currentScale;
            IsCritical = false;
            MaxCritSizedReached = false;
            ThisTextTime = 0;
            startPosition.x = 0;
            startPosition.y = 0;
            combatTextTMP.rectTransform.localPosition = startPosition;
        }

        private void UpdateCombatText()
        {
            if (Time.time > stopDisplay)
            {
                FloatingPanelController.AllScreenTextDATA[ThisPanelID].nodeGO = null;
                FloatingPanelController.AllScreenTextDATA[ThisPanelID].RendererReference = null;
                FloatingPanelController.AllScreenTextDATA[ThisPanelID].LastTimeUsed = 0;
                FloatingPanelController.AllScreenTextDATA[ThisPanelID].ScreenTextGO.SetActive(false);

                stopDisplay = 0;

                combatTextTMP.text = "";
                return;
            }

            if (!IsCritical)
            {
                currentOffset += Time.deltaTime * combatTextSpeed;
                combatTextTMP.rectTransform.localPosition = startPosition + new Vector3(0, currentOffset, 0);


                currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime * combatColorSpeed);
                var color = combatTextTMP.color;
                color.a = currentAlpha;
                combatTextTMP.color = color;

                currentScale = Mathf.Max(1f, currentScale - Time.deltaTime * combatTextScaleDownSpeed);
                combatTextTMP.rectTransform.localScale = startScale * currentScale;
            }
            else
            {
                combatTextTMP.rectTransform.localPosition = startPosition + new Vector3(0, currentOffset, 0);
                
                currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime * (combatColorSpeed / 2));
                var color = combatTextTMP.color;
                color.a = currentAlpha;
                combatTextTMP.color = color;

                if (combatTextTMP.rectTransform.localScale.x <= 1.5f)
                {
                    if (MaxCritSizedReached) return;
                    var newscale = combatTextTMP.rectTransform.localScale.x + CriticalSizeUpRate;

                    combatTextTMP.rectTransform.localScale = new Vector3(newscale, newscale, newscale);
                }
                else
                {
                    MaxCritSizedReached = true;
                    if (combatTextTMP.rectTransform.localScale.x > 1)
                    {
                        var newscale = combatTextTMP.rectTransform.localScale.x - CriticalSizeDownRate;

                        combatTextTMP.rectTransform.localScale = new Vector3(newscale, newscale, newscale);
                    }
                }
            }
        }

        public void ShowCombatText(string message, string eventType, string blockedDMG)
        {
            ResetCombatText();

            combatTextTMP.text = message;

            ThisTextTime = Time.time;
            stopDisplay = Time.time + NormalDamageDisplayTime;

            startPosition.x = 0;
            startPosition.y = 0;

            float RdmX = Random.Range(-75, 75);
            float RdmY = Random.Range(-75, 75);

            switch (eventType)
            {
                case "PHYSICAL_DAMAGE" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = SelfDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "PHYSICAL_DAMAGE":
                    combatTextTMP.color = PhysicalDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "PHYSICAL_DAMAGE_CRITICAL":
                {
                    IsCritical = true;
                    stopDisplay = Time.time + CriticalDmgDisplayTime;

                    if (MobNodeToFollow == CombatManager.playerCombatNode.gameObject)
                    {
                        combatTextTMP.color = SelfDamageColor;
                        combatTextTMP.font = CombatTextFont;
                        combatTextTMP.fontSize = 90;
                        combatTextTMP.enableVertexGradient = false;
                        combatTextTMP.text = "- " + message;
                        stopDisplay = Time.time + 5;
                        startPosition.x = 300;
                    }
                    else
                    {
                        combatTextTMP.color = PhysicalCriticalDamageColor;
                        combatTextTMP.font = CombatTextFont;
                        combatTextTMP.fontSize = 50;
                        combatTextTMP.enableVertexGradient = false;

                        startPosition.x = RdmX;
                        startPosition.y = RdmY;
                    }

                    break;
                }
                case "MAGICAL_DAMAGE" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = SelfDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "MAGICAL_DAMAGE":
                    combatTextTMP.color = MagicalDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "MAGICAL_DAMAGE_CRITICAL":
                {
                    IsCritical = true;
                    stopDisplay = Time.time + CriticalDmgDisplayTime;

                    if (MobNodeToFollow == CombatManager.playerCombatNode.gameObject)
                    {
                        combatTextTMP.color = SelfDamageColor;
                        combatTextTMP.font = CombatTextFont;
                        combatTextTMP.fontSize = 90;
                        combatTextTMP.enableVertexGradient = false;
                        combatTextTMP.text = "- " + message;
                        stopDisplay = Time.time + 5;
                        startPosition.x = 300;
                    }
                    else
                    {
                        combatTextTMP.color = MagicalCriticalDamageColor;
                        combatTextTMP.font = CombatTextFont;
                        combatTextTMP.fontSize = 50;
                        combatTextTMP.enableVertexGradient = false;

                        startPosition.x = RdmX;
                        startPosition.y = RdmY;
                    }

                    break;
                }
                case "NO_DAMAGE_TYPE" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = SelfDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "NO_DAMAGE_TYPE":
                    combatTextTMP.color = NoDamageTypeColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "THORN" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = SelfDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message + " ( Thorn )";
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "THORN":
                    combatTextTMP.color = ThornDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "(Thorn) - " + message;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "HEAL" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = HealColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "+ " + message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = -300;
                    break;
                case "HEAL":
                    combatTextTMP.color = HealColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "HEAL_CRITICAL" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = HealColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 90;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "+ " + message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = -300;
                    break;
                case "HEAL_CRITICAL":
                    combatTextTMP.color = HealColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 50;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "EXP" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = EXPColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 30;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = message + " EXP";
                    stopDisplay = Time.time + 5;
                    startPosition.x = 0;
                    break;
                case "EXP":
                    combatTextTMP.color = EXPColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 30;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "LEVELUP" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = LevelUpColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = message;
                    stopDisplay = Time.time + 5;
                    startPosition.x = 0;
                    startPosition.y = 100;
                    break;
                case "LEVELUP":
                    combatTextTMP.color = LevelUpColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "FACTION_POINT":
                    combatTextTMP.color = FactionColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 30;
                    combatTextTMP.enableVertexGradient = false;
                    stopDisplay = Time.time + 5;

                    startPosition.x = RdmX;
                    startPosition.y = RdmY + 250;
                    break;
                case "IMMUNE" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = ImmuneColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "Immune";
                    stopDisplay = Time.time + 5;
                    startPosition.x = -300;
                    break;
                case "IMMUNE":
                    combatTextTMP.color = ImmuneColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "Immune";

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "BLOCKED" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = PhysicalDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message + " (Blocked " + blockedDMG + ")";
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "BLOCKED":
                    combatTextTMP.color = PhysicalDamageColor;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "- " + message + " (Blocked " + blockedDMG + ")";

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
                case "DODGED" when MobNodeToFollow == CombatManager.playerCombatNode.gameObject:
                    combatTextTMP.color = Color.white;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 40;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "Dodged";
                    stopDisplay = Time.time + 5;
                    startPosition.x = 300;
                    break;
                case "DODGED":
                    combatTextTMP.color = Color.white;
                    combatTextTMP.font = CombatTextFont;
                    combatTextTMP.fontSize = 25;
                    combatTextTMP.enableVertexGradient = false;
                    combatTextTMP.text = "Dodged";

                    startPosition.x = RdmX;
                    startPosition.y = RdmY;
                    break;
            }
            
            combatTextTMP.rectTransform.localPosition = startPosition;
        }
    }
}