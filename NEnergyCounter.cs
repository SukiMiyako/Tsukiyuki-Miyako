using Godot;

// 根节点是Control → 继承Control！和你商人的Node2D逻辑完全一样
public partial class NEnergyCounter : Control
{
	// 空实现，阻断游戏所有逻辑
	public override void _Ready()
	{
	}

	// 空实现，掐死无限循环的_Process调用
	public override void _Process(double delta)
	{
	}
}
