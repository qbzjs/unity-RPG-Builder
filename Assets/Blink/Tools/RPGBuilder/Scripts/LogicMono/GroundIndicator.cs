using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.LogicMono
{
    public class GroundIndicator : MonoBehaviour
    {
        [SerializeField]
        protected bool restrictCursorToRange = false;
        [SerializeField]
        protected GroundIndicatorManager ManagerREF;
        [SerializeField]
        protected Projector projectorREF;

        [SerializeField]
        protected float range = 5f;
        [SerializeField]
        protected float scale = 7f;

        public ParticleSystem particleIndicator;

        public float Scale
        {
            get { return scale; }
            set => this.scale = value;
        }

        public float Range
        {
            get { return range; }
            set { SetRange(value); }
        }

        private void SetRange(float range)
        {
            this.range = range;
        }
    
        protected Vector3 FlattenVector(Vector3 target)
        {
            return new Vector3(target.x, ManagerREF.transform.position.y, target.z);
        }

        public void SetScale(float SCALE)
        {
            if (projectorREF.enabled)
            {
                projectorREF.orthographicSize = SCALE;
            }

            if (particleIndicator != null)
            {
                particleIndicator.Clear();
                ParticleSystem.MainModule main = particleIndicator.main;
                main.startSize = SCALE * 5.75f;
                particleIndicator.Play();
            }
        }

        private void RestrictCursorToRange()
        {
            if (ManagerREF == null) return;
            if (Vector3.Distance(ManagerREF.transform.position, transform.position) > range)
                transform.position = ManagerREF.transform.position + Vector3.ClampMagnitude(transform.position - ManagerREF.transform.position, range);
        }

        private void Update()
        {
            transform.position = ManagerREF.Get3DMousePosition();
            if (restrictCursorToRange)
                RestrictCursorToRange();
        }

        private void LateUpdate()
        {
            transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }
}
