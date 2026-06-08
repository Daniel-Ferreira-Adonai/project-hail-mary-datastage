using Godot;
using System;
using System.Collections.Generic;
using System.Data;

public partial class Player : Node2D
{
	
	[Export] private int _startingMaxHp = 75;
	private int _maxHp;
	[Export] public int MaxHp
	{
		get => _maxHp;
		set
		{
			_maxHp = value;
			GameManager.Instance?.UpdateTopBar();
			UpdateLabelValues();
		}
	}
	public Dictionary<string, int> Debuffs = new();
	private int _currentHp;
	[Export] public int currentHp
	{
		get => _currentHp;
		set
		{
			_currentHp = value;
			GameManager.Instance?.UpdateTopBar();
			UpdateLabelValues();
		}
	}
	[Export] public EffectBar EffectBar;
	[Export] public PlayerEnum playerEnum {get; set;}
	public List<PowerData> ActivePowers { get; set; } = new List<PowerData>();
	[Export] private int _startingGold = 100;
	private int _gold;
	public int Gold
	{
		get => _gold;
		set
		{
			_gold = value;
			GameManager.Instance?.UpdateTopBar();
		}
	}
	[Export] private int _BlockValue;
	[Export] public int BlockValue
	{
		get => _BlockValue;
		set
		{
			_BlockValue = value;
			if (value > 0) _blockSound?.Play();
			UpdateLabelValues();
		}
	}


	
	
	private List<CardData> _BaseDeck = new List<CardData>();  

	public List<RelicData> Relics { get; set; } = new();
	public int BonusCardsToDraw { get; set; } = 0;
	public bool NextDebuffDoubled { get; set; } = false;
	[Export] public BarraDeVida _hpBar;

	public bool _isAttacking = false;
	[Export] public int ImpactFrame = 4;

	private Vector2 _baseAnimScale;
	private Tween _idleTween;
	private bool _useSquashStretch = false;
	private Tween   _hitTween;
	private Vector2 _hitRestPos;
	private bool    _hitResting;
	[Export] private float _attackLungeDistance = 150f;
	[Export] private float _seuZeBarNudge = 18f;
	private Tween   _lungeTween;
	private Vector2 _attackBasePos;
	private Enemy   _attackTarget;


