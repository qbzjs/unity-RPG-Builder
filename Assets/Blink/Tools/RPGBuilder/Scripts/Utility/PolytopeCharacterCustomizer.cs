using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Utility
{
    public class PolytopeCharacterCustomizer : MonoBehaviour
    {
        public Color SkinColor = new Color(191, 96, 59, 1);
        public Color EyesColor = new Color(19, 34, 13, 1);
        public Color HairColor = new Color(152, 90, 28, 1);
        public Color ScleraColor = new Color(231, 208, 208, 1);
        public Color LipsColor = new Color(212, 81, 71, 1);
        public Color ScarsColor = new Color(217, 128, 99, 1);
        public Color Metal1Color = new Color(191, 66, 19, 1);
        public Color Metal2Color = new Color(119, 119, 132, 1);
        public Color Metal3Color = new Color(112, 112, 120, 1);
        public Color Leather1Color = new Color(123, 52, 23, 1);
        public Color Leather2Color = new Color(108, 49, 23, 1);
        public Color Leather3Color = new Color(43, 12, 8, 1);
        public Color Cloth1Color = new Color(37, 72, 89, 1);
        public Color Cloth2Color = new Color(255, 0, 0, 1);
        public Color Cloth3Color = new Color(224, 168, 88, 1);
        public Color Gems1Color = new Color(96, 0, 17, 1);
        public Color Gems2Color = new Color(52, 0, 111, 1);
        public Color Gems3Color = new Color(0, 29, 3, 1);
        public Color Feathers1Color = new Color(197, 129, 129, 1);
        public Color Feathers2Color = new Color(173, 0, 0, 1);
        public Color Feathers3Color = new Color(0, 46, 185, 1);
        public Color CoatColor = new Color(255, 0, 0, 1);

        public List<SkinnedMeshRenderer> characterRenderers = new List<SkinnedMeshRenderer>();

        public bool isUpdating;

        private void Start()
        {
            InitializeMaterialsColors();
        }

        private void Update()
        {
            if (isUpdating)
                InitializeMaterialsColors();
        }

        private void InitializeMaterialsColors()
        {
            foreach (var t in characterRenderers)
            {
                t.material.SetColor("_SKINCOLOR", SkinColor);
                t.material.SetColor("_EYESCOLOR", EyesColor);
                t.material.SetColor("_HAIRCOLOR", HairColor);
                t.material.SetColor("_SCLERACOLOR", ScleraColor);
                t.material.SetColor("_LIPSCOLOR", LipsColor);
                t.material.SetColor("_SCARSCOLOR", ScarsColor);
                t.material.SetColor("_METAL1COLOR", Metal1Color);
                t.material.SetColor("_METAL2COLOR", Metal2Color);
                t.material.SetColor("_METAL3COLOR", Metal3Color);
                t.material.SetColor("_LEATHER1COLOR", Leather1Color);
                t.material.SetColor("_LEATHER2COLOR", Leather2Color);
                t.material.SetColor("_LEATHER3COLOR", Leather3Color);
                t.material.SetColor("_CLOTH1COLOR", Cloth1Color);
                t.material.SetColor("_CLOTH2COLOR", Cloth2Color);
                t.material.SetColor("_CLOTH3COLOR", Cloth3Color);
                t.material.SetColor("_GEMS1COLOR", Gems1Color);
                t.material.SetColor("_GEMS2COLOR", Gems2Color);
                t.material.SetColor("_GEMS3COLOR", Gems3Color);
                t.material.SetColor("_FEATHERS1COLOR", Feathers1Color);
                t.material.SetColor("_FEATHERS2COLOR", Feathers2Color);
                t.material.SetColor("_FEATHERS3COLOR", Feathers3Color);
                t.material.SetColor("_COATOFARMSCOLOR", CoatColor);
            }
        }
    }
}