using Godot;
using System;
using System.Threading.Tasks;

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
    if (Data.tipoCarta != CardData.CardType.Attack && Data.tipoCarta != CardData.CardType.SkillWithEnemyEffect) 
        return;
    
    int previewDamage = CombatManager.Calculate(Data.Damage, player, target, this.Data);
    int previewBlock = CombatManager.CalculateBlock(Data.Block, player, this.Data);

    var descricaoLabel = GetNode<RichTextLabel>("Descricao");

    string damageColor = "white"; 

    string effectColor = "#c3e213"; 

    string loseHpColor = "#ff4d4d"; 
    if (previewDamage > Data.Damage)
        damageColor = "#4dff4d"; 
    else if (previewDamage < Data.Damage)
        damageColor = "#ff4d4d"; 
    
    // if (this.Data.IsCardUpgraded)
    //     effectColor = "#4dff4d"; 
    
    
    string texto = Data.Description
    .Replace("$d", $"[color={damageColor}]{previewDamage}[/color]")
    .Replace("$b", $"[color=white]{previewBlock}[/color]")
    .Replace("$ef", $"[color={effectColor}]{this.Data.EffectValue}[/color]")
    .Replace("$lshp", $"[color={loseHpColor}]{this.Data.EffectValue}[/color]")
    .Replace("$sef", $"[color={effectColor}]{this.Data.SecondaryEffectValue}[/color]");
    
    descricaoLabel.Text = texto;
    
    descricaoLabel.RemoveThemeColorOverride("font_color");
}
    
  public void UpdateBlockPreview(Player player)
{
    if (Data.tipoCarta != CardData.CardType.Skill) 
    return;

    int previewBlock = CombatManager.CalculateBlock(Data.Block, player, this.Data);

    var descricaoLabel = GetNode<RichTextLabel>("Descricao");

    string blockColor = "white";


    string effectColor = "#c3e213"; 
    
    string loseHpColor = "#ff4d4d"; 


    if (previewBlock > Data.Block)
        blockColor = "#4dff4d";
    else if (previewBlock < Data.Block)
        blockColor = "#ff4d4d";

    string texto = Data.Description
        .Replace("$b", $"[color={blockColor}]{previewBlock}[/color]")
        .Replace("$ef", $"[color={effectColor}]{this.Data.EffectValue}[/color]")
        .Replace("$lshp", $"[color={loseHpColor}]{this.Data.EffectValue}[/color]")
        .Replace("$sef", $"[color={effectColor}]{this.Data.SecondaryEffectValue}[/color]");

    descricaoLabel.Text = texto;
    descricaoLabel.RemoveThemeColorOverride("font_color");
}
    public async Task PlayUpgradeAnimation()
    {
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Play("UpgradeCard");
        
        await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
        
        GetNode<Label>("Nome").AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
    }
    public async Task PlayRemoveAnimation()
{
    var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    animPlayer.Play("Remove");
    
    await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
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
        .Replace("$b", data.Block.ToString())
        .Replace("$ef", data.EffectValue.ToString())
        .Replace("$lshp", data.EffectValue.ToString())
        .Replace("$sef", data.SecondaryEffectValue.ToString());
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
