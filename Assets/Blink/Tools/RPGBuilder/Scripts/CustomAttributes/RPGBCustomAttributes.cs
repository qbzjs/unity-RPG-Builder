using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]

public class BaseCustomAttribute : Attribute {}
 
public class IDAttribute : BaseCustomAttribute {}
 
public class AbilityIDAttribute : IDAttribute {}
public class EffectIDAttribute : IDAttribute {}
public class NPCIDAttribute : IDAttribute {}
public class StatIDAttribute : IDAttribute {}
public class PointIDAttribute : IDAttribute {}
public class SpellbookIDAttribute : IDAttribute {}
public class FactionIDAttribute : IDAttribute {}
public class WeaponTemplateIDAttribute : IDAttribute {}
public class SpeciesIDAttribute : IDAttribute {}
public class ComboIDAttribute : IDAttribute {}
public class ItemIDAttribute : IDAttribute {}
public class SkillIDAttribute : IDAttribute {}
public class LevelTemplateIDAttribute : IDAttribute {}
public class RaceIDAttribute : IDAttribute {}
public class ClassIDAttribute : IDAttribute {}
public class LootTableIDAttribute : IDAttribute {}
public class MerchantTableIDAttribute : IDAttribute {}
public class CurrencyIDAttribute : IDAttribute {}
public class RecipeIDAttribute : IDAttribute {}
public class CraftingStationIDAttribute : IDAttribute {}
public class TalentTreeIDAttribute : IDAttribute {}
public class BonusIDAttribute : IDAttribute {}
public class GearSetIDAttribute : IDAttribute {}
public class EnchantmentIDAttribute : IDAttribute {}
public class TaskIDAttribute : IDAttribute {}
public class QuestIDAttribute : IDAttribute {}
public class CoordinateIDAttribute : IDAttribute {}
public class ResourceNodeIDAttribute : IDAttribute {}
public class GameSceneIDAttribute : IDAttribute {}
public class DialogueIDAttribute : IDAttribute {}
public class GameModifierIDAttribute : IDAttribute {}
 
public class RPGDataListAttribute : BaseCustomAttribute {}
 