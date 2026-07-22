using EvaluacionTecnica.WinForms.Data;
using EvaluacionTecnica.WinForms.Models;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;

namespace EvaluacionTecnica.WinForms.Forms;

public class MainForm : Form
{
    // Paketa de colores para la interfaz de usuario
    private static readonly Color AzulPrincipal = Color.FromArgb(10, 91, 166);
    private static readonly Color AzulOscuro = Color.FromArgb(5, 61, 105);
    private static readonly Color AzulDashboard = Color.FromArgb(4, 43, 77);
    private static readonly Color VerdePrincipal = Color.FromArgb(0, 148, 67);
    private static readonly Color VerdeClaro = Color.FromArgb(226, 246, 234);
    private static readonly Color AmarilloPrincipal = Color.FromArgb(255, 211, 0);
    private static readonly Color FondoAplicacion = Color.FromArgb(243, 247, 250);
    private static readonly Color FondoSuave = Color.FromArgb(248, 250, 252);
    private static readonly Color BordeSuave = Color.FromArgb(220, 228, 235);
    private static readonly Color TextoPrincipal = Color.FromArgb(27, 40, 52);
    private static readonly Color TextoSecundario = Color.FromArgb(100, 113, 126);

    private readonly List<Alumno> _alumnos = new();
    private readonly List<Alumno> _seleccionados = new();
    private readonly HashSet<int> _identificadoresSeleccionados = new();
    private readonly MariaDbService _mariaDbService = new();

    // Controles funcionales originales
    private readonly DataGridView _dgvAlumnos = new();
    private readonly ListBox _lstDisponibles = new();
    private readonly ComboBox _cboSeleccionados = new();
    private readonly TextBox _txtDescripcionSeleccionado = new();

    private readonly TextBox _txtNombre = new();
    private readonly CheckBox _chkActivo = new();
    private readonly TextBox _txtNuevaDescripcion = new();

    private readonly Button _btnOrdenar = new RoundedButton();
    private readonly Button _btnAgregar = new RoundedButton();
    private readonly RoundedButton _btnRestaurarSeleccion = new();
    private readonly Button _btnProbarConexion = new RoundedButton();
    private readonly Button _btnConsultarBandas = new RoundedButton();
    private readonly DataGridView _dgvResultadoBandas = new();
    private readonly Label _lblEstadoBase = new();
    private readonly Label _lblEstadoConsulta = new();

