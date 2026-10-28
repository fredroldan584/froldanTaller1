namespace froldanTaller1;

public partial class MainPage : ContentPage
{
    // ruta del archivo txt donde se guardan las inscripciones (en Documentos)
    string rutaArchivo = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "froldanTaller1_inscripciones.txt");

    public MainPage()
    {
        InitializeComponent();
        // para que no se pueda escoger una fecha de nacimiento futura
        dpFechaNacimiento.MaximumDate = DateTime.Today;
    }

    // hace que el formulario se adapte: en el celular usa todo el ancho
    // y en una pantalla grande no pasa de 700 px
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            contenedor.WidthRequest = Math.Min(width, 700);
    }

    // validacion solo letras: si se escribe un numero o simbolo se regresa al texto anterior
    private void SoloLetras_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry entry = (Entry)sender;
        string texto = e.NewTextValue ?? "";

        foreach (char c in texto)
        {
            if (!char.IsLetter(c) && c != ' ')
            {
                entry.Text = e.OldTextValue;
                return;
            }
        }
    }

    // validacion solo numeros (telefono)
    private void SoloNumeros_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry entry = (Entry)sender;
        string texto = e.NewTextValue ?? "";

        foreach (char c in texto)
        {
            if (!char.IsDigit(c))
            {
                entry.Text = e.OldTextValue;
                return;
            }
        }
    }

    // segun lo que se escoja en el picker cambia el limite de digitos y el teclado
    private void pkTipoId_SelectedIndexChanged(object sender, EventArgs e)
    {
        txtIdentificacion.Text = "";
        string tipo = pkTipoId.SelectedItem?.ToString() ?? "";

        if (tipo == "CÉDULA")
        {
            txtIdentificacion.Keyboard = Keyboard.Numeric;
            txtIdentificacion.MaxLength = 10;
            txtIdentificacion.Placeholder = "Cédula (10 dígitos)";
        }
        else if (tipo == "RUC")
        {
            txtIdentificacion.Keyboard = Keyboard.Numeric;
            txtIdentificacion.MaxLength = 13;
            txtIdentificacion.Placeholder = "RUC (13 dígitos, termina en 001)";
        }
        else
        {
            txtIdentificacion.Keyboard = Keyboard.Default;
            txtIdentificacion.MaxLength = 20;
            txtIdentificacion.Placeholder = "Pasaporte (letras y números)";
        }
    }

    // cedula y ruc solo aceptan numeros, el pasaporte acepta letras y numeros
    private void txtIdentificacion_TextChanged(object sender, TextChangedEventArgs e)
    {
        string tipo = pkTipoId.SelectedItem?.ToString() ?? "";
        string texto = e.NewTextValue ?? "";

        foreach (char c in texto)
        {
            bool permitido;
            if (tipo == "CÉDULA" || tipo == "RUC")
                permitido = char.IsDigit(c);
            else
                permitido = char.IsLetterOrDigit(c);

            if (!permitido)
            {
                txtIdentificacion.Text = e.OldTextValue;
                return;
            }
        }
    }

    // boton GUARDAR DATOS: primero valida todo y si esta bien lo guarda en el txt
    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        string error = Validar();
        if (error != "")
        {
            await DisplayAlertAsync("Validación", error, "OK");
            return;
        }

        // armo el texto con todos los datos del formulario
        string datos =
            "========== INSCRIPCIÓN UISRAEL ==========\n" +
            "Fecha de registro: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\n" +
            "Tipo de identificación: " + pkTipoId.SelectedItem + "\n" +
            "Identificación: " + txtIdentificacion.Text + "\n" +
            "Apellidos: " + txtApellidoPaterno.Text + " " + txtApellidoMaterno.Text + "\n" +
            "Nombres: " + txtNombres.Text + "\n" +
            "Género: " + pkGenero.SelectedItem + "\n" +
            "Fecha de nacimiento: " + dpFechaNacimiento.Date?.ToString("dd/MM/yyyy") + "\n" +
            "Estado civil: " + pkEstadoCivil.SelectedItem + "\n" +
            "Teléfono: " + txtTelefono.Text + "\n" +
            "Correo: " + txtCorreo.Text.Trim() + "\n" +
            "Carrera: " + pkCarrera.SelectedItem + "\n" +
            "Modalidad: " + pkModalidad.SelectedItem + "\n" +
            "Campus: " + pkCampus.SelectedItem + "\n\n";

        try
        {
            // AppendAllText agrega al final del archivo, asi no se borran los registros anteriores
            File.AppendAllText(rutaArchivo, datos);

            await DisplayAlertAsync("Datos guardados",
                "La inscripción de " + txtNombres.Text + " " + txtApellidoPaterno.Text +
                " se guardó en:\n" + rutaArchivo, "OK");

            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "No se pudo guardar el archivo: " + ex.Message, "OK");
        }
    }

    // abre el txt con el bloc de notas para ver lo que se guardo
    private async void btnVerArchivo_Clicked(object sender, EventArgs e)
    {
        if (!File.Exists(rutaArchivo))
        {
            await DisplayAlertAsync("Archivo", "Todavía no hay inscripciones guardadas.", "OK");
            return;
        }

        await Launcher.Default.OpenAsync(new OpenFileRequest("Inscripciones", new ReadOnlyFile(rutaArchivo)));
    }

    // revisa todos los campos uno por uno
    // si algo esta mal devuelve el mensaje de error, si todo esta bien devuelve vacio
    private string Validar()
    {
        string tipo = pkTipoId.SelectedItem?.ToString() ?? "";
        string id = txtIdentificacion.Text ?? "";

        if (tipo == "")
            return "Seleccione el tipo de identificación.";

        if (id == "")
            return "Ingrese su número de identificación.";

        if (tipo == "CÉDULA")
        {
            if (id.Length != 10)
                return "La cédula debe tener exactamente 10 números.";
            if (!CedulaValida(id))
                return "La cédula ingresada no es válida.";
        }

        if (tipo == "RUC")
        {
            if (id.Length != 13)
                return "El RUC debe tener exactamente 13 números.";
            if (!id.EndsWith("001"))
                return "El RUC debe terminar en 001.";
        }

        if (tipo == "PASAPORTE" && (id.Length < 5 || id.Length > 20))
            return "El pasaporte debe tener entre 5 y 20 letras o números.";

        if (string.IsNullOrWhiteSpace(txtApellidoPaterno.Text) || string.IsNullOrWhiteSpace(txtApellidoMaterno.Text))
            return "Ingrese ambos apellidos.";

        if (string.IsNullOrWhiteSpace(txtNombres.Text))
            return "Ingrese sus nombres completos.";

        if (pkGenero.SelectedIndex == -1)
            return "Seleccione su género.";

        if (pkEstadoCivil.SelectedIndex == -1)
            return "Seleccione su estado civil.";

        if ((txtTelefono.Text ?? "").Length != 10)
            return "El teléfono debe tener 10 números.";

        string correo = (txtCorreo.Text ?? "").Trim();
        string confirmar = (txtConfirmarCorreo.Text ?? "").Trim();

        if (!correo.Contains("@") || !correo.Contains("."))
            return "Ingrese un correo válido.";

        // validacion de que los dos correos sean iguales
        if (correo.ToLower() != confirmar.ToLower())
            return "Los correos no coinciden.";

        if (pkCarrera.SelectedIndex == -1 || pkModalidad.SelectedIndex == -1 || pkCampus.SelectedIndex == -1)
            return "Seleccione carrera, modalidad y campus.";

        return "";
    }

    // valida que la cedula sea real con el algoritmo del modulo 10
    // los 2 primeros digitos son la provincia y el ultimo es el digito verificador
    private bool CedulaValida(string cedula)
    {
        int provincia = int.Parse(cedula.Substring(0, 2));
        if (provincia < 1 || (provincia > 24 && provincia != 30))
            return false;

        int tercerDigito = int.Parse(cedula.Substring(2, 1));
        if (tercerDigito > 5)
            return false;

        int suma = 0;
        for (int i = 0; i < 9; i++)
        {
            int digito = int.Parse(cedula.Substring(i, 1));

            // los digitos en posicion par se multiplican por 2 y si pasan de 9 se les resta 9
            if (i % 2 == 0)
            {
                digito = digito * 2;
                if (digito > 9)
                    digito = digito - 9;
            }

            suma = suma + digito;
        }

        int verificador = 10 - (suma % 10);
        if (verificador == 10)
            verificador = 0;

        int ultimoDigito = int.Parse(cedula.Substring(9, 1));
        return verificador == ultimoDigito;
    }

    // deja el formulario vacio despues de guardar
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
