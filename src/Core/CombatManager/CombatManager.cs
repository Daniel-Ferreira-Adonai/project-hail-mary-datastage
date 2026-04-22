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
	public override void _Ready()
	{
		Player = GetNode<Player>("Player");
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
		energyLabel = GetNode<Sprite2D>("MoedaEnergia").GetNode<Label>("ValorEnergia");
		UpdateEnergy(maxEnergy);
		_cardManager = GetNode<CardManager>("CardManager");
		if (Player != null)
		{
			Player.StatsChanged += OnPlayerStatsChanged;
		}
		
		StartGame();
	}
	private void OnPlayerStatsChanged()
	{
	_cardManager.UpdateAllCardPreviews(Player, RaycastCheckForEnemy());	
	}
	
	public void EndTurn()
	{
		GD.Print("Cheguei aqui");
		currentEnergy = maxEnergy;
		_cardManager.DiscardHand();
		ExecuteEnemyTurns();
		Player.UpdateTemporaryValues();
		Player.UpdateLabelValues();
		StartTurn();
		
	}
	public void StartTurn()
	{
		_cardManager.DrawCard(_cardManager.cardsDrawedPerTurn);
		UpdateEnergy(currentEnergy);
		_cardManager.OrganizeHand();
	}
	public void StartGame()
	{
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
}
