using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Character;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerAppearanceHandler))]
public class PlayerAppearanceHandlerEditor : Editor
{
    
    private PlayerAppearanceHandler REF;
    private RPGItemDATA itemSettings;
    private RPGGeneralDATA generalSettings;
    private RPGBuilderEditorDATA editorDATA;
    
    private void OnEnable()
    {
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        itemSettings = Resources.Load<RPGItemDATA>(editorDATA.RPGBDatabasePath +"Settings/ItemSettings");
        generalSettings = Resources.Load<RPGGeneralDATA>(editorDATA.RPGBDatabasePath +"Settings/GeneralSettings");
        REF = (PlayerAppearanceHandler) target;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((PlayerAppearanceHandler) target),
            typeof(PlayerAppearanceHandler),
            false);
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        GUILayout.Space(5);
        GUILayout.Label("Body", SubTitleStyle);
        GUILayout.Space(5);
        REF.cachedBodyParent =
            (GameObject) EditorGUILayout.ObjectField("Body Parent", REF.cachedBodyParent, typeof(GameObject),
                true);
        
        GUILayout.Space(5);
        GUILayout.Label("Armor Pieces", SubTitleStyle);
        GUILayout.Space(5);

        var tps2 = serializedObject.FindProperty("armorPieces");
        EditorGUILayout.PropertyField(tps2, true);
        
        REF.cachedArmorsParent =
            (GameObject) EditorGUILayout.ObjectField("Armors Parent", REF.cachedArmorsParent, typeof(GameObject),
                true);
        
        GUILayout.Space(5);
        GUILayout.Label("Armature References", SubTitleStyle);
        GUILayout.Space(5);

        var tps3 = serializedObject.FindProperty("armatureReferences");
        EditorGUILayout.PropertyField(tps3, true);
        REF.armatureParentGO =
            (GameObject) EditorGUILayout.ObjectField("Armature Parent GameObject", REF.armatureParentGO, typeof(GameObject),
                true);
        REF.armatureParentOffset =
            EditorGUILayout.Vector3Field("Armature Parent Offset", REF.armatureParentOffset);

        GUILayout.Space(5);
        GUILayout.Label("Weapon Slots", SubTitleStyle);
        GUILayout.Space(5);
        REF.OneHandWeapon1CombatSlot =
            (Transform) EditorGUILayout.ObjectField("Main Hand: COMBAT", REF.OneHandWeapon1CombatSlot,
                typeof(Transform), true);
        REF.OneHandWeapon1RestSlot =
            (Transform) EditorGUILayout.ObjectField("Main Hand: REST", REF.OneHandWeapon1RestSlot, typeof(Transform),
                true);
        REF.OneHandWeapon2CombatSlot =
            (Transform) EditorGUILayout.ObjectField("Off Hand: COMBAT", REF.OneHandWeapon2CombatSlot, typeof(Transform),
                true);
        REF.OneHandWeapon2RestSlot =
            (Transform) EditorGUILayout.ObjectField("Off Hand: REST", REF.OneHandWeapon2RestSlot, typeof(Transform),
                true);
        REF.TwoHandWeaponCombatSlot =
            (Transform) EditorGUILayout.ObjectField("Two Handed: COMBAT", REF.TwoHandWeaponCombatSlot,
                typeof(Transform), true);
        REF.TwoHandWeaponRestSlot =
            (Transform) EditorGUILayout.ObjectField("Two Handed: REST", REF.TwoHandWeaponRestSlot, typeof(Transform),
                true);
        REF.ShieldCombatSlot =
            (Transform) EditorGUILayout.ObjectField("Shield: COMBAT", REF.ShieldCombatSlot, typeof(Transform),
                true);
        REF.ShieldRestSlot =
            (Transform) EditorGUILayout.ObjectField("Shield: REST", REF.ShieldRestSlot, typeof(Transform),
                true);


        GUILayout.Space(5);
        GUILayout.Label("Body Parts", SubTitleStyle);
        GUILayout.Space(5);

