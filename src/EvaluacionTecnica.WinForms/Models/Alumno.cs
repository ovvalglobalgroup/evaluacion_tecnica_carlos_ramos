namespace EvaluacionTecnica.WinForms.Models;

/// <summary>
/// Representa la clase solicitada en el punto 2 de la evaluación.
/// </summary>
public sealed class Alumno
{
    public int Identificador { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public override string ToString() => Nombre;
}
