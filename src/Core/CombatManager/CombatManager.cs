using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class CombatManager : Node2D
{
	MouseInputTracker _mouse;

	CardManager _cardManager;
	public int maxEnergy = 3;

	public int currentEnergy = 3;

	public Label energyLabel;

	public Player Player;
	private List<Enemy> _activeEnemies = new();

    [Export] private PackedScene _rewardCardScene;

    
    private EncounterData _currentEncounterData;
	public override void _Ready()
	{
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
		energyLabel = GetNode<Sprite2D>("MoedaEnergia").GetNode<Label>("ValorEnergia");
		UpdateEnergy(maxEnergy);
		_cardManager = GetNode<CardManager>("CardManager");
		AjustBackground();
         CallDeferred(nameof(Initialize));
        
	}
    private void Initialize()
{
    Player = PlayerManager.Instance.Player;
    
    if (Player != null)
        Player.StatsChanged += OnPlayerStatsChanged;
    SetupPlayerPosition();
    
}
	private void OnPlayerStatsChanged()
	{
	_cardManager.UpdateAllCardPreviews(Player, RaycastCheckForEnemy());	
	}
	public void AjustBackground()
    {
         var bg = GetNode<Sprite2D>("Sprite2D");
    var screenSize = GetViewport().GetVisibleRect().Size;
    
    bg.Position = screenSize / 2;
    
    var textureSize = bg.Texture.GetSize();
    bg.Scale = new Vector2(
        screenSize.X / textureSize.X,
        screenSize.Y / textureSize.Y
    );
    }
    private void SetupPlayerPosition()
{
    var screenSize = GetViewport().GetVisibleRect().Size;
    

    Player.GlobalPosition = new Vector2(
        screenSize.X * 0.2f, 
        screenSize.Y * 0.45f 
    );
    
    
}
	public void EndTurn()
	{
		GD.Print("Cheguei aqui");
		currentEnergy = maxEnergy;
		_cardManager.DiscardHand();
        
        Player.TriggerRelics(r => r.OnTurnEnd(Player)); 

		ExecuteEnemyTurns();
        updateEnemyDebuffs();
		Player.UpdateTemporaryValues();
		Player.UpdateLabelValues();
		if (!IsCombatOver())
        {
            StartTurn();
        }
		
	}
    public void updateEnemyDebuffs()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        foreach (var node in enemies)
            if (node is Enemy enemy)
                enemy.UpdateTemporaryEffects();
    }
	public void StartTurn()
	{
        Player.TriggerRelics(r => r.OnTurnStart(Player)); 
        int cardsToDraw = _cardManager.cardsDrawedPerTurn + Player.BonusCardsToDraw;
        Player.BonusCardsToDraw = 0;
		_cardManager.DrawCard(cardsToDraw);
		UpdateEnergy(currentEnergy);
		_cardManager.OrganizeHand();
	}
	public void InitializeCombat(EncounterData encounter)  
    {
        _activeEnemies.Clear();
        _cardManager.SetProcessInput(true);
        currentEnergy = maxEnergy;
        UpdateEnergy(currentEnergy);
        _currentEncounterData = encounter;
        SpawnEnemies(encounter);
        StartGame();

        Player.TriggerRelics(r => r.OnCombatStart(Player)); 

    }
	public void StartGame()
	{
        _cardManager.ResetDeck();
		_cardManager.StartDeck();
		
	}
	public override void _Process(double delta)
	{
	}
	public void ExecuteEnemyTurns()
{
    var enemies = GetTree().GetNodesInGroup("enemies");
    foreach (var node in enemies)
    {
        if (node is Enemy enemy)
			{
				Array<IntentData> intents = enemy.getTurnIntents();
				foreach(IntentData intent in intents)
				{
					intent.Execute(enemy,Player);
				}
			}
    }
}
	public Enemy getEnemy(Card card)
	{
		if(!canPlayCard(card)) return null;
		Enemy enemy = RaycastCheckForEnemy();
		GD.Print(enemy);
		if(enemy is not null) {
			handleCardPlayed(card);
			return enemy;
		}
		return null;
	}
	public bool canPlayCard(Card card)
	{
		if(currentEnergy < card.Data.EnergyCost)
		{
			return false;
		}
		return true;
	}
	public void handleCardPlayed(Card card)
	{
		currentEnergy -= card.Data.EnergyCost;
		UpdateEnergy(currentEnergy);
	}
	public Enemy RaycastCheckForEnemy()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D
		{
			Position = _mouse.ScreenPosition,
			CollideWithAreas = true,
			CollisionMask = 2
		};

		var results = spaceState.IntersectPoint(parameters);
		List<Enemy> cards = new List<Enemy>();

		if (results.Count > 0)
		{
			Enemy enemy = results[0]["collider"].As<Area2D>().GetParent<Enemy>();
			return enemy;
		}
		return null;
	}
	public Player getPlayer(Card card)
	{
		if(!canPlayCard(card)) return null;
		handleCardPlayed(card);

		return Player;
	}
	public Player getPlayer()
	{
		return Player;
	}
    public static int Calculate(int baseDamage, Player player, Enemy target = null, Card card = null)
    {
        float damage = baseDamage;
        
        damage += player.Strength;
		damage += player.TemporaryStrength;
        
		if(card != null)
		{
			
		}
        if (target != null && target.Vulnerable > 0)
            damage *= 1.5f;
        
        if (player.IsWeak)
            damage *= 0.75f;
        
        return Mathf.Max(0, Mathf.FloorToInt(damage));
    }
     public static int CalculateEnemieAttack(Enemy enemy)
    {
        float damage = enemy.Strength + enemy.BuffedStrength;
        
        
        if (enemy.Weak > 0)
            damage *= 0.75f;
        
        
        
        return Mathf.Max(0, Mathf.FloorToInt(damage));
    }
    public static int CalculateBlock(int baseBlock, Player player)
    {
        float block = baseBlock + player.Dexterity + player.TemporaryDexterity;
        
        if (player.IsFrail)
            block *= 0.75f;
        
        return Mathf.Max(0, Mathf.FloorToInt(block));
    }

	public void UpdateEnergy(int energy)
	{
		energyLabel.Text = energy.ToString();
	}
