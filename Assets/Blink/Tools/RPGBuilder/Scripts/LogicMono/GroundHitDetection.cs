using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.LogicMono
{
    public class GroundHitDetection : MonoBehaviour
    {
        public RPGAbility ability;
        private float radius;
        private int hitCount;
        private float intervalBetweenHits;
        private float activationDelay;
    
        private int cachedMaxUnitHit;
        private RPGAbility.RPGAbilityRankData rankREF;

        private CombatNode ownerNode;
        
        void InitValues()
        {
            cachedMaxUnitHit = rankREF.MaxUnitHit + (int)CombatManager.Instance.GetTotalOfStatType(ownerNode, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
        }
        
        public void InitGroundAbility(CombatNode owner, float destroyDuration, RPGAbility ABILITY)
        {
            ownerNode = owner;
            Destroy(gameObject, destroyDuration);

            ability = ABILITY;
            var curRank = RPGBuilderUtilities.getAbilityRank(ability.ID);
            rankREF = ability.ranks[curRank];

            radius = rankREF.groundRadius;
            hitCount = rankREF.groundHitCount;
            intervalBetweenHits = rankREF.groundHitInterval;
            activationDelay = rankREF.groundHitTime;
            InitValues();
            StartCoroutine(StartGroundHit(activationDelay));
        }

        private IEnumerator StartGroundHit (float activationDelay)
        {
            var curRank = RPGBuilderUtilities.getAbilityRank(ability.ID);
            rankREF = ability.ranks[curRank];
            yield return new WaitForSeconds(activationDelay);

            for (var i = 0; i < hitCount; i++)
            {
                List<CombatNode> allCbtNodes = CombatManager.Instance.getPotentialCombatNodes(Physics.OverlapSphere(transform.position, radius), ownerNode, rankREF);

                var closestUnits = getClosestUnits(allCbtNodes, cachedMaxUnitHit);
                foreach (var t in closestUnits)
                {
                    CombatManager.Instance.EXECUTE_GROUND_ABILITY_HIT(ownerNode, t, ability, rankREF);
                }
                if (hitCount > 1)
                    yield return new WaitForSeconds(intervalBetweenHits);
                else
                    yield break;
            }
        }

        private List<CombatNode> getClosestUnits(List<CombatNode> allCbtNodes, int maxUnitHit)
        {
            var closestUnits = new List<CombatNode>();
            var allDistances = new List<float>();

            foreach (var t in allCbtNodes)
                if (allDistances.Count >= maxUnitHit)
                {
                    if(t == null) continue;
                    if (t.dead) continue;
                    var dist = Vector3.Distance(transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t;
                }
                else
                {
                    if(t == null) continue;
                    if (t.dead) continue;
                    allDistances.Add(Vector3.Distance(transform.position, t.transform.position));
                    closestUnits.Add(t);
                }

            return closestUnits;
        }
    
    }
}
