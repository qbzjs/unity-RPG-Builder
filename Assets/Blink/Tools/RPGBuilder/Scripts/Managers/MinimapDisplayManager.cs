using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class MinimapDisplayManager : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public RectTransform mapContainer, playerArrow, panel;
        public Image minimapImage;
        public TextMeshProUGUI regionName;

        public Transform playerTransform;
        public float mapScale = 0.1f;

        public bool Initialized;

        private RPGGameScene curGameScene;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static MinimapDisplayManager Instance { get; private set; }

        public void InitializeMinimap(RPGGameScene gameSceneREF)
        {
            curGameScene = gameSceneREF;
            minimapImage.sprite = gameSceneREF.minimapImage;
            Initialized = true;
            regionName.text = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name).displayName;
            RPGBuilderUtilities.EnableCG(thisCG);
        }

        private void Update()
        {
            if (Initialized) UpdateMinimap();
        }

        private void UpdateMinimap()
        {
            if (playerTransform == null)
            {
                if (CombatManager.playerCombatNode != null) playerTransform = CombatManager.playerCombatNode.transform;
                return;
            }

            playerArrow.transform.rotation = Quaternion.Euler(panel.transform.eulerAngles.x, panel.transform.eulerAngles.y,
                -playerTransform.eulerAngles.y);

            var unitScale = GetMapUnitScale();
            var posOffset = curGameScene.mapBounds.center - playerTransform.position;
            var mapPos = new Vector3(posOffset.x * unitScale.x, posOffset.z * unitScale.y, 0f) * mapScale;

            mapContainer.localPosition = new Vector2(mapPos.x, mapPos.y);
            mapContainer.localScale = new Vector3(mapScale, mapScale, 1f);
        }

        private Vector2 GetMapUnitScale()
        {
            return new Vector2(curGameScene.mapSize.x / curGameScene.mapBounds.size.x,
                curGameScene.mapSize.y / curGameScene.mapBounds.size.z);
        }
    }
}