using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class MeshParticleManager : MonoBehaviour
    {

        public ParticleSystem[] particles;

        public void Init(GameObject target)
        {
            foreach (var t in particles)
            {
                var shapeModule = t.shape;
                MeshRenderer meshRef = target.GetComponent<MeshRenderer>();
                if (meshRef != null)
                {
                    shapeModule.shapeType = ParticleSystemShapeType.MeshRenderer;
                    shapeModule.meshRenderer = meshRef;
                }
                SkinnedMeshRenderer skinnedMeshRef = target.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRef != null)
                {
                    shapeModule.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                    shapeModule.skinnedMeshRenderer = skinnedMeshRef;
                }
            }
        }
    }
}
