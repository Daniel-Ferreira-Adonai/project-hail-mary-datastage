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

	private Tween _lowHpPulse;

	public void Setup(int maxHp, int currentHp)
	{
		if(!isPlayer)
		{
			energyCoin.QueueFree();
		}
		MaxValue = maxHp;
		Value = currentHp;
		UpdateLowHpPulse(currentHp, maxHp);
	}

	public void UpdateHp(int currentHp)
	{
		Value = currentHp;
		UpdateLowHpPulse(currentHp, (int)MaxValue);
	}

	public void updateLabels(int currentHp, int maxHp, int block)
	{
		healthLabel.Text = currentHp.ToString() + "/" + maxHp.ToString();
		blockLabel.Text = block.ToString();
		UpdateLowHpPulse(currentHp, maxHp);
	}

	private void UpdateLowHpPulse(int current, int max)
	{
		bool low = max > 0 && current > 0 && (float)current / max <= 0.25f;
		if (low && _lowHpPulse is null)
		{
			_lowHpPulse = CreateTween().SetLoops()
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			_lowHpPulse.TweenProperty(this, "modulate", new Color(1f, 0.55f, 0.55f), 0.5f);
			_lowHpPulse.TweenProperty(this, "modulate", Colors.White, 0.5f);
		}
		else if (!low && _lowHpPulse is not null)
		{
			_lowHpPulse.Kill(); _lowHpPulse = null; Modulate = Colors.White;
		}
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