    // Controles visuales del dashboard
    private readonly Panel _contenedorPaginas = new();
    private readonly Panel _paginaAlumnos = new();
    private readonly Panel _paginaConsultarBandas = new();
    private readonly Panel _paginaProbarConexion = new();
    private readonly Button _btnNavAlumnos = new RoundedButton();
    private readonly Button _btnNavConsultarBandas = new RoundedButton();
    private readonly Button _btnNavProbarConexion = new RoundedButton();
    private readonly Label _lblTituloPagina = new();
    private readonly Label _lblSubtituloPagina = new();
    private readonly Label _lblTotalAlumnos = new();
    private readonly Label _lblTotalActivos = new();
    private readonly Label _lblTotalSeleccionados = new();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);


    /**********************************************************************************************************
        Ejecución del método principal para inicilizar la interfaz de usuario y sus componentes a renderizar
    **********************************************************************************************************/
    public MainForm()
    {
        Text = "Evaluación Técnica - Analista de Aplicaciones de Software";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1180, 720);
        Size = new Size(1440, 900);
        Font = new Font("Segoe UI", 10F);
        BackColor = FondoAplicacion;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;

        SuspendLayout();

        // Renderización de métodos principales
        CargarIconoFormulario();
        CrearInterfazUsuario();
        ConfigurarEventos();
        CargarAlumnosIniciales();
        MostrarPaginaAlumnos();

        ResumeLayout(true);
    }


    /**********************************************************************************************************
        Creación de la parte gráfica del dashboard incluyendo los elementos de paginación
    **********************************************************************************************************/
    private void CrearInterfazUsuario()
    {
        var estructuraPrincipal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = FondoAplicacion,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        estructuraPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 265F));
        estructuraPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var barraLateral = CrearAside();

        var zonaPrincipal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = FondoAplicacion,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        zonaPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
        zonaPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        zonaPrincipal.Controls.Add(CrearEncabezado(), 0, 0);

        _contenedorPaginas.Dock = DockStyle.Fill;
        _contenedorPaginas.BackColor = FondoAplicacion;
        _contenedorPaginas.Margin = Padding.Empty;

        CrearSeccionAlumnos();
        CrearPaginaConsultarBandas();
        CrearPaginaProbarConexion();

        _paginaAlumnos.Dock = DockStyle.Fill;
        _paginaConsultarBandas.Dock = DockStyle.Fill;
        _paginaProbarConexion.Dock = DockStyle.Fill;

        // Manejo de contenido dinamico al seleccionar la pagina correspondiente
        _contenedorPaginas.Controls.Add(_paginaProbarConexion);
        _contenedorPaginas.Controls.Add(_paginaConsultarBandas);
        _contenedorPaginas.Controls.Add(_paginaAlumnos);

        zonaPrincipal.Controls.Add(_contenedorPaginas, 0, 1);

        estructuraPrincipal.Controls.Add(barraLateral, 0, 0);
        estructuraPrincipal.Controls.Add(zonaPrincipal, 1, 0);

        Controls.Add(estructuraPrincipal);
    }


    /**********************************************************************************************************
        Crear la sección Aside del dashboard
    **********************************************************************************************************/
    private Control CrearAside()
    {
        var barra = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AzulDashboard,
            Padding = new Padding(18, 20, 18, 20),
            Margin = Padding.Empty
        };

        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

        // Logo del banco
        var marca = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(4, 0, 4, 12)
        };
        marca.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        marca.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var logoBanco = new PictureBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = Padding.Empty,
            TabStop = false
        };

        // Gestión de la imagen en el directorio local
        var rutaLogoBarra = Path.Combine(
            AppContext.BaseDirectory,
            "Images",
            "logo_bdd.png");

        if (File.Exists(rutaLogoBarra))
        {
            // Se replica la imagen dentro del formulario
            using var imagenTemporal = Image.FromFile(rutaLogoBarra);
            logoBanco.Image = new Bitmap(imagenTemporal);
        }
        else
        {
            //En caso de no encontrar la imagen
            logoBanco.BackColor = AzulDashboard;
        }

        // Libera la imagen al cerrar el formulario.
        logoBanco.Disposed += (_, _) => logoBanco.Image?.Dispose();

        marca.Controls.Add(logoBanco, 0, 0);

        // Carga de los elemento de menu de navegación
        ConfigurarMenuNavegacion(_btnNavAlumnos, "  Gestión de alumnos");
        ConfigurarMenuNavegacion(_btnNavConsultarBandas, "  Consultar bandas");
        ConfigurarMenuNavegacion(_btnNavProbarConexion, "  Probar conexión");

        var tarjetaInferior = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 76, 119),
            BorderColor = Color.FromArgb(44, 103, 145),
            CornerRadius = 18,
            Margin = new Padding(0, 12, 0, 0),
            Padding = new Padding(16)
        };

        var infoInferior = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        infoInferior.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        infoInferior.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        infoInferior.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

        infoInferior.Controls.Add(new Label
        {
            Text = "EVALUACIÓN TÉCNICA",
            Dock = DockStyle.Fill,
            ForeColor = AmarilloPrincipal,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        infoInferior.Controls.Add(new Label
        {
            Text = "Gestión de alumnos y consulta de cartera",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);


        tarjetaInferior.Controls.Add(infoInferior);

        contenido.Controls.Add(marca, 0, 0);
        contenido.Controls.Add(_btnNavAlumnos, 0, 1);
        contenido.Controls.Add(_btnNavConsultarBandas, 0, 2);
        contenido.Controls.Add(_btnNavProbarConexion, 0, 3);
        contenido.Controls.Add(tarjetaInferior, 0, 5);

        barra.Controls.Add(contenido);
        return barra;
    }


    /**********************************************************************************************************
        Crear el encabezado del dashboard
    **********************************************************************************************************/
    private Control CrearEncabezado()
    {
        var contenedor = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = FondoAplicacion,
            Padding = new Padding(24, 18, 24, 8)
        };

        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 22,
            Padding = new Padding(24, 14, 20, 14)
        };

        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 275F));

        var textos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        textos.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        textos.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        _lblTituloPagina.Text = "Gestión de alumnos";
        _lblTituloPagina.Dock = DockStyle.Fill;
        _lblTituloPagina.ForeColor = TextoPrincipal;
        _lblTituloPagina.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
        _lblTituloPagina.TextAlign = ContentAlignment.BottomLeft;

        _lblSubtituloPagina.Text = "Administración y selección de los alumnos registrados";
        _lblSubtituloPagina.Dock = DockStyle.Fill;
        _lblSubtituloPagina.ForeColor = TextoSecundario;
        _lblSubtituloPagina.Font = new Font("Segoe UI", 10F);
        _lblSubtituloPagina.TextAlign = ContentAlignment.TopLeft;

        textos.Controls.Add(_lblTituloPagina, 0, 0);
        textos.Controls.Add(_lblSubtituloPagina, 0, 1);

        var identificacion = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 14, 0, 0)
        };

        var distintivo = new Label
        {
            Text = "BD",
            Width = 46,
            Height = 46,
            BackColor = AmarilloPrincipal,
            ForeColor = AzulOscuro,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(10, 0, 0, 0)
        };
        distintivo.Resize += (_, _) => AplicarRegionCircular(distintivo);

        var nombre = new Label
        {
            Text = "Banco CoDesarrollo",
            AutoSize = false,
            Width = 180,
            Height = 48,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Margin = Padding.Empty
        };

        identificacion.Controls.Add(distintivo);
        identificacion.Controls.Add(nombre);

        contenido.Controls.Add(textos, 0, 0);
        contenido.Controls.Add(identificacion, 1, 0);
        tarjeta.Controls.Add(contenido);
        contenedor.Controls.Add(tarjeta);

        return contenedor;
    }


    /**********************************************************************************************************
        Crear la sección donde se almacena la infromación de los alumnos
    **********************************************************************************************************/
    private void CrearSeccionAlumnos()
    {
        _paginaAlumnos.BackColor = FondoAplicacion;
        _paginaAlumnos.Padding = new Padding(24, 10, 24, 22);

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var estadisticas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        estadisticas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        estadisticas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        estadisticas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

        estadisticas.Controls.Add(
            CrearTarjetaEstadistica(
                "ALUMNOS REGISTRADOS",
                "Total cargado en el Grid View",
                AzulPrincipal,
                _lblTotalAlumnos), 0, 0);

        estadisticas.Controls.Add(
            CrearTarjetaEstadistica(
                "ALUMNOS ACTIVOS",
                "Disponibles para seleccionar",
                VerdePrincipal,
                _lblTotalActivos), 1, 0);

        estadisticas.Controls.Add(
            CrearTarjetaEstadistica(
                "SELECCIONADOS",
                "Movidos desde la lista disponible",
                AmarilloPrincipal,
                _lblTotalSeleccionados), 2, 0);

        var cuerpo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63F));
        cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37F));

        var columnaIzquierda = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 8, 10, 0)
        };
        columnaIzquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        columnaIzquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        columnaIzquierda.Controls.Add(CrearTarjetaGrid(), 0, 0);
        columnaIzquierda.Controls.Add(CrearTarjetaNuevoAlumno(), 0, 1);

        var columnaDerecha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = new Padding(10, 8, 0, 0)
        };
        columnaDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 43F));
        columnaDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
        columnaDerecha.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));

        ConfigurarListaDisponibles();
        ConfigurarComboSeleccionados();
        ConfigurarDescripcionSeleccionado();

        var selectorSeleccionados = CrearSelectorSeleccionados();

        columnaDerecha.Controls.Add(
            CrearTarjetaContenido(
                "Disponibles",
                "Solo se muestran alumnos con estado activo. Realice doble clic para seleccionarlos.",
                _lstDisponibles,
                VerdePrincipal,
                new Padding(0, 0, 0, 8)), 0, 0);

        columnaDerecha.Controls.Add(
            CrearTarjetaContenido(
                "Seleccionados",
                "Elija un alumno para consultar su descripción o restáurelo a Disponibles.",
                selectorSeleccionados,
                AzulPrincipal,
                new Padding(0, 8, 0, 8)), 0, 1);

        columnaDerecha.Controls.Add(
            CrearTarjetaContenido(
                "Descripción del alumno",
                "Detalle correspondiente al elemento seleccionado.",
                _txtDescripcionSeleccionado,
                AmarilloPrincipal,
                new Padding(0, 8, 0, 0)), 0, 2);

        cuerpo.Controls.Add(columnaIzquierda, 0, 0);
        cuerpo.Controls.Add(columnaDerecha, 1, 0);

        estructura.Controls.Add(estadisticas, 0, 0);
        estructura.Controls.Add(cuerpo, 0, 1);

        _paginaAlumnos.Controls.Add(estructura);
    }


    /**********************************************************************************************************
        Crear la tarjeta donde se almacena la tabla para mostrar la información de los alumnos
    **********************************************************************************************************/
    private Control CrearTarjetaGrid()
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(18, 12, 18, 16)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));

        var titulo = new Label
        {
            Text = "Alumnos cargados al iniciar el programa",
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _btnOrdenar.Text = "Ordenar alfabéticamente";
        _btnOrdenar.Dock = DockStyle.Fill;
        _btnOrdenar.Margin = new Padding(8, 5, 0, 5);
        EstilizarBotonSecundario(_btnOrdenar);

        encabezado.Controls.Add(titulo, 0, 0);
        encabezado.Controls.Add(_btnOrdenar, 1, 0);

        ConfigurarDataGridView();

        estructura.Controls.Add(encabezado, 0, 0);
        estructura.Controls.Add(_dgvAlumnos, 0, 1);
        tarjeta.Controls.Add(estructura);

        return tarjeta;
    }


    /**********************************************************************************************************
        Crear la tarjeta con el formulario para crear un nuevo alumno con su respectiva información
    **********************************************************************************************************/
    private Control CrearTarjetaNuevoAlumno()
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(18, 12, 18, 14)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        estructura.Controls.Add(new Label
        {
            Text = "Registrar un nuevo alumno",
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false,
            Margin = Padding.Empty
        }, 0, 0);

        // Se crea el identificador autoincremental asigando a cada alumno automaticamente
        var formulario = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 0)
        };

        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        formulario.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        formulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

        _txtNombre.Dock = DockStyle.Fill;

        _chkActivo.Text = "Alumno activo";
        _chkActivo.Checked = true;
        _chkActivo.AutoSize = true;
        _chkActivo.Anchor = AnchorStyles.Left;
        _chkActivo.ForeColor = TextoPrincipal;
        _chkActivo.Margin = new Padding(0, 8, 0, 0);

        _txtNuevaDescripcion.Dock = DockStyle.Fill;
        _txtNuevaDescripcion.Multiline = true;
        _txtNuevaDescripcion.ScrollBars = ScrollBars.Vertical;
        _txtNuevaDescripcion.Margin = new Padding(0, 4, 0, 4);

        EstilizarCampo(_txtNombre);
        EstilizarCampo(_txtNuevaDescripcion);

        _btnAgregar.Text = "Agregar alumno";
        _btnAgregar.Width = 150;
        _btnAgregar.Height = 36;
        _btnAgregar.Anchor = AnchorStyles.Right;
        _btnAgregar.Margin = new Padding(0, 7, 0, 4);
        EstilizarBotonPrimario(_btnAgregar, VerdePrincipal);

        // Nombre del alumno
        var etiquetaNombre = CrearEtiquetaCampo("Nombre");
        formulario.Controls.Add(etiquetaNombre, 0, 0);
        formulario.SetColumnSpan(etiquetaNombre, 2);

        formulario.Controls.Add(_txtNombre, 0, 1);
        formulario.SetColumnSpan(_txtNombre, 2);

        // Descripción del alumno
        var etiquetaDescripcion = CrearEtiquetaCampo("Descripción");
        formulario.Controls.Add(etiquetaDescripcion, 0, 2);
        formulario.SetColumnSpan(etiquetaDescripcion, 2);

        formulario.Controls.Add(_txtNuevaDescripcion, 0, 3);
        formulario.SetColumnSpan(_txtNuevaDescripcion, 2);

        // Estado del alumno
        formulario.Controls.Add(_chkActivo, 0, 4);
        formulario.Controls.Add(_btnAgregar, 1, 4);

        estructura.Controls.Add(formulario, 0, 1);
        tarjeta.Controls.Add(estructura);

        return tarjeta;
    }


    /**********************************************************************************************************
       Crear la tarjeta para gestionar el contenido de los estudiantes
   **********************************************************************************************************/
    private Control CrearTarjetaContenido(
        string titulo,
        string subtitulo,
        Control contenido,
        Color colorAcento,
        Padding margen)
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = margen,
            Padding = new Padding(16, 12, 16, 14)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 7F));
        encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        encabezado.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        encabezado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        encabezado.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = colorAcento,
            Margin = new Padding(0, 3, 0, 5)
        }, 0, 0);
        encabezado.SetRowSpan(encabezado.GetControlFromPosition(0, 0)!, 2);

        encabezado.Controls.Add(new Label
        {
            Text = titulo,
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(10, 0, 0, 0)
        }, 1, 0);

        encabezado.Controls.Add(new Label
        {
            Text = subtitulo,
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 8.7F),
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true,
            Margin = new Padding(10, 0, 0, 0)
        }, 1, 1);

        contenido.Dock = DockStyle.Fill;
        contenido.Margin = new Padding(0, 4, 0, 0);

        estructura.Controls.Add(encabezado, 0, 0);
        estructura.Controls.Add(contenido, 0, 1);
        tarjeta.Controls.Add(estructura);

        return tarjeta;
    }


    /**********************************************************************************************************
        Crear tarjetas ubicadas en la parte superior de la sección de alumnos
    **********************************************************************************************************/

    private Control CrearTarjetaEstadistica(
        string titulo,
        string detalle,
        Color colorAcento,
        Label etiquetaValor)
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 0, 16, 12),
            Padding = new Padding(20, 14, 18, 14)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9F));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        var acento = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = colorAcento,
            Margin = new Padding(0, 0, 0, 0)
        };

        estructura.Controls.Add(acento, 0, 0);
        estructura.SetRowSpan(acento, 3);

        estructura.Controls.Add(new Label
        {
            Text = titulo,
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 0);

        etiquetaValor.Text = "0";
        etiquetaValor.Dock = DockStyle.Fill;
        etiquetaValor.ForeColor = TextoPrincipal;
        etiquetaValor.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        etiquetaValor.TextAlign = ContentAlignment.MiddleLeft;
        etiquetaValor.Margin = new Padding(14, 0, 0, 0);

        estructura.Controls.Add(etiquetaValor, 1, 1);

        estructura.Controls.Add(new Label
        {
            Text = detalle,
            Dock = DockStyle.Fill,
            ForeColor = colorAcento == AmarilloPrincipal ? VerdePrincipal : colorAcento,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 2);

        tarjeta.Controls.Add(estructura);
        return tarjeta;
    }


    /**********************************************************************************************************
        Crear la sección para consultar la sección Banda de Datos / Capital vencido
    **********************************************************************************************************/
    private void CrearPaginaConsultarBandas()
    {
        _paginaConsultarBandas.BackColor = FondoAplicacion;
        _paginaConsultarBandas.Padding =
            new Padding(24, 10, 24, 22);

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        estructura.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        estructura.Controls.Add(
            CrearTarjetaConsultaSql(),
            0,
            0);

        _paginaConsultarBandas.Controls.Add(estructura);
    }


    /**********************************************************************************************************
        Crear la sección para comprobar la conexión del aplicativo con MariaDB
    **********************************************************************************************************/
    private void CrearPaginaProbarConexion()
    {
        _paginaProbarConexion.BackColor = FondoAplicacion;
        _paginaProbarConexion.Padding = new Padding(24, 10, 24, 22);

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var resumen = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        resumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

        resumen.Controls.Add(
            CrearTarjetaDatoBase(
                "MOTOR DE DATOS",
                "MariaDB",
                "Conexión mediante MySqlConnector",
                AzulPrincipal), 0, 0);

        resumen.Controls.Add(
            CrearTarjetaDatoBase(
                "SERVIDOR LOCAL",
                $"{DatabaseSettings.Server}:{DatabaseSettings.Port}",
                $"Base de datos: {DatabaseSettings.Database}",
                VerdePrincipal), 1, 0);

        resumen.Controls.Add(
            CrearTarjetaEstadoBase(), 2, 0);

        estructura.Controls.Add(resumen, 0, 0);
        estructura.Controls.Add(CrearTarjetaProbarConexion(), 0, 1);

        _paginaProbarConexion.Controls.Add(estructura);
    }


    /**********************************************************************************************************
        Crear la tarjeta del motor de base de datos utilizado
    **********************************************************************************************************/
    private Control CrearTarjetaDatoBase(
        string titulo,
        string valor,
        string detalle,
        Color colorAcento)
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 0, 16, 12),
            Padding = new Padding(20, 14, 18, 14)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9F));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        var acento = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = colorAcento
        };
        estructura.Controls.Add(acento, 0, 0);
        estructura.SetRowSpan(acento, 3);

        estructura.Controls.Add(new Label
        {
            Text = titulo,
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 0);

        estructura.Controls.Add(new Label
        {
            Text = valor,
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 1);

        estructura.Controls.Add(new Label
        {
            Text = detalle,
            Dock = DockStyle.Fill,
            ForeColor = colorAcento,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 2);

        tarjeta.Controls.Add(estructura);
        return tarjeta;
    }


    /**********************************************************************************************************
        Crear la tarjeta del estado de conexión con la base de datos
    **********************************************************************************************************/
    private Control CrearTarjetaEstadoBase()
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(20, 14, 18, 14)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 9F));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        var acento = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AmarilloPrincipal
        };
        estructura.Controls.Add(acento, 0, 0);
        estructura.SetRowSpan(acento, 3);

        estructura.Controls.Add(new Label
        {
            Text = "ESTADO DE CONEXIÓN",
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 0);

        _lblEstadoBase.Text = "Sin comprobar";
        _lblEstadoBase.Dock = DockStyle.Fill;
        _lblEstadoBase.ForeColor = TextoPrincipal;
        _lblEstadoBase.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _lblEstadoBase.TextAlign = ContentAlignment.MiddleLeft;
        _lblEstadoBase.AutoEllipsis = true;
        _lblEstadoBase.Margin = new Padding(14, 0, 0, 0);

        estructura.Controls.Add(_lblEstadoBase, 1, 1);

        estructura.Controls.Add(new Label
        {
            Text = "Utilice el botón Probar conexión",
            Dock = DockStyle.Fill,
            ForeColor = VerdePrincipal,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(14, 0, 0, 0)
        }, 1, 2);

        tarjeta.Controls.Add(estructura);
        return tarjeta;
    }


    /**********************************************************************************************************
       Crear la tarjeta con ek boton para realizar al validación del estado de conexión
   **********************************************************************************************************/
    private Control CrearTarjetaProbarConexion()
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(24)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 88F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));

        estructura.Controls.Add(new Label
        {
            Text = "Verificación de la conexión con MariaDB",
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        estructura.Controls.Add(new Label
        {
            Text = "Compruebe que el servicio MySQL de XAMPP esté iniciado y que los datos de " +
                   "DatabaseSettings.cs coincidan con su servidor local.",
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 1);

        var datosConexion = new DashboardCard
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = FondoSuave,
            BorderColor = BordeSuave,
            CornerRadius = 14,
            Padding = new Padding(20),
            Margin = new Padding(0, 12, 0, 12)
        };

        datosConexion.Controls.Add(new Label
        {
            Text =
                $"Servidor: {DatabaseSettings.Server}:{DatabaseSettings.Port}\r\n" +
                $"Base de datos: {DatabaseSettings.Database}\r\n" +
                $"Usuario: {DatabaseSettings.UserId}",
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 11F),
            TextAlign = ContentAlignment.MiddleLeft
        });

        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 10, 0, 0)
        };

        _btnProbarConexion.Text = "Probar conexión";
        _btnProbarConexion.Width = 175;
        _btnProbarConexion.Height = 40;
        EstilizarBotonPrimario(_btnProbarConexion, VerdePrincipal);

        acciones.Controls.Add(_btnProbarConexion);

        estructura.Controls.Add(datosConexion, 0, 2);
        estructura.Controls.Add(acciones, 0, 3);
        tarjeta.Controls.Add(estructura);

        return tarjeta;
    }


    /**********************************************************************************************************
       Crear la tarjeta donde se ejecuta la consulta SQL para la consulta por bandas
    **********************************************************************************************************/
    private Control CrearTarjetaConsultaSql()
    {
        var tarjeta = new DashboardCard
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderColor = BordeSuave,
            CornerRadius = 18,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(20, 14, 20, 16)
        };

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };

        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        var encabezado = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };

        encabezado.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        encabezado.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 210F));

        var textos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };

        textos.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        textos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        textos.Controls.Add(new Label
        {
            Text = "Capital vencido por bandas",
            Dock = DockStyle.Fill,
            ForeColor = TextoPrincipal,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        textos.Controls.Add(new Label
        {
            Text = "Consulta las cuotas no pagadas de créditos vigentes con garantía prendaria.",
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.TopLeft
        }, 0, 1);

        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 15, 0, 0)
        };

        _btnConsultarBandas.Text = "Consultar bandas";
        _btnConsultarBandas.Width = 170;
        _btnConsultarBandas.Height = 38;

        EstilizarBotonPrimario(
            _btnConsultarBandas,
            VerdePrincipal);

        acciones.Controls.Add(_btnConsultarBandas);

        encabezado.Controls.Add(textos, 0, 0);
        encabezado.Controls.Add(acciones, 1, 0);

        ConfigurarDataGridViewBandas();

        var pie = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };

        pie.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 35F));

        pie.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 65F));

        _lblEstadoConsulta.Text = "Estado: sin consultar";
        _lblEstadoConsulta.Dock = DockStyle.Fill;
        _lblEstadoConsulta.ForeColor = TextoSecundario;
        _lblEstadoConsulta.Font =
            new Font("Segoe UI", 8.8F, FontStyle.Bold);
        _lblEstadoConsulta.TextAlign =
            ContentAlignment.MiddleLeft;
        _lblEstadoConsulta.AutoEllipsis = true;

        var configuracion = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = TextoSecundario,
            Text =
                $"Servidor: {DatabaseSettings.Server}:{DatabaseSettings.Port}   |   " +
                $"Base: {DatabaseSettings.Database}   |   " +
                $"Usuario: {DatabaseSettings.UserId}",
            Font = new Font("Segoe UI", 8.8F),
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true
        };

        pie.Controls.Add(_lblEstadoConsulta, 0, 0);
        pie.Controls.Add(configuracion, 1, 0);

        estructura.Controls.Add(encabezado, 0, 0);
        estructura.Controls.Add(_dgvResultadoBandas, 0, 1);
        estructura.Controls.Add(pie, 0, 2);

        tarjeta.Controls.Add(estructura);

        return tarjeta;
    }



    /**********************************************************************************************************
        Crear la tarjeta donde se ve el resultado en formato tabla para el capital vencido por bandas
    **********************************************************************************************************/
    private void ConfigurarDataGridViewBandas()
    {
        _dgvResultadoBandas.Dock = DockStyle.Fill;
        _dgvResultadoBandas.AutoGenerateColumns = false;
        _dgvResultadoBandas.AllowUserToAddRows = false;
        _dgvResultadoBandas.AllowUserToDeleteRows = false;
        _dgvResultadoBandas.AllowUserToResizeRows = false;
        _dgvResultadoBandas.AllowUserToResizeColumns = false;
        _dgvResultadoBandas.ReadOnly = true;
        _dgvResultadoBandas.MultiSelect = false;
        _dgvResultadoBandas.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;
        _dgvResultadoBandas.RowHeadersVisible = false;
        _dgvResultadoBandas.BorderStyle = BorderStyle.None;
        _dgvResultadoBandas.BackgroundColor = Color.White;
        _dgvResultadoBandas.GridColor = BordeSuave;
        _dgvResultadoBandas.EnableHeadersVisualStyles = false;
        _dgvResultadoBandas.ColumnHeadersBorderStyle =
            DataGridViewHeaderBorderStyle.None;
        _dgvResultadoBandas.ColumnHeadersHeight = 42;
        _dgvResultadoBandas.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _dgvResultadoBandas.RowTemplate.Height = 38;
        _dgvResultadoBandas.AutoSizeRowsMode =
            DataGridViewAutoSizeRowsMode.None;
        _dgvResultadoBandas.Margin =
            new Padding(0, 6, 0, 6);

        _dgvResultadoBandas.ColumnHeadersDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = AzulPrincipal,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Bold),
                Alignment =
                    DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = AzulPrincipal,
                SelectionForeColor = Color.White,
                Padding = new Padding(8, 0, 8, 0)
            };

        _dgvResultadoBandas.DefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = TextoPrincipal,
                SelectionBackColor =
                    Color.FromArgb(221, 237, 250),
                SelectionForeColor = AzulOscuro,
                Font = new Font("Segoe UI", 9.5F),
                Alignment =
                    DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(8, 4, 8, 4)
            };

        _dgvResultadoBandas.AlternatingRowsDefaultCellStyle =
            new DataGridViewCellStyle
            {
                BackColor = FondoSuave,
                Alignment =
                    DataGridViewContentAlignment.MiddleCenter
            };

        _dgvResultadoBandas.Columns.Clear();

        _dgvResultadoBandas.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Numero",
                HeaderText = "#",
                Width = 75,
                SortMode =
                    DataGridViewColumnSortMode.NotSortable,
                HeaderCell =
                {
                    Style =
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter
                    }
                },
                DefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter,
                        Font = new Font(
                            "Segoe UI",
                            9.5F,
                            FontStyle.Bold)
                    }
            });

        _dgvResultadoBandas.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Banda",
                HeaderText = "BANDA DE VENCIMIENTO",
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 50F,
                MinimumWidth = 260,
                SortMode =
                    DataGridViewColumnSortMode.NotSortable,
                HeaderCell =
                {
                    Style =
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter
                    }
                },
                DefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter
                    }
            });

        _dgvResultadoBandas.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CapitalTotal",
                HeaderText = "CAPITAL TOTAL",
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 50F,
                MinimumWidth = 260,
                SortMode =
                    DataGridViewColumnSortMode.NotSortable,
                HeaderCell =
                {
                    Style =
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter
                    }
                },
                DefaultCellStyle =
                    new DataGridViewCellStyle
                    {
                        Alignment =
                            DataGridViewContentAlignment.MiddleCenter,
                        Format = "C2",
                        FormatProvider =
                            CultureInfo.GetCultureInfo("es-EC"),
                        Font = new Font(
                            "Segoe UI",
                            9.5F,
                            FontStyle.Bold),
                        ForeColor = AzulOscuro
                    }
            });
    }


    /**********************************************************************************************************
       Configuración de estilos generales para botones, tablas y elementos utilizados en el dashboard
    **********************************************************************************************************/
    private void ConfigurarDataGridView()
    {
        _dgvAlumnos.Dock = DockStyle.Fill;
        _dgvAlumnos.AutoGenerateColumns = false;
        _dgvAlumnos.AllowUserToAddRows = false;
        _dgvAlumnos.AllowUserToDeleteRows = false;
        _dgvAlumnos.AllowUserToResizeRows = false;
        _dgvAlumnos.ReadOnly = true;
        _dgvAlumnos.MultiSelect = false;
        _dgvAlumnos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvAlumnos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _dgvAlumnos.RowHeadersVisible = false;
        _dgvAlumnos.BorderStyle = BorderStyle.None;
        _dgvAlumnos.BackgroundColor = Color.White;
        _dgvAlumnos.GridColor = BordeSuave;
        _dgvAlumnos.EnableHeadersVisualStyles = false;
        _dgvAlumnos.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _dgvAlumnos.ColumnHeadersHeight = 40;
        _dgvAlumnos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        _dgvAlumnos.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AzulPrincipal,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            SelectionBackColor = AzulPrincipal,
            Padding = new Padding(6, 0, 6, 0)
        };

        _dgvAlumnos.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = TextoPrincipal,
            SelectionBackColor = Color.FromArgb(221, 237, 250),
            SelectionForeColor = AzulOscuro,
            Font = new Font("Segoe UI", 9.2F),
            Padding = new Padding(6, 3, 6, 3),
            WrapMode = DataGridViewTriState.True
        };

        _dgvAlumnos.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = FondoSuave
        };

        _dgvAlumnos.Columns.Clear();

        _dgvAlumnos.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Alumno.Identificador),
            HeaderText = "IDENTIFICADOR",
            Width = 125
        });

        _dgvAlumnos.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Alumno.Nombre),
            HeaderText = "NOMBRE",
            Width = 190
        });

        _dgvAlumnos.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(Alumno.Activo),
            HeaderText = "ACTIVO",
            Width = 80,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                NullValue = false
            }
        });

        _dgvAlumnos.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Alumno.Descripcion),
            HeaderText = "DESCRIPCIÓN",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 260,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                WrapMode = DataGridViewTriState.True,
                Padding = new Padding(6, 3, 6, 3)
            }
        });
    }

    private void ConfigurarListaDisponibles()
    {
        _lstDisponibles.Dock = DockStyle.Fill;
        _lstDisponibles.DisplayMember = nameof(Alumno.Nombre);
        _lstDisponibles.BorderStyle = BorderStyle.None;
        _lstDisponibles.BackColor = FondoSuave;
        _lstDisponibles.ForeColor = TextoPrincipal;
        _lstDisponibles.Font = new Font("Segoe UI", 9.5F);
        _lstDisponibles.DrawMode = DrawMode.OwnerDrawFixed;
        _lstDisponibles.ItemHeight = 34;
        _lstDisponibles.IntegralHeight = false;
    }

    private Control CrearSelectorSeleccionados()
    {
   
        var contenedor = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        contenedor.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        contenedor.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 132F));

        contenedor.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        _cboSeleccionados.Dock = DockStyle.Top;
        _cboSeleccionados.Height = 34;
        _cboSeleccionados.Margin = new Padding(0, 0, 10, 0);

        _btnRestaurarSeleccion.Text = "Restaurar";
        _btnRestaurarSeleccion.Width = 120;
        _btnRestaurarSeleccion.Height = 36;
        _btnRestaurarSeleccion.Anchor =
            AnchorStyles.Top | AnchorStyles.Right;
        _btnRestaurarSeleccion.Margin = Padding.Empty;
        _btnRestaurarSeleccion.Padding = Padding.Empty;
        _btnRestaurarSeleccion.TextAlign =
            ContentAlignment.MiddleCenter;
        _btnRestaurarSeleccion.TabStop = false;
        _btnRestaurarSeleccion.CornerRadius = 11;

        EstilizarBotonPrimario(
            _btnRestaurarSeleccion,
            VerdePrincipal);


        _btnRestaurarSeleccion.ForeColor = Color.White;
        _btnRestaurarSeleccion.FlatAppearance.BorderSize = 0;
        _btnRestaurarSeleccion.UseVisualStyleBackColor = false;

        contenedor.Controls.Add(_cboSeleccionados, 0, 0);
        contenedor.Controls.Add(_btnRestaurarSeleccion, 1, 0);

        return contenedor;
    }

    private void ConfigurarComboSeleccionados()
    {
        _cboSeleccionados.Dock = DockStyle.Top;
        _cboSeleccionados.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboSeleccionados.DisplayMember = nameof(Alumno.Nombre);
        _cboSeleccionados.FlatStyle = FlatStyle.Flat;
        _cboSeleccionados.BackColor = FondoSuave;
        _cboSeleccionados.ForeColor = TextoPrincipal;
        _cboSeleccionados.Font = new Font("Segoe UI", 10F);
        _cboSeleccionados.Height = 34;
    }

    private void ConfigurarDescripcionSeleccionado()
    {
        _txtDescripcionSeleccionado.Dock = DockStyle.Fill;
        _txtDescripcionSeleccionado.Multiline = true;
        _txtDescripcionSeleccionado.ReadOnly = true;
        _txtDescripcionSeleccionado.ScrollBars = ScrollBars.Vertical;
        _txtDescripcionSeleccionado.BorderStyle = BorderStyle.None;
        _txtDescripcionSeleccionado.BackColor = FondoSuave;
        _txtDescripcionSeleccionado.ForeColor = TextoPrincipal;
        _txtDescripcionSeleccionado.Font = new Font("Segoe UI", 10F);
    }

    private static Label CrearEtiquetaCampo(string texto) => new()
    {
        Text = texto.ToUpperInvariant(),
        Dock = DockStyle.Fill,
        AutoSize = false,
        AutoEllipsis = false,
        UseMnemonic = false,
        ForeColor = TextoSecundario,
        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
        TextAlign = ContentAlignment.BottomLeft,
        Margin = new Padding(0, 0, 10, 2),
        Padding = Padding.Empty
    };

    private static void EstilizarCampo(Control control)
    {
        control.BackColor = Color.White;
        control.ForeColor = TextoPrincipal;
        control.Font = new Font("Segoe UI", 10F);
        control.Margin = new Padding(0, 4, 8, 4);

        switch (control)
        {
            case TextBox textBox:
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;

            case NumericUpDown numeric:
                numeric.BorderStyle = BorderStyle.FixedSingle;
                break;
        }
    }

    private static void EstilizarBotonPrimario(Button boton, Color colorFondo)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.BackColor = colorFondo;
        boton.ForeColor = Color.White;
        boton.Cursor = Cursors.Hand;
        boton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        boton.UseVisualStyleBackColor = false;
    }

    private static void EstilizarBotonSecundario(Button boton)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.BorderColor = BordeSuave;
        boton.BackColor = Color.White;
        boton.ForeColor = AzulPrincipal;
        boton.Cursor = Cursors.Hand;
        boton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        boton.UseVisualStyleBackColor = false;
    }

    private static void ConfigurarMenuNavegacion(Button boton, string texto)
    {
        boton.Text = texto;
        boton.Dock = DockStyle.Fill;
        boton.Margin = new Padding(0, 4, 0, 4);
        boton.Padding = new Padding(14, 0, 0, 0);
        boton.TextAlign = ContentAlignment.MiddleLeft;
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.BackColor = AzulDashboard;
        boton.ForeColor = Color.FromArgb(218, 230, 239);
        boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        boton.Cursor = Cursors.Hand;
        boton.UseVisualStyleBackColor = false;
    }

    private void ConfigurarEventos()
    {
        // Navegación del dashboard
        _btnNavAlumnos.Click += (_, _) => MostrarPaginaAlumnos();
        _btnNavConsultarBandas.Click += (_, _) => MostrarPaginaConsultarBandas();
        _btnNavProbarConexion.Click += (_, _) => MostrarPaginaProbarConexion();

        _lstDisponibles.DoubleClick += (_, _) => MoverDisponibleASeleccionados();
        _lstDisponibles.DrawItem += DibujarElementoDisponible;
        _cboSeleccionados.SelectedIndexChanged += (_, _) => MostrarDescripcionSeleccionado();
        _btnRestaurarSeleccion.Click += (_, _) => RestaurarSeleccionadoADisponibles();
        _btnOrdenar.Click += (_, _) => OrdenarAlfabeticamente();
        _btnAgregar.Click += (_, _) => AgregarAlumno();
        _btnProbarConexion.Click += async (_, _) => await ProbarConexionAsync();
        _btnConsultarBandas.Click += async (_, _) => await ConsultarBandasAsync();

        _btnOrdenar.MouseEnter += (_, _) => _btnOrdenar.BackColor = FondoSuave;
        _btnOrdenar.MouseLeave += (_, _) => _btnOrdenar.BackColor = Color.White;
    }

    private void MostrarPaginaAlumnos()
    {
        _paginaAlumnos.Visible = true;
        _paginaConsultarBandas.Visible = false;
        _paginaProbarConexion.Visible = false;
        _paginaAlumnos.BringToFront();

        _lblTituloPagina.Text = "Gestión de alumnos";
        _lblSubtituloPagina.Text = "Administre, ordene y seleccione los alumnos registrados";

        MarcarBotonNavegacion(_btnNavAlumnos);
    }

    private void MostrarPaginaConsultarBandas()
    {
        _paginaAlumnos.Visible = false;
        _paginaConsultarBandas.Visible = true;
        _paginaProbarConexion.Visible = false;
        _paginaConsultarBandas.BringToFront();

        _lblTituloPagina.Text = "Consultar bandas";
        _lblSubtituloPagina.Text = "Consulte el capital vencido de créditos vigentes con garantía prendaria";

        MarcarBotonNavegacion(_btnNavConsultarBandas);
    }

    private void MostrarPaginaProbarConexion()
    {
        _paginaAlumnos.Visible = false;
        _paginaConsultarBandas.Visible = false;
        _paginaProbarConexion.Visible = true;
        _paginaProbarConexion.BringToFront();

        _lblTituloPagina.Text = "Probar conexión";
        _lblSubtituloPagina.Text = "Verifique la comunicación de la aplicación con MariaDB";

        MarcarBotonNavegacion(_btnNavProbarConexion);
    }

    private void MarcarBotonNavegacion(Button activo)
    {
        foreach (var boton in new[]
        {
            _btnNavAlumnos,
            _btnNavConsultarBandas,
            _btnNavProbarConexion
        })
        {
            var seleccionado = ReferenceEquals(boton, activo);

            boton.BackColor = seleccionado
                ? VerdePrincipal
                : AzulDashboard;

            boton.ForeColor = Color.White;
        }
    }

    private void DibujarElementoDisponible(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _lstDisponibles.Items.Count)
        {
            return;
        }

        var alumno = _lstDisponibles.Items[e.Index] as Alumno;
        var seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

        var fondo = seleccionado ? AzulPrincipal : FondoSuave;
        var texto = seleccionado ? Color.White : TextoPrincipal;
        var colorIndicador = seleccionado ? AmarilloPrincipal : VerdePrincipal;

        using var pincelFondo = new SolidBrush(fondo);
        e.Graphics.FillRectangle(pincelFondo, e.Bounds);

        using var pincelIndicador = new SolidBrush(colorIndicador);
        e.Graphics.FillEllipse(
            pincelIndicador,
            e.Bounds.Left + 10,
            e.Bounds.Top + (e.Bounds.Height - 9) / 2,
            9,
            9);

        using var pincelTexto = new SolidBrush(texto);
        e.Graphics.DrawString(
            alumno?.Nombre ?? string.Empty,
            e.Font ?? _lstDisponibles.Font,
            pincelTexto,
            e.Bounds.Left + 28,
            e.Bounds.Top + 7);

        e.DrawFocusRectangle();
    }

    /**********************************************************************************************************
       Carga de alumnos iniciales para llenar la tabla
    **********************************************************************************************************/
    private void CargarAlumnosIniciales()
    {
        _alumnos.AddRange(new[]
        {
            new Alumno
            {
                Identificador = 1,
                Nombre = "Marco Pérez",
                Activo = true,
                Descripcion = "Alumno con excelentes calificaciones"
            },
            new Alumno
            {
                Identificador = 2,
                Nombre = "Pilar Toapanta",
                Activo = false,
                Descripcion = "Alumno ha desertado en múltiples ocasiones"
            },
            new Alumno
            {
                Identificador = 3,
                Nombre = "Adrián Almeida",
                Activo = true,
                Descripcion = "Alumno promedio, proceso aprendizaje."
            },
            new Alumno
            {
                Identificador = 4,
                Nombre = "Marcela Pazmiño",
                Activo = true,
                Descripcion = "Alumno regular, requiere refuerzo"
            },
            new Alumno
            {
                Identificador = 5,
                Nombre = "Arturo Ureña",
                Activo = true,
                Descripcion = "Alumno regular, ha desertado en 2 ocasiones"
            },
            new Alumno
            {
                Identificador = 6,
                Nombre = "Lina Cachago",
                Activo = false,
                Descripcion = "Alumno no asiste desde segunda clase"
            }
        });

        RefrescarGrid(_alumnos);
        RefrescarDisponibles();
        RefrescarSeleccionados();
        RefrescarEstadisticas();
    }

    private void RefrescarGrid(IEnumerable<Alumno> elementos)
    {
        _dgvAlumnos.DataSource = null;
        _dgvAlumnos.DataSource = elementos.ToList();
        _dgvAlumnos.ClearSelection();
        RefrescarEstadisticas();
    }

    private void RefrescarDisponibles()
    {
        var disponibles = _alumnos
            .Where(alumno => alumno.Activo)
            .Where(alumno => !_identificadoresSeleccionados.Contains(alumno.Identificador))
            .OrderBy(alumno => alumno.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        _lstDisponibles.DataSource = null;
        _lstDisponibles.DataSource = disponibles;
        _lstDisponibles.DisplayMember = nameof(Alumno.Nombre);

        RefrescarEstadisticas();
    }

    private void RefrescarSeleccionados()
    {
        _cboSeleccionados.DataSource = null;
        _cboSeleccionados.DataSource = _seleccionados.ToList();
        _cboSeleccionados.DisplayMember = nameof(Alumno.Nombre);

        _btnRestaurarSeleccion.Enabled = true;
        _btnRestaurarSeleccion.ForeColor = Color.White;
        _btnRestaurarSeleccion.Cursor = _seleccionados.Count > 0
            ? Cursors.Hand
            : Cursors.Default;

        if (_seleccionados.Count == 0)
        {
            _txtDescripcionSeleccionado.Clear();
        }

        RefrescarEstadisticas();
    }

    private void RefrescarEstadisticas()
    {
        _lblTotalAlumnos.Text = _alumnos.Count.ToString(CultureInfo.InvariantCulture);
        _lblTotalActivos.Text = _alumnos.Count(alumno => alumno.Activo)
            .ToString(CultureInfo.InvariantCulture);
        _lblTotalSeleccionados.Text = _seleccionados.Count
            .ToString(CultureInfo.InvariantCulture);
    }

    private void MoverDisponibleASeleccionados()
    {
        if (_lstDisponibles.SelectedItem is not Alumno itemVisual)
        {
            return;
        }

        var alumno = _alumnos.FirstOrDefault(elemento =>
            elemento.Identificador == itemVisual.Identificador &&
            elemento.Activo);

        if (alumno is null ||
            !_identificadoresSeleccionados.Add(alumno.Identificador))
        {
            return;
        }

        _seleccionados.Add(alumno);
        RefrescarDisponibles();
        RefrescarSeleccionados();
        _cboSeleccionados.SelectedItem = alumno;
    }

    private void RestaurarSeleccionadoADisponibles()
    {
        if (_cboSeleccionados.SelectedItem is not Alumno alumnoSeleccionado)
        {
            return;
        }

        var eliminado = _seleccionados.RemoveAll(alumno =>
            alumno.Identificador == alumnoSeleccionado.Identificador);

        if (eliminado == 0)
        {
            return;
        }

        _identificadoresSeleccionados.Remove(alumnoSeleccionado.Identificador);

        RefrescarDisponibles();
        RefrescarSeleccionados();
    }

    private void MostrarDescripcionSeleccionado()
    {
        _txtDescripcionSeleccionado.Text =
            _cboSeleccionados.SelectedItem is Alumno alumno
                ? alumno.Descripcion
                : string.Empty;
    }

    private void OrdenarAlfabeticamente()
    {
        var ordenados = _alumnos
            .OrderBy(
                alumno => alumno.Nombre,
                StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        RefrescarGrid(ordenados);
    }

    private void AgregarAlumno()
    {
        
        var identificador = ObtenerSiguienteIdentificador();
        var nombre = _txtNombre.Text.Trim();
        var descripcion = _txtNuevaDescripcion.Text.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show(
                "Ingrese el nombre del alumno.",
                "Validación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _txtNombre.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            MessageBox.Show(
                "Ingrese la descripción del alumno.",
                "Validación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _txtNuevaDescripcion.Focus();
            return;
        }

        _alumnos.Add(new Alumno
        {
            Identificador = identificador,
            Nombre = nombre,
            Activo = _chkActivo.Checked,
            Descripcion = descripcion
        });

        RefrescarGrid(_alumnos);
        RefrescarDisponibles();
        LimpiarFormularioNuevoAlumno();
    }

    private int ObtenerSiguienteIdentificador()
    {
       
        return _alumnos.Count == 0
            ? 1
            : _alumnos.Max(alumno => alumno.Identificador) + 1;
    }

    private void LimpiarFormularioNuevoAlumno()
    {
        _txtNombre.Clear();
        _chkActivo.Checked = true;
        _txtNuevaDescripcion.Clear();
        _txtNombre.Focus();
    }

    private async Task ProbarConexionAsync()
    {
        CambiarEstadoBotonesBase(false);
        _lblEstadoBase.Text = "Conectando...";
        _lblEstadoBase.ForeColor = AzulPrincipal;

        try
        {
            var version = await _mariaDbService.ProbarConexionAsync();

            _lblEstadoBase.Text = $"Conexión correcta · {version}";
            _lblEstadoBase.ForeColor = VerdePrincipal;
        }
        catch (Exception ex)
        {
            _lblEstadoBase.Text = "Error de conexión";
            _lblEstadoBase.ForeColor = Color.Firebrick;

            MessageBox.Show(
                "No fue posible conectar con MariaDB. Verifique que MySQL esté iniciado en XAMPP, " +
                "que haya importado los scripts y que DatabaseSettings.cs tenga los datos correctos.\n\n" +
                ex.Message,
                "Error de base de datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            CambiarEstadoBotonesBase(true);
        }
    }

    private async Task ConsultarBandasAsync()
    {
        CambiarEstadoBotonesBase(false);

        _dgvResultadoBandas.DataSource = null;
        _lblEstadoConsulta.Text = "Estado: consultando...";
        _lblEstadoConsulta.ForeColor = AzulPrincipal;

        try
        {
            var bandas =
                await _mariaDbService.ObtenerCapitalPorBandasAsync();

            var filas = bandas
                .Select((item, indice) => new
                {
                    Numero = indice + 1,
                    Banda = item.Banda,
                    CapitalTotal = item.CapitalTotal
                })
                .ToList();

            _dgvResultadoBandas.DataSource = filas;
            _dgvResultadoBandas.ClearSelection();

            _lblEstadoConsulta.Text =
                filas.Count == 0
                    ? "Estado: consulta ejecutada sin resultados"
                    : $"Estado: consulta ejecutada correctamente · {filas.Count} bandas";

            _lblEstadoConsulta.ForeColor =
                filas.Count == 0
                    ? TextoSecundario
                    : VerdePrincipal;
        }
        catch (Exception ex)
        {
            _dgvResultadoBandas.DataSource = null;
            _lblEstadoConsulta.Text =
                "Estado: error al ejecutar la consulta";
            _lblEstadoConsulta.ForeColor = Color.Firebrick;

            MessageBox.Show(
                "No fue posible ejecutar la consulta. " +
                "Importe los tres scripts SQL en el orden indicado.\n\n" +
                ex.Message,
                "Error de consulta",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            CambiarEstadoBotonesBase(true);
        }
    }

    private void CambiarEstadoBotonesBase(bool habilitados)
    {
        _btnProbarConexion.Enabled = habilitados;
        _btnConsultarBandas.Enabled = habilitados;
    }

    private void CargarIconoFormulario()
    {
        var rutaLogo = Path.Combine(
            AppContext.BaseDirectory,
            "Images",
            "bdd.png");

        if (!File.Exists(rutaLogo))
        {
            return;
        }

        IntPtr manejador = IntPtr.Zero;

        try
        {
            using var imagenOriginal = new Bitmap(rutaLogo);
            using var imagenIcono = new Bitmap(imagenOriginal, new Size(32, 32));

            manejador = imagenIcono.GetHicon();

            using var iconoTemporal = Icon.FromHandle(manejador);
            Icon = (Icon)iconoTemporal.Clone();
            ShowIcon = true;
        }
        catch
        {
           
        }
        finally
        {
            if (manejador != IntPtr.Zero)
            {
                DestroyIcon(manejador);
            }
        }
    }

    private static void AplicarRegionCircular(Control control)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }

        using var ruta = new GraphicsPath();
        ruta.AddEllipse(0, 0, control.Width, control.Height);

        var regionAnterior = control.Region;
        control.Region = new Region(ruta);
        regionAnterior?.Dispose();
    }

    private sealed class DashboardCard : Panel
    {
        public int CornerRadius { get; set; } = 18;
        public Color BorderColor { get; set; } = Color.Gainsboro;
        public int BorderThickness { get; set; } = 1;

        public DashboardCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnResize(EventArgs eventArgs)
        {
            base.OnResize(eventArgs);
            ActualizarRegion();
        }

        protected override void OnPaint(PaintEventArgs eventArgs)
        {
            eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rectangulo = new Rectangle(
                0,
                0,
                Math.Max(1, Width - 1),
                Math.Max(1, Height - 1));

            using var ruta = CrearRutaRedondeada(rectangulo, CornerRadius);
            using var pincel = new SolidBrush(BackColor);
            using var lapiz = new Pen(BorderColor, BorderThickness);

            eventArgs.Graphics.FillPath(pincel, ruta);
            eventArgs.Graphics.DrawPath(lapiz, ruta);

            base.OnPaint(eventArgs);
        }

        private void ActualizarRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using var ruta = CrearRutaRedondeada(
                new Rectangle(0, 0, Width, Height),
                CornerRadius);

            var regionAnterior = Region;
            Region = new Region(ruta);
            regionAnterior?.Dispose();
        }
    }

    private sealed class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 11;

        public RoundedButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnResize(EventArgs eventArgs)
        {
            base.OnResize(eventArgs);

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using var ruta = CrearRutaRedondeada(
                new Rectangle(0, 0, Width, Height),
                CornerRadius);

            var regionAnterior = Region;
            Region = new Region(ruta);
            regionAnterior?.Dispose();
        }
    }

    private static GraphicsPath CrearRutaRedondeada(
        Rectangle rectangulo,
        int radio)
    {
        var ruta = new GraphicsPath();
        var diametro = Math.Max(2, radio * 2);

        if (rectangulo.Width < diametro || rectangulo.Height < diametro)
        {
            ruta.AddRectangle(rectangulo);
            ruta.CloseFigure();
            return ruta;
        }

        var arco = new Rectangle(
            rectangulo.Location,
            new Size(diametro, diametro));

        ruta.AddArc(arco, 180, 90);

        arco.X = rectangulo.Right - diametro;
        ruta.AddArc(arco, 270, 90);

        arco.Y = rectangulo.Bottom - diametro;
        ruta.AddArc(arco, 0, 90);

        arco.X = rectangulo.Left;
        ruta.AddArc(arco, 90, 90);

        ruta.CloseFigure();
        return ruta;
    }
}
