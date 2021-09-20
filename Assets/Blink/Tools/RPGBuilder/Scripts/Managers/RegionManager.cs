using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.World;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.Managers
{
    public class RegionManager : MonoBehaviour
    {
        public static RegionManager Instance { get; private set; }
        private RPGGameScene.REGION_DATA previousRegionData;

        public float FogTransitionSpeed = 0.5f;
        public float LightTransitionSpeed = 0.5f;
        public float SkyboxTransitionSpeed = 0.5f;
        
        
        private bool isFogUpdating;
        private Color fogColorTarget;
        private float fogStartDistanceTarget, fogEndDistanceTarget, fogDensityTarget;
        
        
        private bool isLightUpdating;
        private Color lightColorTarget;
        private float lightIntensityTarget;
        private Light currentLight;
        
        private bool isSkyboxUpdating;
        private Material currentSkybox;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public IEnumerator InitializeDefaultRegion()
        {
            yield return new WaitForSeconds(0.25f);
            RPGGameScene.REGION_DATA currentPlayerRegion = getPlayerRegion();
            if (currentPlayerRegion != null)
            {
                MinimapDisplayManager.Instance.regionName.text = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name).displayName;
				EnterRegion(currentPlayerRegion.regionName);
                yield break;
            }
            // Player is not inside any region
            // Check if there is a "Default" region setup for this game scene

            RPGGameScene.REGION_DATA defaultRegion = getDefaultRegion();
            if (defaultRegion == null) yield break;
            EnterRegion(defaultRegion.regionName);
        }

        private RPGGameScene.REGION_DATA getRegionDataByName(RPGGameScene gameScene, string regionName)
        {
            foreach (var region in gameScene.regions)
            {
                if (region.regionName == regionName) return region;
            }

            return null;
        }
        private class RegionCheckingData
        {
            public Region regionREF;
            public RPGGameScene.REGION_DATA RegionData;
            public Collider collider;
        }
        public RPGGameScene.REGION_DATA getPlayerRegion()
        {
            List<RegionCheckingData> allRegions = new List<RegionCheckingData>();
            RPGGameScene currentGameSceneREF = RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name);
            
            Region[] regionsRefs = FindObjectsOfType<Region>();

            foreach (var regionRef in regionsRefs)
            {
                RegionCheckingData newRegionCheckData = new RegionCheckingData();
                newRegionCheckData.regionREF = regionRef;
                newRegionCheckData.collider = regionRef.GetComponent<Collider>();
                newRegionCheckData.RegionData = getRegionDataByName(currentGameSceneREF, regionRef.regionName);

                if (newRegionCheckData.collider != null && newRegionCheckData.RegionData != null)
                {
                    allRegions.Add(newRegionCheckData);
                }
            }

            List<RegionCheckingData> regionsWithoutPlayer = new List<RegionCheckingData>();
            foreach (var regionChecked in allRegions)
            {
                if (regionChecked.regionREF.shapeType == Region.RegionShapeType.Cube)
                {
                    if (!ColliderContainsPoint(regionChecked.collider.transform, CombatManager.playerCombatNode.transform.position))
                    {
                        regionsWithoutPlayer.Add(regionChecked);
                    }
                }
                else if (regionChecked.regionREF.shapeType == Region.RegionShapeType.Sphere)
                {
                    if (!PointInSphere(CombatManager.playerCombatNode.transform.position, regionChecked.collider.transform.position, regionChecked.collider.transform.localScale.y))
                    {
                        regionsWithoutPlayer.Add(regionChecked);
                    }
                }
            }
            
            foreach (var t in regionsWithoutPlayer)
            {
                allRegions.Remove(t);
            }

            if (allRegions.Count == 0)
            {
                return getDefaultRegion();
            }

            RPGGameScene.REGION_DATA currentRegion = null;
            float curBiggestExtent = 0;
            foreach (var validRegion in allRegions)
            {
                var bounds = validRegion.collider.bounds;
                float extentSize = bounds.extents.x +
                                   bounds.extents.y +
                                   bounds.extents.z;
                if (!(extentSize > curBiggestExtent)) continue;
                curBiggestExtent = extentSize;
                currentRegion = validRegion.RegionData;
            }

            return currentRegion;
        }

        private RPGGameScene.REGION_DATA getDefaultRegion()
        {
            RPGGameScene currentGameSceneREF =
                RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name);

            foreach (var region in currentGameSceneREF.regions)
            {
                if (region.regionName == "Default")
                {
                    return region;
                }
            }

            return null;
        }

        public void EnterRegion(string regionName)
        {
            RPGGameScene currentGameSceneREF =
                RPGBuilderUtilities.GetGameSceneFromName(SceneManager.GetActiveScene().name);

            foreach (var region in currentGameSceneREF.regions)
            {
                if(region.regionName != regionName) continue;
                if(region == previousRegionData) continue;
                
                MinimapDisplayManager.Instance.regionName.text = region.regionName == "Default" ? currentGameSceneREF.displayName : region.regionName;

                if (region.welcomeText)
                {
                    TriggerWelcomeMessage(region);
                }
                if (region.fogChange)
                {
                    TriggerFogChange(region);
                }
                if (region.lightningChange)
                {
                    TriggerLightningChange(region);
                }
                if (region.skyboxChange)
                {
                    TriggerSkyboxChange(region);
                }
                if (region.musicChange)
                {
                    TriggerMusicChange(region);
                }
                
                FadePreviousRegionCameraParticles();
                if (region.cameraParticleChange)
                {
                    TriggerCameraParticleChange(region);
                }
                
                if (region.combatModeChange)
                {
                    TriggerCombatModeChange(region);
                }
                else
                {
                    CombatManager.Instance.combatEnabled = true;
                }
                
                if (region.combatStateChange)
                {
                    TriggerCombatStateChange(region);
                }
                else
                {
                    // TODO handle combat change again
                }
                
                if (region.gameActions)
                {
                    GameActionsManager.Instance.TriggerGameActions(region.GameActionsList);
                }

                previousRegionData = region;
            }
        }
        
        public void ExitRegion()
        {
            RPGGameScene.REGION_DATA currentPlayerRegion = getPlayerRegion();

            if (currentPlayerRegion != null)
            {
                EnterRegion(currentPlayerRegion.regionName);
            }
            else
            {
                // Player is not inside any region
                // Check if there is a "Default" region setup for this game scene
                
                RPGGameScene.REGION_DATA defaultRegion = getDefaultRegion();
                if (defaultRegion != null)
                {
                    EnterRegion(defaultRegion.regionName);
                }
            }
        }

        private void TriggerWelcomeMessage(RPGGameScene.REGION_DATA regionData)
        {
            RegionMessageDisplayManager.Instance.ShowRegionMessage(regionData.welcomeMessageText, regionData.welcomeMessageDuration);
        }

        private void TriggerFogChange(RPGGameScene.REGION_DATA regionData)
        {
            RenderSettings.fog = regionData.fogEnabled;
            if (!regionData.fogEnabled) return;
            FogTransitionSpeed = regionData.fogTransitionSpeed;
            RenderSettings.fogMode = regionData.fogMode;
            fogColorTarget = regionData.fogColor;
            if (regionData.fogMode == FogMode.Linear)
            {
                fogStartDistanceTarget = regionData.fogStartDistance;
                fogEndDistanceTarget = regionData.fogEndDistance;
            }
            else
            {
                fogDensityTarget = regionData.fogDensity;
            }

            isFogUpdating = true;
        }

        private void TriggerLightningChange(RPGGameScene.REGION_DATA regionData)
        {
            GameObject lightGO = GameObject.Find(regionData.lightGameobjectName);
            if (lightGO == null) return;
            Light light = lightGO.GetComponent<Light>();
            if (light == null) return;
            currentLight = light;
            light.enabled = regionData.lightEnabled;
            if (!regionData.lightEnabled) return;
            LightTransitionSpeed = regionData.lightTransitionSpeed;
            RenderSettings.fogMode = regionData.fogMode;
            lightColorTarget = regionData.lightColor;
            lightIntensityTarget = regionData.lightIntensity;
            isLightUpdating = true;
        }

        private void TriggerSkyboxChange(RPGGameScene.REGION_DATA regionData)
        {
            Material skybox = RenderSettings.skybox;
            if (skybox == null) return;
            currentSkybox = skybox;
            SkyboxTransitionSpeed = regionData.skyboxTransitionSpeed;
            skybox.SetTexture("_Tex_Blend", regionData.skyboxCubemap);
            skybox.SetFloat("_CubemapTransition", 0);
            isSkyboxUpdating = true;
        }

        private void TriggerMusicChange(RPGGameScene.REGION_DATA regionData)
        {
            if (regionData.musicClips.Count == 0) return;
            MusicManager.Instance.HandleMusicFadeCoroutine(regionData.musicClips[MusicManager.Instance.GETRandomMusicIndex(0, regionData.musicClips.Count)]);
        }

        private void TriggerCameraParticleChange(RPGGameScene.REGION_DATA regionData)
        {
            if (Camera.main == null) return;
            BlendCameraParticle(regionData);
        }

        private IEnumerator FadeParticleSystem(ParticleSystem particle)
        {
            var particleMain = particle.main;
            particleMain.loop = false;
            yield return new WaitForSeconds(particleMain.startLifetime.constantMax + 10f);
            if(particle != null) Destroy(particle.gameObject);
        }

        private void FadePreviousRegionCameraParticles()
        {
            foreach (var particle in Camera.main.gameObject.GetComponentsInChildren<CameraParticleObject>())
            {
                StartCoroutine(FadeParticleSystem(particle.GetComponent<ParticleSystem>()));
            }
        }
        
        private void BlendCameraParticle (RPGGameScene.REGION_DATA regionData)
        {
            if (regionData.cameraParticle == null) return;
            GameObject newCameraParticle = Instantiate(regionData.cameraParticle, Vector3.zero,
                regionData.cameraParticle.transform.rotation);
            newCameraParticle.transform.SetParent(Camera.main.transform);
            newCameraParticle.transform.localPosition = Vector3.zero;
        }

        private void TriggerCombatModeChange(RPGGameScene.REGION_DATA regionData)
        {
            CombatManager.Instance.combatEnabled = regionData.combatEnabled;
            if(!regionData.combatEnabled) CombatManager.Instance.HandleTurnOffCombat();
        }

        private void TriggerCombatStateChange(RPGGameScene.REGION_DATA regionData)
        {
            CombatManager.Instance.inCombatOverriden = true;
            if (regionData.inCombat)
            {
                
                if (RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState) CombatManager.Instance.HandleCombatAction(CombatManager.playerCombatNode);
            }
            else
            {
                if (RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState)CombatManager.Instance.ResetCombat(CombatManager.playerCombatNode);
            }
        }

        
        
        bool ColliderContainsPoint(Transform ColliderTransform, Vector3 Point)
        {
            Vector3 localPos = ColliderTransform.InverseTransformPoint(Point);
            return Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f;
        }
        public bool PointInSphere(Vector3 pnt, Vector3 sphereCenter, float sphereRadius)
        {
            return (sphereCenter - pnt).magnitude < sphereRadius;
        }
        
        private void Update()
        {
            if (isFogUpdating)
            {
                HandleFogTransition();
            }
            if (isLightUpdating)
            {
                HandleLightTransition();
            }
            if (isSkyboxUpdating)
            {
                HandleSkyboxTransition();
            }
        }

        private void HandleFogTransition()
        {
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColorTarget, Time.deltaTime * FogTransitionSpeed);
            if (RenderSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, fogEndDistanceTarget,
                    Time.deltaTime * FogTransitionSpeed);
                RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, fogStartDistanceTarget,
                    Time.deltaTime * FogTransitionSpeed);
            }
            else
            {
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, fogDensityTarget,
                    Time.deltaTime * FogTransitionSpeed);
            }

            if (!fogTransitionIsDone()) return;
            isFogUpdating = false;
            RenderSettings.fogColor = fogColorTarget;
            RenderSettings.fogEndDistance = fogEndDistanceTarget;
            RenderSettings.fogStartDistance = fogStartDistanceTarget;
            RenderSettings.fogDensity = fogDensityTarget;
        }

        private bool fogTransitionIsDone()
        {
            if (RenderSettings.fogMode == FogMode.Linear)
            {
                if (almostMatching(RenderSettings.fogEndDistance, fogEndDistanceTarget, 0.00001f) &&
                    almostMatching(RenderSettings.fogStartDistance, fogStartDistanceTarget, 0.00001f))
                {
                    return true;
                }
            }
            else
            {
                if (almostMatching(RenderSettings.fogDensity, fogDensityTarget, 0.00001f))
                {
                    return true;
                }
            }

            return false;
        }
        private bool lightTransitionIsDone()
        {
            return almostMatching(currentLight.intensity, lightIntensityTarget, 0.001f);
        }
        
        private bool almostMatching(float value1, float value2, float treshold)
        {
            return Mathf.Abs(value1 - value2) <= treshold;
        }

        private void HandleLightTransition()
        {
            if (currentLight == null) return;
            currentLight.color = Color.Lerp(currentLight.color, lightColorTarget, Time.deltaTime * LightTransitionSpeed);
            currentLight.intensity = Mathf.Lerp(currentLight.intensity, lightIntensityTarget, Time.deltaTime * LightTransitionSpeed);

            if (!lightTransitionIsDone()) return;
            isLightUpdating = false;
            currentLight.color = lightColorTarget;
            currentLight.intensity = lightIntensityTarget;
        }

        private void HandleSkyboxTransition()
        {
            if (currentSkybox == null) return;
            float lerpValue = Mathf.Lerp(currentSkybox.GetFloat("_CubemapTransition"), 1f, Time.deltaTime * SkyboxTransitionSpeed);
            currentSkybox.SetFloat("_CubemapTransition", lerpValue);

            if (!almostMatching(currentSkybox.GetFloat("_CubemapTransition"), 1, 0.05f)) return;
            isSkyboxUpdating = false;
            currentSkybox.SetTexture("_Tex", currentSkybox.GetTexture("_Tex_Blend"));
            currentSkybox.SetFloat("_CubemapTransition", 0);
        }
    }
}
