namespace CalculatriceMaui;

public partial class MainPage : ContentPage
{
    private readonly CalculatorEngine _engine = new();

    private double _resultBaseSize = 56;
    private double _availableWidth = 300;

    public MainPage()
    {
        InitializeComponent();

        // Le contenu du ScrollView est au moins aussi large que la zone visible :
        // le texte reste ainsi aligné à droite tant qu'il ne déborde pas.
        ExpressionScroll.SizeChanged += (_, _) =>
        {
            if (ExpressionScroll.Width > 0) ExpressionHost.MinimumWidthRequest = ExpressionScroll.Width;
        };
        ResultScroll.SizeChanged += (_, _) =>
        {
            if (ResultScroll.Width > 0) ResultHost.MinimumWidthRequest = ResultScroll.Width;
        };

        Refresh();
    }

    // ------------------------------------------------- Adaptation écran / orientation

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;

        bool landscape = width > height;

        // En paysage, on libère de la hauteur en masquant la barre de titre.
        TopBar.IsVisible = !landscape;

        // Taille des touches dépendante de la largeur ET de la hauteur disponibles.
        Resources["KeyFontSize"] = Math.Clamp(Math.Min(width / 12, height / 22), 16d, 30d);

        _resultBaseSize = Math.Clamp(height * 0.075, 26d, 60d);
        _availableWidth = Math.Max(width - 48, 100);
        ExpressionLabel.FontSize = Math.Clamp(_resultBaseSize * 0.4, 14d, 22d);

        UpdateResultFontSize();
    }

    private void UpdateResultFontSize()
    {
        int length = Math.Max(ResultLabel.Text?.Length ?? 1, 1);
        double fitting = _availableWidth / (length * 0.58);
        ResultLabel.FontSize = Math.Clamp(Math.Min(_resultBaseSize, fitting), 16d, _resultBaseSize);
    }

    // ----------------------------------------------------------------- Affichage

    private void Refresh()
    {
        ExpressionLabel.Text = _engine.Expression;
        ResultLabel.Text = _engine.Display;

        if (_engine.HasError)
            ResultLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Red, Colors.IndianRed);
        else
            ResultLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

        UpdateResultFontSize();

        // Si le texte dépasse malgré tout, on montre sa fin (chiffres les plus récents).
        Dispatcher.Dispatch(async () =>
        {
            await ExpressionScroll.ScrollToAsync(ExpressionScroll.ContentSize.Width, 0, false);
            await ResultScroll.ScrollToAsync(ResultScroll.ContentSize.Width, 0, false);
        });
    }

    // ------------------------------------------------------ Gestionnaires d'événements

    private void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Text.Length > 0)
            _engine.InputDigit(button.Text[0]);
        Refresh();
    }

    private void OnDecimalClicked(object? sender, EventArgs e)
    {
        _engine.InputDecimal();
        Refresh();
    }

    private void OnOperatorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string op } && op.Length == 1)
            _engine.InputOperator(op[0]);
        Refresh();
    }

    private void OnEqualsClicked(object? sender, EventArgs e)
    {
        _engine.Evaluate();
        Refresh();
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _engine.Reset();
        Refresh();
    }

    private void OnBackspaceClicked(object? sender, EventArgs e)
    {
        _engine.Backspace();
        Refresh();
    }

    private void OnSignClicked(object? sender, EventArgs e)
    {
        _engine.ToggleSign();
        Refresh();
    }

    private void OnPercentClicked(object? sender, EventArgs e)
    {
        _engine.Percent();
        Refresh();
    }

    private void OnSqrtClicked(object? sender, EventArgs e)
    {
        _engine.SquareRoot();
        Refresh();
    }

    private void OnSquareClicked(object? sender, EventArgs e)
    {
        _engine.Square();
        Refresh();
    }

    private void OnInverseClicked(object? sender, EventArgs e)
    {
        _engine.Inverse();
        Refresh();
    }
}
