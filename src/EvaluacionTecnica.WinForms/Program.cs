using EvaluacionTecnica.WinForms.Forms;

namespace EvaluacionTecnica.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
