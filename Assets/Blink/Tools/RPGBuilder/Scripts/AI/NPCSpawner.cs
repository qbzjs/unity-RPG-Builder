using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class NPCSpawner : MonoBehaviour
    {
    
        public AILogic.SpawnerType spawnerType;
        public int spawnCount;

        [Serializable]
        public class NPC_SPAWN_DATA
        {
            public RPGNpc npc;
            public float spawnChance = 100;
        }
        public List<NPC_SPAWN_DATA> spawnData = new List<NPC_SPAWN_DATA>();

        public int npcCountMax = 1;
        public List<CombatNode> curNPCs = new List<CombatNode>();

        private bool hasSpawnedOnce;
        private int curSpawnedCount;

        public Mesh spawnerGizmoMesh;
        public float areaRadius = 10f, areaHeight = 20f;
        public Color gizmoColor = Color.yellow;
        public LayerMask groundLayers;

        public bool usePosition;
        private void Start()
        {
            for (int i = 0; i < npcCountMax; i++)
            {
                StartCoroutine(ExecuteSpawner(0));
            }
        }

        private void Awake()
        {
            CombatManager.Instance.allNPCSpawners.Add(this);
        }

        private void OnDestroy()
        {
            if (RPGBuilderEssentials.Instance.getCurrentScene().name ==
                RPGBuilderEssentials.Instance.generalSettings.mainMenuSceneName) return;
            CombatManager.Instance.RemoveSpawnerFromList(this);
        }

        public IEnumerator ExecuteSpawner (float delay)
        {
            switch (spawnerType)
            {
                case AILogic.SpawnerType.Count:
                    if (curSpawnedCount >= spawnCount) yield break;
                
                    yield return new WaitForSeconds(delay);
                    if(curNPCs.Count >= npcCountMax) yield break;
                    SpawnNPC();
                    curSpawnedCount++;
                    break;

                case AILogic.SpawnerType.Endless:
                    yield return new WaitForSeconds(delay);
                    if(curNPCs.Count >= npcCountMax) yield break;
                    SpawnNPC();
                    break;
                case AILogic.SpawnerType.Manual:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        RPGNpc PickRandomNPC()
        {
            float rdmNPC = Random.Range(0f, 100f);
            float offset = 0;
            foreach (var t in spawnData)
            {
                if (rdmNPC >= 0 + offset && rdmNPC <= t.spawnChance + offset)
                {
                    RPGNpc npc = t.npc;
                    return npc;
                }
                offset += t.spawnChance;
            }

            return null;
        }

        private void SpawnNPC ()
        {
            RPGNpc pickedNPC = PickRandomNPC();
            if (pickedNPC == null) return;
            
            CombatNode newNPC = CombatManager.Instance.SetupNPCPrefab(pickedNPC, false, false, null, GetNPCPosition(), transform.rotation);
            newNPC.spawnerREF = this;
            curNPCs.Add(newNPC);
        }

        private Vector3 GetNPCPosition()
        {
            if (usePosition) return transform.position;
            
            Vector3 worldPos = transform.position;
            Vector3 spawnPos =
                new Vector3(Random.Range(worldPos.x - areaRadius, worldPos.x + areaHeight), transform.position.y + areaRadius,
                    Random.Range(worldPos.z - areaRadius, worldPos.z + areaRadius));
            
            if (Physics.Raycast(spawnPos, -transform.up, out var hit, areaHeight, groundLayers))
            {
                spawnPos = hit.point;
            }
            else
            {
                spawnPos = transform.position;
                Debug.LogWarning("Spawn Point could not be found, transform position was used instead." +
                               "Make sure that the NPC Spawner is placed correctly and has the right ground layers assigned");
            }

            return spawnPos;
        }
    
        public void ManualSpawnNPC()
        {
            if (curNPCs.Count >= npcCountMax)
            {
                Destroy(curNPCs[0].gameObject);
                curNPCs.RemoveAt(0);
            }

            RPGNpc pickedNPC = PickRandomNPC();
            if (pickedNPC == null) return;
            CombatNode newNPC = CombatManager.Instance.SetupNPCPrefab(pickedNPC, false, false, null, GetNPCPosition(), transform.rotation);
            newNPC.spawnerREF = this;
            curNPCs.Add(newNPC);
        }
        private void OnDrawGizmos()
        {
            if (usePosition) return;
            Gizmos.color = gizmoColor;
            Gizmos.DrawMesh(spawnerGizmoMesh, transform.position, Quaternion.identity, new Vector3(areaRadius*2, areaHeight, areaRadius*2));
        }
    }
}
