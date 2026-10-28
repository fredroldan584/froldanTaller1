namespace froldanTaller1;

public partial class LoginPage : ContentPage
{
    // usuario y clave para entrar al sistema
    string usuarioValido = "froldan";
    string claveValida = "Uisrael2026";

    // contador de intentos, al tercer error se bloquea el boton
    int intentos = 0;
    int maxIntentos = 3;

    public LoginPage()
    {
        InitializeComponent();
    }

    // para que el login no se estire tanto en pantallas grandes
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            contenedor.WidthRequest = Math.Min(width, 450);
    }

    // boton INGRESAR: compara el usuario y la clave
    private async void btnIngresar_Clicked(object sender, EventArgs e)
    {
        string usuario = (txtUsuario.Text ?? "").Trim();
        string clave = txtClave.Text ?? "";

        if (usuario == "" || clave == "")
        {
            lblMensaje.Text = "Ingrese usuario y contraseña.";
            return;
        }

        if (usuario == usuarioValido && clave == claveValida)
        {
            lblMensaje.Text = "";
            await DisplayAlertAsync("Bienvenido", "Acceso correcto. Hola, " + usuario + ".", "Continuar");
            // si todo esta bien cambia a la pantalla del formulario
            Application.Current!.Windows[0].Page = new AppShell();
            return;
        }

        // si se equivoca suma un intento y borra la clave
        intentos = intentos + 1;
        txtClave.Text = "";

        if (intentos >= maxIntentos)
        {
            btnIngresar.IsEnabled = false;
            lblMensaje.Text = "Demasiados intentos fallidos. Acceso bloqueado.";
        }
        else
        {
            lblMensaje.Text = "Usuario o contraseña incorrectos. Intentos restantes: " + (maxIntentos - intentos);
        }
    }
}