private void SpawnEnemies(EncounterData encounter)
{
    if (encounter.Enemies == null || encounter.Enemies.Count == 0)
    {
        GD.PrintErr("Nenhum inimigo");
        return;
    }

    var enemyScene = GD.Load<PackedScene>("res://src/Core/Enemy/Enemy.tscn");

    Node2D enemyContainer = GetNodeOrNull<Node2D>("Enemies");
    if (enemyContainer == null)
    {
        enemyContainer = new Node2D { Name = "Enemies" };
        AddChild(enemyContainer);
    }

    var result = GenerateDefaultPositions(encounter);
    Vector2[] positions = result.positions;
    float scaleFactor = result.scaleFactor;

    for (int i = 0; i < encounter.Enemies.Count; i++)
    {
        var enemy = enemyScene.Instantiate<Enemy>();

        enemyContainer.AddChild(enemy);

        enemy.Setup(encounter.Enemies[i]);

        enemy.Scale *= scaleFactor;

        enemy.GlobalPosition = positions[i];

        enemy.AddToGroup("enemies");
        enemy.Died += OnEnemyDied;

        _activeEnemies.Add(enemy);
    }

    GD.Print($"Spawned {_activeEnemies.Count} enemies");
}

private (Vector2[] positions, float scaleFactor) GenerateDefaultPositions(EncounterData encounter)
{
    int enemyCount = encounter.Enemies.Count;
    Vector2[] positions = new Vector2[enemyCount];

    Vector2 screenSize = GetViewport().GetVisibleRect().Size;

    float baseY = screenSize.Y * 0.45f;

    EnemySize largestSize = EnemySize.Medium;
    foreach (var enemyData in encounter.Enemies)
    {
        if (enemyData.Size > largestSize)
            largestSize = enemyData.Size;
    }

    float spacing = EnemyScaler.GetIdealSpacing(largestSize);

    float[] widths = new float[enemyCount];
    for (int i = 0; i < enemyCount; i++)
    {
        var data = encounter.Enemies[i];
        widths[i] = EnemyScaler.CalculateDisplayWidth(data.Sprite, data.Size);
    }

    float totalWidth = 0f;
    for (int i = 0; i < enemyCount; i++)
    {
        totalWidth += widths[i];
    }
    totalWidth += spacing * Mathf.Max(0, enemyCount - 1);

    float maxAllowedWidth = screenSize.X * 0.45f;

    float scaleFactor = 1f;
    if (totalWidth > maxAllowedWidth)
    {
        scaleFactor = maxAllowedWidth / totalWidth;
    }

    for (int i = 0; i < enemyCount; i++)
    {
        widths[i] *= scaleFactor;
    }
    spacing *= scaleFactor;

    float rightEdge = screenSize.X - 20f;

    float totalScaledWidth = 0f;
    for (int i = 0; i < enemyCount; i++)
    {
        totalScaledWidth += widths[i];
    }
    totalScaledWidth += spacing * Mathf.Max(0, enemyCount - 1);

    float startX = rightEdge - totalScaledWidth;

    float minX = screenSize.X * 0.55f;
    startX = Mathf.Max(startX, minX);

    float currentX = startX;

    for (int i = 0; i < enemyCount; i++)
    {
        float halfWidth = widths[i] / 2f;

        float yVariation = CalculateVerticalVariation(
            i,
            enemyCount,
            encounter.Enemies[i].Size
        );

        float x = currentX + halfWidth;

        x = Mathf.Min(x, screenSize.X - halfWidth - 5f);

        positions[i] = new Vector2(
            x,
            baseY + yVariation
        );

        currentX += widths[i] + spacing;
    }

    return (positions, scaleFactor);
}


