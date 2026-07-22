namespace EvaluacionTecnica.WinForms.Models;

public sealed class BandaVencimiento
{
    public int Orden { get; init; }

    public string Banda { get; init; } = string.Empty;

    public decimal CapitalTotal { get; init; }
}
