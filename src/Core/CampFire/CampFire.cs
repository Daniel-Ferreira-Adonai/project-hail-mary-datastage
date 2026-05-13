using Godot;

public partial class CampFire : Control
{
    [Signal] public delegate void ExitRequestedEventHandler();

    private Godot.Button _restButton;
    private Godot.Button _upgradeButton;
    private Godot.Button _proceedButton;
    private Label _messageLabel;
    private BarraDeVida _hpBar;

    public override void _Ready()
    {
        _restButton    = GetNode<Godot.Button>("ActionPanel/MarginContainer/VBoxContainer/ChoicesContainer/RestChoice/RestButton");
        _upgradeButton = GetNode<Godot.Button>("ActionPanel/MarginContainer/VBoxContainer/ChoicesContainer/UpgradeChoice/UpgradeButton");
        _messageLabel  = GetNode<Label>("ActionPanel/MarginContainer/VBoxContainer/MessageLabel");
        _proceedButton = GetNode<Godot.Button>("ProceedButton");
        _proceedButton.Visible = false;

        var vbox = GetNode<VBoxContainer>("ActionPanel/MarginContainer/VBoxContainer");
        var hpBarScene = GD.Load<PackedScene>("res://Test/TestScenes/BarraDeVida.tscn");
        _hpBar = hpBarScene.Instantiate<BarraDeVida>();
        vbox.AddChild(_hpBar);
        vbox.MoveChild(_hpBar, _messageLabel.GetIndex());

        RefreshHpBar();
        SetMessage("Escolha uma ação na fogueira.");
    }

    public void OnRestPressed()
    {
        var player = PlayerManager.Instance?.Player;
        if (!IsInstanceValid(player))
        {
            SetMessage("Player não encontrado.");
            return;
        }

        int hpBefore   = player.currentHp;
        int healAmount = Mathf.CeilToInt(player.MaxHp * 0.3f);
        player.TryToHeal(healAmount);

        _restButton.Disabled    = true;
        _upgradeButton.Disabled = true;
        _proceedButton.Visible  = true;

        RefreshHpBar();
        GameManager.Instance?.UpdateTopBar();

        int healed = player.currentHp - hpBefore;
        SetMessage(healed > 0
            ? $"Descansou e recuperou {healed} de vida!"
            : "Você já estava com a vida cheia.");
    }

    public void OnUpgradePressed()
    {
        var player = PlayerManager.Instance?.Player;
        if (!IsInstanceValid(player)) { SetMessage("Player não encontrado."); return; }

        var deck = player.GetDeck();
        bool hasUpgradable = deck.Exists(c => !c.IsCardUpgraded);
        if (!hasUpgradable) { SetMessage("Todas as cartas já foram melhoradas."); return; }

        GameManager.Instance?.ShowDeckViewer(DeckViewer.ViewerMode.Upgrade);

        _restButton.Disabled    = true;
        _upgradeButton.Disabled = true;
        _proceedButton.Visible  = true;
        SetMessage("Escolha uma carta para melhorar.");
    }

    public void OnExitPressed()
    {
        EmitSignal(SignalName.ExitRequested);
    }

    private void RefreshHpBar()
    {
        var player = PlayerManager.Instance?.Player;
        if (!IsInstanceValid(player)) return;

        _hpBar.Setup(player.MaxHp, player.currentHp);
        _hpBar.updateLabels(player.currentHp, player.MaxHp, 0);
    }

    private void SetMessage(string message)
    {
        _messageLabel.Text = message;
    }
}