 	[Signal]
    public delegate void StatsChangedEventHandler();
	private int _TemporaryDexterity = 0;

	
	[Export] public int TemporaryDexterity
    {
        get => _TemporaryDexterity;
        set
        {
            _TemporaryDexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _temporaryStrength = 0;

	[Export] public int TemporaryStrength
    {
        get => _temporaryStrength;
        set
        {
            _temporaryStrength = value;
            if (value > 0) _strengthSound?.Play();
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _perCombatTemporaryStrength = 0;

	[Export] public int PerCombatTemporaryStrength
    {
        get => _perCombatTemporaryStrength;
        set
        {
            _perCombatTemporaryStrength = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _perCombatTemporarydexterity = 0;
    [Export] public int PerCombatTemporaryDexterity
    {
        get => _perCombatTemporarydexterity;
        set
        {
            _perCombatTemporarydexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _dexterity = 0;
    [Export] public int Dexterity
    {
        get => _dexterity;
        set
        {
            _dexterity = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	private int _strength = 0;
    [Export] public int Strength
    {
        get => _strength;
        set
        {
            _strength = value;
            EmitSignal(SignalName.StatsChanged);
        }
    }
	public int Weak
{
    get => Debuffs.ContainsKey("Weak") ? Debuffs["Weak"] : 0;
    set => Debuffs["Weak"] = value;
}

public int Frail
{
    get => Debuffs.ContainsKey("Frail") ? Debuffs["Frail"] : 0;
    set => Debuffs["Frail"] = value;
}
	[Export] public AnimatedSprite2D animation;
	[Export] private PackedScene _damageLabelScene;

	private AudioStreamPlayer _attackSound;
	private AudioStreamPlayer _slashSound;
	private AudioStreamPlayer _blockSound;
	private AudioStreamPlayer _strengthSound;

	[Signal]
	public delegate void AttackImpactEventHandler();

	private bool isAlive;


		public override void _Ready()
	{
		ApplyCharacterData();
		_baseAnimScale = animation.Scale;
		if (_useSquashStretch)
			StartIdleBreathing();
		setupBasicDeck();
		PlayerManager.Instance.Player = this;
		SetupHpBar();
		GD.Print($"[HPBar] parent={_hpBar?.GetParent()?.Name} scale={_hpBar?.Scale} " +
		         $"pos={_hpBar?.Position} global={_hpBar?.GlobalPosition} " +
		         $"anchors=({_hpBar?.AnchorTop},{_hpBar?.AnchorBottom}) " +
		         $"offsets=({_hpBar?.OffsetTop},{_hpBar?.OffsetBottom}) " +
		         $"visible={_hpBar?.Visible} size={_hpBar?.Size}");
		UpdateLabelValues();
		SetupEffectBar();
		_attackSound  = GetNodeOrNull<AudioStreamPlayer>("AttackSound");
		_slashSound   = GetNodeOrNull<AudioStreamPlayer>("SlashSound");
		_blockSound   = GetNodeOrNull<AudioStreamPlayer>("BlockSound");
		_strengthSound = GetNodeOrNull<AudioStreamPlayer>("StrengthSound");

		var relicFiles = DirAccess.GetFilesAt("res://Data/Relics/");
		foreach (var file in relicFiles)
		{
			if (file.EndsWith(".tres"))
			{
				var relic = GD.Load<RelicData>($"res://Data/Relics/{file}");
				if (relic != null)
					Relics.Add(relic);
			}
		}
	}
private void SetupEffectBar()
{
    if (EffectBar == null) return;
    // Container de 300px centrado acima da cabeça do player.
    // Scale do Player (~3.83, 3.57) seria herdado pelo EffectBar,
    // então já corrigimos com scale inverso no .tscn. Aqui só
    // posicionamos: -150 world / scale.X = left edge, alignment=Center faz o resto.
    EffectBar.CustomMinimumSize = new Vector2(300, 40);
    EffectBar.Position = new Vector2(-150f / Scale.X, -65f);
}

public void AddPower(PowerData power)
{
    ActivePowers.Add(power);
    EmitSignal(SignalName.StatsChanged);
}
	private void ApplyCharacterData()
	{
		_useSquashStretch = true;
		var character = RunData.SelectedCharacter;
		if (character is not null)
		{
			_maxHp        = character.StartingMaxHp;
			_currentHp    = character.StartingMaxHp;
			Gold          = character.StartingGold;

			if (character.IdleSprite != null || character.AttackSprite != null)
				ApplyCharacterSprites(character);
		}
		else
		{
			_maxHp     = _startingMaxHp;
			_currentHp = _startingMaxHp;
			Gold       = _startingGold;
		}
	}

	private void ApplyCharacterSprites(CharacterData character)
	{
		var idleTex   = character.IdleSprite   ?? character.AttackSprite;
		var attackTex = character.AttackSprite ?? character.IdleSprite;

		var frames = new SpriteFrames();
		frames.RemoveAnimation("default");

		frames.AddAnimation("idle");
		frames.SetAnimationLoop("idle", true);
		frames.SetAnimationSpeed("idle", 2.0f);
		frames.AddFrame("idle", idleTex);
		frames.AddFrame("idle", idleTex);

		frames.AddAnimation("attack");
		frames.SetAnimationLoop("attack", false);
		frames.SetAnimationSpeed("attack", 12.0f);
		for (int i = 0; i < 7; i++)
			frames.AddFrame("attack", attackTex);

		animation.SpriteFrames = frames;
		animation.Scale *= character.SpriteScale;
		_useSquashStretch = true;
		animation.Play("idle");

		if (_hpBar is not null)
		{
			_hpBar.OffsetTop    += _seuZeBarNudge;
			_hpBar.OffsetBottom += _seuZeBarNudge;
		}
	}

private void StartIdleBreathing()
{
	_idleTween?.Kill();
	if (_baseAnimScale == Vector2.Zero) return;

	_idleTween = CreateTween().SetLoops(0);
	// Inspira: espreme levemente na horizontal, cresce na vertical
	_idleTween.TweenProperty(animation, "scale",
		new Vector2(_baseAnimScale.X * 0.97f, _baseAnimScale.Y * 1.04f), 0.9f)
		.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	// Expira: volta ao normal
	_idleTween.TweenProperty(animation, "scale",
		_baseAnimScale, 0.9f)
		.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
}

private void SetupHpBar()
{
    if (_hpBar == null) return;
    _hpBar.Setup(MaxHp, currentHp);
    _hpBar.Visible = true;
}
	public override void _Process(double delta)
	{
		
	}
		public void TriggerRelics(Action<RelicData> hook)
	{
		foreach (var relic in Relics)
			hook(relic);
	}
	public void AddRelic(RelicData relic)
{
    Relics.Add(relic);
    RelicBar.Instance?.LoadRelics(Relics);
}
	public void CalculateDamageTaken(int enemyDamage)
	{	
		TriggerRelics(r => r.OnTakeDamage(this, ref enemyDamage)); 

		int damageToHp = Math.Max(0, enemyDamage - BlockValue);		
		
		BlockValue = Math.Max(0, BlockValue - enemyDamage);		
		currentHp -= damageToHp;
		if(currentHp <= 0)
		{
			isAlive = false;
			return;
		}
		UpdateLabelValues();
		SpawnDamageLabel(damageToHp);

	}
	public void TakeDamage(int damage)
{
    currentHp -= damage;
    
    if (currentHp <= 0)
    {
        currentHp = 0;
        isAlive = false;
    }
    
    SpawnDamageLabel(damage);
    UpdateLabelValues();
}
public void TriggerPowers(Action<PowerData> trigger)
{
    foreach (var power in new System.Collections.Generic.List<PowerData>(ActivePowers))
        trigger(power);
}
private void SpawnDamageLabel(int damage)
{
    if (_damageLabelScene == null) return;
    
    var label = _damageLabelScene.Instantiate<DamageLabel>();
    AddChild(label); // ← filho do Player
    label.Visible = true;
    label.ZIndex = 100;
    label.Position = new Vector2(0, -50); 
    label.Setup(damage);
}
	public void UpdateLabelValues()
{
    
    _hpBar?.Setup(MaxHp, currentHp);
    _hpBar?.updateLabels(currentHp, MaxHp, BlockValue);
	    EffectBar?.UpdateEffects(GetAllEffects()); // 🔥 AQUI

}
public void UpdateTemporaryValues()
{
    _temporaryStrength = 0;
    _TemporaryDexterity = 0;
    BlockValue = 0;

    ApplyDebuffEffects();
}

private void ApplyDebuffEffects()
{
    var keys = new List<string>(Debuffs.Keys);
    foreach (var key in keys)
    {
        if (key == "Sangria" && Debuffs[key] > 0)
        {
            TakeDamage(Debuffs[key]);

            var enemies = GetTree().GetNodesInGroup("enemies");
            foreach (var node in enemies)
            {
                if (node is Enemy enemy)
                    enemy.CurrentHealth += Debuffs[key];
            }
        }

        if (Debuffs[key] > 0) Debuffs[key]--;
        if (Debuffs[key] <= 0) Debuffs.Remove(key);
    }
}
	public void UpdatePerCombatTemporaryValues()
	{
		_perCombatTemporarydexterity = 0;
		_perCombatTemporaryStrength = 0;
	}
	  public void ReceiveGold(int amount)
    {
        Gold += amount;
    }
	  public bool SpendGold(int amount)
    {
        if(Gold < amount) return false;
        Gold -= amount;
        return true;
    }
	public List<CardData> GetDeck() => _BaseDeck;
	public void AddCardToDeck(CardData cardData)
	{
		_BaseDeck.Add(cardData);
		GameManager.Instance?.UpdateTopBar();
	}
	public bool RemoveCardFromDeck(CardData cardData)
	{
		if (cardData is null) return false;

		bool removed = _BaseDeck.Remove(cardData);
		if (removed)
		{
			GameManager.Instance?.UpdateTopBar();
		}

		return removed;
	}
	public void ApplyDebuff(string debuff, int value)
{
    if (!Debuffs.ContainsKey(debuff))
        Debuffs[debuff] = 0;

    Debuffs[debuff] += value;
    EmitSignal(SignalName.StatsChanged);
}
	public CardData UpgradeRandomCard()
{
    var nonUpgraded = _BaseDeck.FindAll(cardData => !cardData.IsCardUpgraded);
    
    if (nonUpgraded.Count == 0) return null;
    
    int random = (int)GD.RandRange(0, nonUpgraded.Count - 1);
	CardData cardData = nonUpgraded[random]; 
	cardData.IsCardUpgraded = true;
    return cardData;
}
public int GetTotalStrength()
{
    return Strength + TemporaryStrength + PerCombatTemporaryStrength;
}

public List<(EffectData data, int value)> GetAllEffects()
{
    var list = new List<(EffectData, int)>();

    if (Weak > 0)
    {
        var data = EffectManager.Instance.GetEffect("weak");
        if (data != null)
            list.Add((data, Weak));
    }

    if (Frail > 0)
    {
        var data = EffectManager.Instance.GetEffect("frail");
        if (data != null)
            list.Add((data, Frail));
    }

    int totalStrength = GetTotalStrength();
    if (totalStrength > 0)
    {
        var data = EffectManager.Instance.GetEffect("strength");
        if (data != null)
            list.Add((data, totalStrength));
    }

    int totalDex = GetTotalDexterity();
    if (totalDex > 0)
    {
        var data = EffectManager.Instance.GetEffect("dexterity");
        if (data != null)
            list.Add((data, totalDex));
    }

    var grouped = new Dictionary<string, int>();

    foreach (var p in ActivePowers)
    {
        if (!grouped.ContainsKey(p.Id))
            grouped[p.Id] = 0;

        grouped[p.Id]++;
    }

    foreach (var kvp in grouped)
    {
        var data = EffectManager.Instance.GetEffect(kvp.Key);
        if (data != null)
            list.Add((data, kvp.Value));
    }

    return list;
}
public int GetTotalDexterity()
{
    return Dexterity + TemporaryDexterity + PerCombatTemporaryDexterity;
}
public CardData RemoveRandomCard()
{
    if (_BaseDeck.Count == 0) return null;
    
    int random = GD.RandRange(0, _BaseDeck.Count - 1);
    CardData cardData = _BaseDeck[random];
    return RemoveCardFromDeck(cardData) ? cardData : null;
}
	public void setupBasicDeck()
	{
		RunStats.Reset();
		if (RunData.SelectedCharacter?.CharacterName == "Seu Zé")
		{
			SetupSeuZeDeck();
			return;
		}
		SetupCorvoDeck();
	}

	private void SetupCorvoDeck()
	{
		CardData strikeData = GD.Load<CardData>("res://Data/Cards/StrikeCard.tres");
        CardData defendData = GD.Load<CardData>("res://Data/Cards/BlockCard.tres");
        CardData blockVunarable = GD.Load<CardData>("res://Data/Cards/BlockVunarable.tres");
		CardData SangriaCard = GD.Load<CardData>("res://Data/Cards/SangriaCard.tres");
		CardData CorteHemolitico = GD.Load<CardData>("res://Data/Cards/CorteHemoliticoCard.tres");
		CardData DefesaSangue = GD.Load<CardData>("res://Data/Cards/DefesaSanguessugaCard.tres");
		CardData Hemorragia = GD.Load<CardData>("res://Data/Cards/Hemorragia.tres");
		CardData PactoDeSangue = GD.Load<CardData>("res://Data/Cards/PactoDeSangueCard.tres");
		CardData sedeDeSangue = GD.Load<CardData>("res://Data/Cards/SedeDeSangueCard.tres");
		CardData coagualcao = GD.Load<CardData>("res://Data/Cards/CoagulacaoCard.tres");

		for(int i = 0; i < 3; i++)
		{
			AddCardToDeck((CardData)strikeData.Duplicate());
			AddCardToDeck((CardData)defendData.Duplicate());
			AddCardToDeck((CardData)blockVunarable.Duplicate());
			AddCardToDeck((CardData)SangriaCard.Duplicate());
			AddCardToDeck((CardData)CorteHemolitico.Duplicate());
			AddCardToDeck((CardData)sedeDeSangue.Duplicate());
			AddCardToDeck((CardData)DefesaSangue.Duplicate());
			AddCardToDeck((CardData)Hemorragia.Duplicate());
			AddCardToDeck((CardData)PactoDeSangue.Duplicate());
			AddCardToDeck((CardData)coagualcao.Duplicate());
		}
	}

	private void SetupSeuZeDeck()
	{
		// básicas: Mangada×3 (ataque), MangaRosa×4 (defesa)
		// extras:  Pau×2, Leite×2, Salada×3, Madura×2, Cajado×2, Facão×2  (=20 total)
		var mangada = GD.Load<CardData>("res://Data/Cards/Ze/Mangada.tres");
		var rosa    = GD.Load<CardData>("res://Data/Cards/Ze/MangaRosa.tres");
		var pau     = GD.Load<CardData>("res://Data/Cards/Ze/PauDeCatarManga.tres");
		var leite   = GD.Load<CardData>("res://Data/Cards/Ze/MangaComLeite.tres");
		var salada  = GD.Load<CardData>("res://Data/Cards/Ze/SaladaDeFruta.tres");
		var madura  = GD.Load<CardData>("res://Data/Cards/Ze/MangaMadura.tres");
		var cajado  = GD.Load<CardData>("res://Data/Cards/Ze/CajadoDoLampiao.tres");
		var facao   = GD.Load<CardData>("res://Data/Cards/Ze/FacaoDoFeirante.tres");

		if (mangada is null || rosa is null || pau is null || leite is null ||
		    salada is null || madura is null || cajado is null || facao is null)
		{
			GD.PushError("SetupSeuZeDeck: falha ao carregar uma ou mais cartas do Seu Zé — verifique os .tres em Data/Cards/Ze/");
			SetupCorvoDeck();
			return;
		}

		ApplySeuZeArt(mangada);
		ApplySeuZeArt(rosa);
		ApplySeuZeArt(pau);
		ApplySeuZeArt(leite);
		ApplySeuZeArt(salada);
		ApplySeuZeArt(madura);
		ApplySeuZeArt(cajado);
		ApplySeuZeArt(facao);

		for (int i = 0; i < 3; i++) AddCardToDeck((CardData)mangada.Duplicate());
		for (int i = 0; i < 4; i++) AddCardToDeck((CardData)rosa.Duplicate());
		for (int i = 0; i < 2; i++) AddCardToDeck((CardData)pau.Duplicate());
		for (int i = 0; i < 2; i++) AddCardToDeck((CardData)leite.Duplicate());
		for (int i = 0; i < 3; i++) AddCardToDeck((CardData)salada.Duplicate());
		for (int i = 0; i < 2; i++) AddCardToDeck((CardData)madura.Duplicate());
		for (int i = 0; i < 2; i++) AddCardToDeck((CardData)cajado.Duplicate());
		for (int i = 0; i < 2; i++) AddCardToDeck((CardData)facao.Duplicate());
	}

	public static void ApplySeuZeArt(CardData card)
	{
		if (card is null) return;
		string path = card.GetType().Name switch
		{
			"MangadaCard"         => "res://Test/CardArtsZe/mangada.png",
			"MangaRosaCard"       => "res://Test/CardArtsZe/manga_rosa.png",
			"PauDeCatarMangaCard" => "res://Test/CardArtsZe/pau_de_catar_manga.png",
			"MangaComLeiteCard"   => "res://Test/CardArtsZe/manga_com_leite.png",
			"SaladaDeFrutaCard"   => "res://Test/CardArtsZe/salada_de_fruta.png",
			"MangaMaduraCard"     => "res://Test/CardArtsZe/manga_madura.png",
			"CajadoDoLampiaoCard" => "res://Test/CardArtsZe/cajado_do_lampiao.png",
			"FacaoDoFeiranteCard" => "res://Test/CardArtsZe/facao_do_feirante.png",
			_                     => null,
		};
		if (path is null) return;
		var tex = GD.Load<Texture2D>(path);
		if (tex is not null) card.Art = tex;
	}
	public void TryToHeal(int healValue)
	{
		if(this._currentHp == MaxHp) 
		return;

		if(this._currentHp + healValue >= MaxHp)
		{
			currentHp = MaxHp;
			return;
		}
		this.currentHp += healValue;
	}


public async void PlayAttackAnimation(Enemy target = null)
{
    if (_isAttacking) return;
    _isAttacking = true;
    _slashSound?.Play();
    _idleTween?.Kill();

    if (target is null || !IsInstanceValid(target))
        target = CombatManager.Instance?.GetFirstEnemy();
    _attackTarget = target;

    _attackBasePos = Position;
    Vector2 dir = (target is not null && IsInstanceValid(target))
        ? (target.GlobalPosition - GlobalPosition).Normalized()
        : Vector2.Right;
    Vector2 lungePos = _attackBasePos + dir * _attackLungeDistance;

    if (_useSquashStretch)
    {
        var anticipation = CreateTween().SetParallel();
        anticipation.TweenProperty(animation, "scale",
            new Vector2(_baseAnimScale.X * 0.88f, _baseAnimScale.Y * 1.12f), 0.08f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        anticipation.TweenProperty(this, "position",
            _attackBasePos - dir * 24f, 0.08f)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        await ToSignal(anticipation, Tween.SignalName.Finished);
    }

    _lungeTween?.Kill();
    _lungeTween = CreateTween();
    _lungeTween.TweenProperty(this, "position", lungePos, 0.12f)
        .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

    animation.Play("attack");
    animation.FrameChanged += OnAttackFrameChanged;
    await ToSignal(animation, AnimatedSprite2D.SignalName.AnimationFinished);
    animation.FrameChanged -= OnAttackFrameChanged;

    var bounce = CreateTween().SetParallel();
    if (_useSquashStretch)
        bounce.TweenProperty(animation, "scale", _baseAnimScale, 0.38f)
            .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    bounce.TweenProperty(this, "position", _attackBasePos, 0.38f)
        .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    await ToSignal(bounce, Tween.SignalName.Finished);

    animation.Play("idle");
    if (_useSquashStretch)
        StartIdleBreathing();
    _isAttacking = false;
    _attackTarget = null;
}
	public void PlayHitReaction(Vector2 pushDir)
	{
		if (_hitResting) return;
		_hitResting = true;
		_hitRestPos = animation.Position;

		_hitTween?.Kill();
		_hitTween = CreateTween();
		_hitTween.TweenProperty(animation, "position",
			_hitRestPos + pushDir * 18f, 0.06f)
			.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_hitTween.TweenProperty(animation, "position",
			_hitRestPos, 0.28f)
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		_hitTween.TweenCallback(Callable.From(() => _hitResting = false));
	}

	public CardData DuplicateRandomCard()
	{
		if (_BaseDeck.Count == 0) return null;

		var rng = new Random();
		var card = _BaseDeck[rng.Next(_BaseDeck.Count)];
		AddCardToDeck(card);
		return card;
	}
public (CardData removed, CardData added) TransformRandomCard()
{
    if (_BaseDeck.Count == 0) return (null, null);

    var rng = new Random();
    int index = rng.Next(_BaseDeck.Count);
    CardData removed = _BaseDeck[index];
    _BaseDeck.RemoveAt(index);

    var allCards = new List<CardData>();
    var files = DirAccess.GetFilesAt("res://Data/Cards/");
    foreach (var file in files)
    {
        if (file.EndsWith(".tres"))
        {
            var card = GD.Load<CardData>($"res://Data/Cards/{file}");
            if (card != null)
                allCards.Add(card);
        }
    }

    if (allCards.Count == 0) return (removed, null);

    CardData added = allCards[rng.Next(allCards.Count)];
    AddCardToDeck(added);
    return (removed, added);
}
private void OnAttackFrameChanged()
{
    if (animation.Animation == "attack" && animation.Frame == ImpactFrame)
    {
        _attackSound?.Play();
        EmitSignal(SignalName.AttackImpact);

        if (_attackTarget is not null && IsInstanceValid(_attackTarget))
        {
            Vector2 dir = (_attackTarget.GlobalPosition - GlobalPosition).Normalized();
            _attackTarget.PlayHitReaction(dir);
        }
        CombatManager.Instance?.ShakeScreen(7f, 0.18f);
    }
}
}
