using System.Text;
using System.Text.RegularExpressions;

namespace froldanTaller1;

public partial class MainPage : ContentPage
{
    // Archivo TXT donde se guardan todas las inscripciones.
    // En Windows queda en la carpeta Documentos; en Android, en la carpeta de datos de la app.
    private readonly string rutaArchivo = Path.Combine(
        DeviceInfo.Platform == DevicePlatform.WinUI
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : FileSystem.AppDataDirectory,
        "froldanTaller1_inscripciones.txt");

    public MainPage()
    {
        InitializeComponent();
        // No se permite elegir una fecha de nacimiento futura
        dpFechaNacimiento.MaximumDate = DateTime.Today;
    }

    // ===== INTERFAZ ADAPTABLE =====
    // En celular ocupa todo el ancho; en pantallas grandes se limita a 700 px y se centra.
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            contenedor.WidthRequest = Math.Min(width, 700);
    }

    // ===== VALIDACIÓN 1: SOLO LETRAS (permite tildes, ñ y espacios) =====
    private void SoloLetras_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        string limpio = Regex.Replace(e.NewTextValue ?? "", @"[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ ]", "");
        if (limpio != e.NewTextValue)
            entry.Text = limpio;
    }

    // ===== VALIDACIÓN 2: SOLO NÚMEROS =====
    private void SoloNumeros_TextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        string limpio = Regex.Replace(e.NewTextValue ?? "", @"[^0-9]", "");
        if (limpio != e.NewTextValue)
            entry.Text = limpio;
    }

    // Ajusta las reglas del campo según el tipo de identificación
    private void pkTipoId_SelectedIndexChanged(object sender, EventArgs e)
    {
        txtIdentificacion.Text = "";
        switch (pkTipoId.SelectedItem?.ToString())
        {
            case "CÉDULA":
                txtIdentificacion.Keyboard = Keyboard.Numeric;
                txtIdentificacion.MaxLength = 10;
                txtIdentificacion.Placeholder = "Cédula (10 dígitos)";
                break;
            case "RUC":
                txtIdentificacion.Keyboard = Keyboard.Numeric;
                txtIdentificacion.MaxLength = 13;
                txtIdentificacion.Placeholder = "RUC (13 dígitos, termina en 001)";
                break;
            default:
                txtIdentificacion.Keyboard = Keyboard.Default;
                txtIdentificacion.MaxLength = 20;
                txtIdentificacion.Placeholder = "Pasaporte (letras y números)";
                break;
        }
    }

    // Cédula y RUC: solo números. Pasaporte: letras y números.
    private void txtIdentificacion_TextChanged(object sender, TextChangedEventArgs e)
    {
        string tipo = pkTipoId.SelectedItem?.ToString() ?? "";
        if (tipo == "CÉDULA" || tipo == "RUC")
        {
            SoloNumeros_TextChanged(sender, e);
        }
        else
        {
            string limpio = Regex.Replace(e.NewTextValue ?? "", @"[^a-zA-Z0-9]", "").ToUpper();
            if (limpio != e.NewTextValue)
                txtIdentificacion.Text = limpio;
        }
    }

    // ===== BOTÓN GUARDAR DATOS (VALIDACIÓN 4: enviar todo a un archivo TXT) =====
    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string? error = Validar();
        if (error != null)
        {
            await DisplayAlertAsync("Validación", error, "OK");
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("========== INSCRIPCIÓN UISRAEL ==========");
            sb.AppendLine($"Fecha de registro: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Tipo de identificación: {pkTipoId.SelectedItem}");
            sb.AppendLine($"Identificación: {txtIdentificacion.Text}");
            sb.AppendLine($"Apellidos: {txtApellidoPaterno.Text} {txtApellidoMaterno.Text}");
            sb.AppendLine($"Nombres: {txtNombres.Text}");
            sb.AppendLine($"Género: {pkGenero.SelectedItem}");
            sb.AppendLine($"Fecha de nacimiento: {dpFechaNacimiento.Date:dd/MM/yyyy}");
            sb.AppendLine($"Estado civil: {pkEstadoCivil.SelectedItem}");
            sb.AppendLine($"Teléfono: {txtTelefono.Text}");
            sb.AppendLine($"Correo: {txtCorreo.Text?.Trim()}");
            sb.AppendLine($"Carrera: {pkCarrera.SelectedItem}");
            sb.AppendLine($"Modalidad: {pkModalidad.SelectedItem}");
            sb.AppendLine($"Campus: {pkCampus.SelectedItem}");
            sb.AppendLine();

            // Agrega el registro al final del archivo (no borra los anteriores)
            await File.AppendAllTextAsync(rutaArchivo, sb.ToString());

            await DisplayAlertAsync("Datos guardados",
                $"La inscripción de {txtNombres.Text} {txtApellidoPaterno.Text} se guardó en:\n{rutaArchivo}",
                "OK");

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "No se pudo guardar el archivo: " + ex.Message, "OK");
        }
    }

    // Abre el archivo TXT con el programa predeterminado (Bloc de notas en Windows)
    private async void btnVerArchivo_Clicked(object sender, EventArgs e)
    {
        if (!File.Exists(rutaArchivo))
        {
            await DisplayAlertAsync("Archivo", "Todavía no hay inscripciones guardadas.", "OK");
            return;
        }
        await Launcher.Default.OpenAsync(new OpenFileRequest("Inscripciones", new ReadOnlyFile(rutaArchivo)));
    }

    // Revisa todos los campos. Devuelve el mensaje de error o null si todo está bien.
    private string? Validar()
    {
        if (pkTipoId.SelectedIndex == -1)
            return "Seleccione el tipo de identificación.";

        string id = txtIdentificacion.Text ?? "";
        string tipo = pkTipoId.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(id))
            return "Ingrese su número de identificación.";

        if (tipo == "CÉDULA")
        {
            if (!Regex.IsMatch(id, @"^\d{10}$"))
                return "La cédula debe tener exactamente 10 números.";
            if (!CedulaValida(id))
                return "La cédula ingresada no es válida.";
        }
        else if (tipo == "RUC")
        {
            if (!Regex.IsMatch(id, @"^\d{13}$"))
                return "El RUC debe tener exactamente 13 números.";
            if (!id.EndsWith("001"))
                return "El RUC debe terminar en 001.";
        }
        else if (!Regex.IsMatch(id, @"^[A-Z0-9]{5,20}$"))
        {
            return "El pasaporte debe tener entre 5 y 20 letras o números.";
        }

        if (string.IsNullOrWhiteSpace(txtApellidoPaterno.Text) || string.IsNullOrWhiteSpace(txtApellidoMaterno.Text))
            return "Ingrese ambos apellidos.";

        if (string.IsNullOrWhiteSpace(txtNombres.Text))
            return "Ingrese sus nombres completos.";

        if (pkGenero.SelectedIndex == -1)
            return "Seleccione su género.";

        if (pkEstadoCivil.SelectedIndex == -1)
            return "Seleccione su estado civil.";

        if (!Regex.IsMatch(txtTelefono.Text ?? "", @"^\d{10}$"))
            return "El teléfono debe tener 10 números.";

        string correo = (txtCorreo.Text ?? "").Trim();
        string confirmar = (txtConfirmarCorreo.Text ?? "").Trim();

        if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return "Ingrese un correo válido.";

        // ===== VALIDACIÓN 3: QUE COINCIDAN LOS CORREOS =====
        if (!string.Equals(correo, confirmar, StringComparison.OrdinalIgnoreCase))
            return "Los correos no coinciden.";

        if (pkCarrera.SelectedIndex == -1 || pkModalidad.SelectedIndex == -1 || pkCampus.SelectedIndex == -1)
            return "Seleccione carrera, modalidad y campus.";

        return null;
    }

    // Validación de cédula ecuatoriana (algoritmo módulo 10)
    private static bool CedulaValida(string cedula)
    {
        int provincia = int.Parse(cedula.Substring(0, 2));
        if (provincia < 1 || (provincia > 24 && provincia != 30)) return false;
        if (cedula[2] - '0' > 5) return false;

        int suma = 0;
        for (int i = 0; i < 9; i++)
        {
            int digito = cedula[i] - '0';
            if (i % 2 == 0)
            {
                digito *= 2;
                if (digito > 9) digito -= 9;
            }
            suma += digito;
        }
        int verificador = (10 - (suma % 10)) % 10;
        return verificador == cedula[9] - '0';
    }

    private void LimpiarFormulario()
    {
        pkTipoId.SelectedIndex = -1;
        txtIdentificacion.Text = "";
        txtApellidoPaterno.Text = "";
        txtApellidoMaterno.Text = "";
        txtNombres.Text = "";
        pkGenero.SelectedIndex = -1;
        dpFechaNacimiento.Date = DateTime.Today;
        pkEstadoCivil.SelectedIndex = -1;
        txtTelefono.Text = "";
        txtCorreo.Text = "";
        txtConfirmarCorreo.Text = "";
        pkCarrera.SelectedIndex = -1;
        pkModalidad.SelectedIndex = -1;
        pkCampus.SelectedIndex = -1;
    }
}