        REF.useBodyParts = EditorGUILayout.Toggle("Hide Body Parts?", REF.useBodyParts);
        if (REF.useBodyParts)
        {
            if (GUILayout.Button("+ Add Body Renderer"))
                REF.bodyRenderers.Add(new PlayerAppearanceHandler.BodyRenderer());

            var ThisList = serializedObject.FindProperty("bodyRenderers");
            REF.bodyRenderers = GetTargetObjectOfProperty(ThisList) as List<PlayerAppearanceHandler.BodyRenderer>;

            for (var a = 0; a < REF.bodyRenderers.Count; a++)
            {
                GUILayout.Space(10);
                var requirementNumber = a + 1;
                EditorGUILayout.BeginHorizontal();

                GUIStyle xButtonStyle = new GUIStyle();
                xButtonStyle.normal.textColor = Color.red;
                xButtonStyle.fontSize = 25;
                if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    REF.bodyRenderers.RemoveAt(a);
                    return;
                }

                EditorGUILayout.LabelField("" + requirementNumber + ":", GUILayout.Width(25));

                EditorGUILayout.BeginVertical();
                var index5 = getIndexFromName("ArmorSlot",
                    REF.bodyRenderers[a].armorSlotType);
                var tempIndex = EditorGUILayout.Popup("Armor Slot", index5, itemSettings.armorSlotsList.ToArray());
                if (itemSettings.armorSlotsList.Count > 0)
                    REF.bodyRenderers[a].armorSlotType = itemSettings.armorSlotsList[tempIndex];
                REF.bodyRenderers[a].bodyRenderer = (Renderer) EditorGUILayout.ObjectField("Body Renderer",
                    REF.bodyRenderers[a].bodyRenderer, typeof(Renderer), true);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        GUILayout.Space(5);
        GUILayout.Label("Armor Renderers", SubTitleStyle);
        GUILayout.Space(5);

        if (GUILayout.Button("+ Add Armor Renderer")) REF.armorRenderers.Add(new PlayerAppearanceHandler.ArmorRenderer());

        var ThisList2 = serializedObject.FindProperty("armorRenderers");
        REF.armorRenderers = GetTargetObjectOfProperty(ThisList2) as List<PlayerAppearanceHandler.ArmorRenderer>;

        for (var a = 0; a < REF.armorRenderers.Count; a++)
        {
            GUILayout.Space(10);
            var requirementNumber = a + 1;
            EditorGUILayout.BeginHorizontal();

            GUIStyle xButtonStyle = new GUIStyle();
            xButtonStyle.normal.textColor = Color.red;
            xButtonStyle.fontSize = 25;
            if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                REF.armorRenderers.RemoveAt(a);
                return;
            }

            EditorGUILayout.LabelField("" + requirementNumber + ":", GUILayout.Width(25));

            EditorGUILayout.BeginVertical();
            var index5 = getIndexFromName("ArmorSlot",
                REF.armorRenderers[a].armorSlotType);
            var tempIndex = EditorGUILayout.Popup("Armor Slot", index5, itemSettings.armorSlotsList.ToArray());
            if (itemSettings.armorSlotsList.Count > 0)
                REF.armorRenderers[a].armorSlotType = itemSettings.armorSlotsList[tempIndex];
            REF.armorRenderers[a].armorRenderer = (SkinnedMeshRenderer) EditorGUILayout.ObjectField("Renderer Reference",
                REF.armorRenderers[a].armorRenderer, typeof(SkinnedMeshRenderer), true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        if (generalSettings.useOldController)
        {
            GUILayout.Space(5);
            GUILayout.Label("Built In Controller References", SubTitleStyle);
            GUILayout.Space(5);
            REF.HeadTransformSocket = (Transform) EditorGUILayout.ObjectField("Transform",
                REF.HeadTransformSocket, typeof(Transform), true);
        }
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(REF);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(REF, "Modified Player Appearance Handler" + REF.gameObject.name);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(REF);
        }
    }

    private int getIndexFromName(string dataType, string curName)
    {
        switch (dataType)
        {
            case "ArmorSlot":
            case "armorSlot":
                for (var i = 0; i < itemSettings.armorSlotsList.Count; i++)
                    if (itemSettings.armorSlotsList[i] == curName) return i;
                return 0;

            default:
                return -1;
        }
    }
    
    private object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }

        return obj;
    }
    private object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext()) return null;
        return enm.Current;
    }
}
