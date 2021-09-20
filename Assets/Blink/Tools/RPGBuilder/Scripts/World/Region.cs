using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.World
{
    public class Region : MonoBehaviour
    {
        public enum RegionShapeType
        {
            Cube,
            Sphere
        }

        public RegionShapeType shapeType;
        
        public string regionName;
        public Color sceneColor = Color.green;
        public bool showGizmo = true;
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != CombatManager.playerCombatNode.gameObject) return;
        
            RegionManager.Instance.EnterRegion(regionName);
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject != CombatManager.playerCombatNode.gameObject) return;
        
            RegionManager.Instance.ExitRegion();
        }
        
        void OnDrawGizmos()
        {
            if (!showGizmo) return;
            Gizmos.color = sceneColor;
            var transform1 = transform;
            if (shapeType == RegionShapeType.Cube)
            {
                DrawCube(transform1.position, transform1.rotation, transform1.localScale);
            }
            else
            {
                Gizmos.DrawSphere(transform1.position, transform1.localScale.y);
            }
        }
        
        public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
 
            Gizmos.matrix *= cubeTransform;
 
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
 
            Gizmos.matrix = oldGizmosMatrix;
        }
    }
}
