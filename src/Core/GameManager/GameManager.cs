using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class GameManager : Node2D
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
		StartGame();
	}
	public void EndTurn()
	{
		GD.Print("Cheguei aqui");
		currentEnergy = maxEnergy;
		_cardManager.DiscardHand();
		ExecuteEnemyTurns();
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
	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
	public void UpdateEnergy(int energy)
	{
		energyLabel.Text = energy.ToString();
	}
}
