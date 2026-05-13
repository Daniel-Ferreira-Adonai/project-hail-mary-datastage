using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class EnemyTurn : Resource
{
	[Export] public Array<IntentData> Actions = new Array<IntentData>();
}
