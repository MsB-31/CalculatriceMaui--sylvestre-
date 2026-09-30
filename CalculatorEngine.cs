using System.Globalization;

namespace CalculatriceMaui;

/// <summary>
/// Moteur de la calculatrice : aucune dépendance à l'interface.
/// Utilise decimal pour éviter les erreurs d'arrondi binaires (0,1 + 0,2 = 0,3).
/// </summary>
public sealed class CalculatorEngine
{
    private const int MaxDigits = 15;
    private const int MaxDecimals = 10;

    private decimal? _accumulator;      // opérande gauche
    private char? _pendingOp;           // + - * /
    private bool _startNew = true;      // le prochain chiffre remplace l'affichage
    private bool _awaitingOperand;      // un opérateur vient d'être saisi
    private bool _justEvaluated;        // on vient d'appuyer sur "=" (ou une fonction unaire)

    /// <summary>Nombre en cours de saisie / dernier résultat (format invariant, ex. "12.5").</summary>
    public string Current { get; private set; } = "0";

    /// <summary>Opération en cours, affichée au-dessus du résultat.</summary>
    public string Expression { get; private set; } = string.Empty;

    public string? Error { get; private set; }
    public bool HasError => Error is not null;

    /// <summary>Texte à afficher (virgule décimale française).</summary>
    public string Display => HasError ? Error! : Current.Replace('.', ',');

    // ------------------------------------------------------------------ Saisie

    public void InputDigit(char digit)
    {
        BeginEntry();

        if (_startNew)
        {
            Current = digit.ToString();
            _startNew = false;
            _awaitingOperand = false;
            return;
        }

        if (Current == "0") { Current = digit.ToString(); return; }
        if (Current == "-0") { Current = "-" + digit; return; }
        if (DigitCount(Current) >= MaxDigits) return;

        Current += digit;
    }

    public void InputDecimal()
    {
        BeginEntry();

        if (_startNew)
        {
            Current = "0.";
            _startNew = false;
            _awaitingOperand = false;
            return;
        }

        if (!Current.Contains('.') && DigitCount(Current) < MaxDigits)
            Current += ".";
    }

    public void Backspace()
    {
        if (HasError) { Reset(); return; }
        if (_startNew || _justEvaluated) return; // on n'efface pas un résultat

        if (Current.Length <= 1 || (Current.Length == 2 && Current[0] == '-'))
            Current = "0";
        else
            Current = Current[..^1];
    }

    public void Reset()
    {
        Current = "0";
        Expression = string.Empty;
        Error = null;
        _accumulator = null;
        _pendingOp = null;
        _startNew = true;
        _awaitingOperand = false;
        _justEvaluated = false;
    }

    public void ToggleSign()
    {
        if (HasError) return;

        if (_awaitingOperand)
        {
            // "5 +" puis ± : on commence un second opérande négatif
            Current = "-0";
            _startNew = false;
            _awaitingOperand = false;
            return;
        }

        if (ParseCurrent() == 0m && !Current.StartsWith('-')) return;

        Current = Current.StartsWith('-') ? Current[1..] : "-" + Current;
    }

    public void Percent()
    {
        if (HasError) return;

        decimal x = ParseCurrent();
        decimal result;

        // 200 + 10 % => 10 % de 200 = 20 ; sinon x / 100
        if (_pendingOp is '+' or '-' && _accumulator is not null && !_awaitingOperand)
            result = _accumulator.Value * x / 100m;
        else
            result = x / 100m;

        SetUnaryResult($"{ToDisplay(x)} %", result);
    }

    // -------------------------------------------------------------- Opérations

