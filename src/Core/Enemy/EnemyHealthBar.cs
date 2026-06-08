using Godot;
using System.Collections.Generic;

public partial class EnemyHealthBar : ProgressBar
{
    private Label _hpLabel;
    private Godot.Panel _blockPanel;
    private Label _blockLabel;
    private int   _maxHp;
    private float _barWidth;
    private Tween _lowHpPulse;

    private static readonly Dictionary<EnemySize, Vector2> SizeMap = new()
    {
        { EnemySize.Tiny,   new Vector2(65f,  16f) },
        { EnemySize.Small,  new Vector2(95f,  18f) },
        { EnemySize.Medium, new Vector2(150f, 20f) },
        { EnemySize.Large,  new Vector2(185f, 20f) },
        { EnemySize.Boss,   new Vector2(250f, 24f) },
    };

    public override void _Ready()
    {
        _hpLabel    = GetNode<Label>("HpLabel");
        _blockPanel = GetNode<Godot.Panel>("BlockPanel");
        _blockLabel = GetNode<Label>("BlockPanel/BlockLabel");
    }

    public void Setup(int maxHp, EnemySize size, float spriteDisplayWidth)
    {
        _maxHp   = maxHp;
        MaxValue = maxHp;
        Value    = maxHp;

        var barSize = SizeMap.GetValueOrDefault(size, new Vector2(120f, 20f));
        _barWidth = Mathf.Clamp(Mathf.Max(barSize.X, spriteDisplayWidth), 50f, 280f);
        float barH = barSize.Y;

        CustomMinimumSize = new Vector2(_barWidth, barH);
        Size              = new Vector2(_barWidth, barH);
        Position          = new Vector2(-_barWidth / 2f, Position.Y);

        // Anchor block panel to left edge of bar
        _blockPanel.Position = new Vector2(-38f, -5f);

        int fontSize = size switch
        {
            EnemySize.Tiny  => 9,
            EnemySize.Small => 10,
            EnemySize.Boss  => 13,
            _               => 11,
        };
        _hpLabel.AddThemeFontSizeOverride("font_size", fontSize);
        _blockLabel.AddThemeFontSizeOverride("font_size", fontSize);

        _hpLabel.Text = $"{maxHp}/{maxHp}";
        UpdateBlock(0);
    }

    public void UpdateHp(int currentHp)
    {
        int clamped   = Mathf.Max(0, currentHp);
        Value         = clamped;
        _hpLabel.Text = $"{clamped}/{_maxHp}";
        UpdateLowHpPulse(clamped, _maxHp);
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

    public void UpdateBlock(int block)
    {
        bool hasBlock       = block > 0;
        _blockPanel.Visible = hasBlock;
        if (hasBlock)
            _blockLabel.Text = block.ToString();
    }
}
