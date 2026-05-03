using Godot;

public partial class BarraDeVida : ProgressBar
{
	[Export] Label healthLabel;
	[Export] Label blockLabel;
	// [Export] Texture2D BarWithShield;
	// [Export] Texture2D BarWithoutShield;
	// [Export] Texture2D ShieldEmpty;
	// [Export] Texture2D ShieldWithValue;
	// [Export] TextureRect _shieldIcon;
	[Export] public Panel energyCoin;
	
	[Export] bool isPlayer = true;
	public void Setup(int maxHp, int currentHp)
	{
		if(!isPlayer)
		{
			energyCoin.QueueFree();
		}
		MaxValue = maxHp;
		Value = currentHp;
	}
	
	public void UpdateHp(int currentHp)
	{
		Value = currentHp;
	}
	  public void updateLabels(int currentHp, int maxHp, int block)
	{
		healthLabel.Text = currentHp.ToString() + "/" + maxHp.ToString();
		blockLabel.Text = block.ToString();
		
	}
	//   public void updateLabels(int currentHp, int maxHp, int block)
	// {
	//     healthLabel.Text = currentHp.ToString() + "/" + maxHp.ToString();
	//     blockLabel.Text = block.ToString();

	//     bool hasBlock = block > 0;

	//     TextureOver = hasBlock ? BarWithShield : BarWithoutShield;
	// 	TintProgress = hasBlock ? new Color(0.3f, 0.6f, 1f) : new Color(1f, 1f, 1f);

	// 		if (_shieldIcon != null)
	// {
	// 	_shieldIcon.Visible = true;
	// 	_shieldIcon.Texture = hasBlock ? ShieldWithValue : ShieldEmpty;
	// 	blockLabel.Visible = hasBlock; 
	// }
	// }
	// public void animation()
	// {
	// 	    var tween = CreateTween().SetLoops();
	// tween.TweenProperty(this, "tint_progress", new Color(1f, 0.3f, 0.3f), 0.8f)
	//     .SetTrans(Tween.TransitionType.Sine)
	//     .SetEase(Tween.EaseType.InOut);
	// tween.TweenProperty(this, "tint_progress", new Color(1f, 0.6f, 0.6f), 0.8f)
	//     .SetTrans(Tween.TransitionType.Sine)
	//     .SetEase(Tween.EaseType.InOut);
	// }
}
