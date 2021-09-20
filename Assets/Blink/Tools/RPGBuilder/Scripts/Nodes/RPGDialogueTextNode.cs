using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using XNode;

[NodeWidth(360)]
[NodeTint(46, 46, 46)]
[CreateNodeMenu("Dialogue Text Node")]
public class RPGDialogueTextNode : Node
{
	[Input] public RPGDialogueTextNode previousNode;
	[Output] public RPGDialogueBaseNode nextNodes;

	public enum IdentityType { NPC, Player }
	[NodeEnum] public IdentityType identityType;

	public string nodeName = "";
	public string message = "";

	public bool hasGameActions;
	public List<RPGBGameActions> GameActionsList = new List<RPGBGameActions>();
	
	public bool hasRequirements;
	public List<RequirementsManager.RequirementDATA> RequirementList = new List<RequirementsManager.RequirementDATA>();

	public bool showSettings;
	public bool viewedEndless = true, clickedEndless = true;
	public int viewCountMax, clickCountMax;

	public bool editorInitialized;

	public Sprite nodeImage;

	// GetValue should be overridden to return a value for any specified output port
	public override object GetValue(NodePort port) {

		// Get new a and b values from input connections. Fallback to field values if input is not connected
		RPGDialogueTextNode previousNode = GetInputValue<RPGDialogueTextNode>("previousNode", this.previousNode);

		// After you've gotten your input values, you can perform your calculations and return a value
		return previousNode;
	}
}