private float CalculateVerticalVariation(int index, int totalCount, EnemySize size)
{
    if (totalCount == 1)
        return 0f;

    float wave = Mathf.Sin(index * 1.2f) * 15f;
    
    float sizeOffset = size switch
    {
        EnemySize.Tiny => -10f,
        EnemySize.Small => -5f,
        EnemySize.Medium => 0f,
        EnemySize.Large => 10f,
        EnemySize.Boss => 20f,
        _ => 0f
    };
    
    float edgeOffset = 0f;
    if (totalCount > 2)
    {
        float normalizedPos = (float)index / (totalCount - 1); 
        float centerDistance = Mathf.Abs(0.5f - normalizedPos) * 2; 
        edgeOffset = centerDistance * 8f;
    }
    
    return wave + sizeOffset + edgeOffset;
}
	  private void OnEnemyDied(Enemy enemy)
    {
        GD.Print($"Inimigo {enemy.Name} morreu!");
        
        Player.TriggerRelics(r => r.OnKillEnemy(Player, enemy)); 

        _activeEnemies.Remove(enemy);
        
        if (_activeEnemies.Count == 0)
        {
            EndCombat(victory: true);
        }
    }

    public bool IsCombatOver()
    {
        if (Player.currentHp <= 0)
        {
            EndCombat(victory: false);
            return true;
        }

        if (_activeEnemies.Count == 0)
        {
            EndCombat(victory: true);
            return true;
        }

        return false;
    }

    private void EndCombat(bool victory)
    {
        GD.Print(victory ? "VITÓRIA!" : "DERROTA!");

        _cardManager.SetProcessInput(false);

        if (victory)
         {
             CallDeferred(nameof(ShowVictoryScreen));
        }
        // else
        // {
        //     CallDeferred(nameof(ShowDefeatScreen));
        // }
    }

    private async void ShowVictoryScreen()
    {
        GD.Print("Mostrando tela de vitória...");
        GD.Print($"Ouro ganho: {_currentEncounterData?.RewardGold ?? 50}");
        _cardManager.DiscardHand();
        InstantiateRewardCard();
       

    }
   private async void InstantiateRewardCard()
{
    if(_rewardCardScene == null)
    {
        GD.PrintErr("RewardCardScene não foi atribuído no Inspector!");
        return;
    }
    
    var rewardCard = _rewardCardScene.Instantiate<RewardCard>();
    UI.Instance.AddUI(rewardCard);

    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    rewardCard.SetAnchorsPreset(Control.LayoutPreset.Center);
}
}