    public void InputOperator(char op)
    {
        if (HasError) return;

        if (_justEvaluated)
        {
            _justEvaluated = false;
            Expression = string.Empty;
        }

        if (_pendingOp is not null && !_awaitingOperand)
        {
            // enchaînement : 2 + 3 × ... => on calcule 2 + 3 d'abord
            decimal left = _accumulator!.Value;
            decimal right = ParseCurrent();

            if (!TryCompute(left, _pendingOp.Value, right, out decimal r))
            {
                Expression = $"{ToDisplay(left)} {Symbol(_pendingOp.Value)} {ToDisplay(right)}";
                return;
            }

            _accumulator = r;
            Current = Format(r);
        }
        else if (_pendingOp is null)
        {
            _accumulator = ParseCurrent();
        }
        // sinon : opérateur remplacé (ex. "5 +" puis "×")

        _pendingOp = op;
        _awaitingOperand = true;
        _startNew = true;
        Expression = $"{ToDisplay(_accumulator!.Value)} {Symbol(op)}";
    }

    public void Evaluate()
    {
        if (HasError || _pendingOp is null) return;

        decimal left = _accumulator!.Value;
        decimal right = _awaitingOperand ? left : ParseCurrent(); // "5 + =" => 10
        char op = _pendingOp.Value;

        Expression = $"{ToDisplay(left)} {Symbol(op)} {ToDisplay(right)} =";

        if (!TryCompute(left, op, right, out decimal result)) return;

        Current = Format(result);
        _accumulator = null;
        _pendingOp = null;
        _awaitingOperand = false;
        _startNew = true;
        _justEvaluated = true;
    }

    // --------------------------------------------------- Fonctions avancées

    public void SquareRoot()
    {
        if (HasError) return;

        decimal x = ParseCurrent();
        string label = $"√({ToDisplay(x)})";

        if (x < 0m) { Fail("Racine d'un nombre négatif impossible", label); return; }

        SetUnaryResult(label, (decimal)Math.Sqrt((double)x));
    }

    public void Square()
    {
        if (HasError) return;

        decimal x = ParseCurrent();
        string label = $"sqr({ToDisplay(x)})";

        try { SetUnaryResult(label, x * x); }
        catch (OverflowException) { Fail("Résultat trop grand", label); }
    }

    public void Inverse()
    {
        if (HasError) return;

        decimal x = ParseCurrent();
        string label = $"1/({ToDisplay(x)})";

        if (x == 0m) { Fail("Division par zéro impossible", label); return; }

        SetUnaryResult(label, 1m / x);
    }

    // ----------------------------------------------------------------- Interne

    private void BeginEntry()
    {
        if (HasError) Reset();

        if (_justEvaluated)
        {
            _accumulator = null;
            _pendingOp = null;
            _awaitingOperand = false;
            _justEvaluated = false;
            _startNew = true;
            Expression = string.Empty;
        }
    }

    private void SetUnaryResult(string label, decimal result)
    {
        Current = Format(result);
        _startNew = true;
        _awaitingOperand = false;

        if (_pendingOp is null)
        {
            Expression = label;
            _justEvaluated = true;
        }
    }

    private void Fail(string message, string expression)
    {
        Error = message;
        Expression = expression;
    }

    private bool TryCompute(decimal a, char op, decimal b, out decimal result)
    {
        result = 0m;

        try
        {
            switch (op)
            {
                case '+': result = a + b; break;
                case '-': result = a - b; break;
                case '*': result = a * b; break;
                case '/':
                    if (b == 0m)
                    {
                        Error = "Division par zéro impossible";
                        return false;
                    }
                    result = a / b;
                    break;
            }
            return true;
        }
        catch (OverflowException)
        {
            Error = "Résultat trop grand";
            return false;
        }
    }

    private decimal ParseCurrent() =>
        decimal.TryParse(Current, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal v) ? v : 0m;

    private static int DigitCount(string s) => s.Count(char.IsDigit);

    private static string Format(decimal value)
    {
        value = Math.Round(value, MaxDecimals, MidpointRounding.AwayFromZero);
        value /= 1.0000000000000000000000000000m; // supprime les zéros inutiles (2.50 -> 2.5)
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string ToDisplay(decimal value) => Format(value).Replace('.', ',');

    private static string Symbol(char op) => op switch
    {
        '+' => "+",
        '-' => "−",
        '*' => "×",
        '/' => "÷",
        _ => op.ToString()
    };
}
