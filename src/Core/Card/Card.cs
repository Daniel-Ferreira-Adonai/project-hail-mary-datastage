using Godot;
using System;

public partial class Card : Node2D
{
	private MouseInputTracker _mouse;

	public CardData Data {get;  set;}
	
	[Signal] public delegate void CardHoveredEventHandler(Card card);
	[Signal] public delegate void CardUnhoveredEventHandler(Card card);
	
	public override void _Ready()
	{
   		//  GetParent().GetParent<CardManager>().ConnectCardSignals(this);
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
	}

	public override void _Process(double delta)
	{
		
	}
	public void _on_area_2d_mouse_entered()
	{
		EmitSignal(SignalName.CardHovered,this);
	}
	public void _on_area_2d_mouse_exited()
	{
		EmitSignal(SignalName.CardUnhovered, this);
	}
	public void Play(object enemie, object playerObj)
{
    if (playerObj is not Player player)
        return;

    if (Data.tipoCarta == CardData.CardType.Attack || Data.tipoCarta == CardData.CardType.SkillWithEnemyEffect)
    {
        if (enemie is Enemy enemy)
        {
            Data.Execute(enemy, player);
        }
    }
    else
    {
        Data.Execute(player);
    }
}
	public void UpdateDamagePreview(Player player, Enemy target = null)
{
    if (Data.tipoCarta != CardData.CardType.Attack) 
        return;

    int previewDamage = CombatManager.Calculate(Data.Damage, player, target);

    var descricaoLabel = GetNode<RichTextLabel>("Descricao");

    string texto = Data.Description
        .Replace("$d", previewDamage.ToString());

    descricaoLabel.Text = texto;

    if (previewDamage > Data.Damage)
        descricaoLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f)); 
    else if (previewDamage < Data.Damage)
        descricaoLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f)); 
    else
        descricaoLabel.RemoveThemeColorOverride("font_color"); 
}
    
    public void UpdateBlockPreview(Player player)
{
    if (Data.tipoCarta != CardData.CardType.Skill) 
        return;

    int previewBlock = CombatManager.CalculateBlock(Data.Block, player);

    var descricaoLabel = GetNode<RichTextLabel>("Descricao");

    string texto = Data.Description
        .Replace("$b", previewBlock.ToString());

    descricaoLabel.Text = texto;

    if (previewBlock > Data.Block)
        descricaoLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
    else if (previewBlock < Data.Block)
        descricaoLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
    else
        descricaoLabel.RemoveThemeColorOverride("font_color");
}

	public void Setup(CardData data)
	{
		Data = data;
        GetNode<TextureRect>("ArteCarta").Texture = data.Art;
		var nomeLabel = GetNode<Label>("Nome");
        nomeLabel.Text = data.CardName;

        if (this.Data.IsCardUpgraded)
                nomeLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
            else
                nomeLabel.RemoveThemeColorOverride("font_color");

		GetNode<Label>("Custo").Text = data.EnergyCost.ToString();
        string descricao = data.Description
        .Replace("$d", data.Damage.ToString())
        .Replace("$b", data.Block.ToString());
		GetNode<RichTextLabel>("Descricao").Text = descricao;
		}
        public void UnloadVisuals()
    {
        // GetNode<Sprite2D>("MolduraFotoCard").Texture = null;
    }
		public void LoadVisuals()
        {
            // // carrega textura só quando entra na mão
            // if (Data.Art != null)
            //     GetNode<Sprite2D>("MolduraFotoCard").Texture = Data.Art;
        }
}
