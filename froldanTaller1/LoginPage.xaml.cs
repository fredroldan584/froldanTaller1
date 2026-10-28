using System.Security.Cryptography;
using System.Text;

namespace froldanTaller1;

public partial class LoginPage : ContentPage
{
    // Credenciales de acceso.
    // La contraseña NO se guarda en texto plano: solo se guarda su hash SHA-256 (protección de datos).
    // Usuario: froldan   |   Contraseña: Uisrael2026
    private const string UsuarioValido = "froldan";
    private const string HashClaveValida = "97f0324e31f28a7cc08c534cfffd829b2aad42cb3d586f9b7e143fc7503b39e4";

    private const int MaxIntentos = 3;
    private int intentos = 0;

    public LoginPage()
    {
        InitializeComponent();
    }

    // Interfaz adaptable: ancho máximo de 450 px en pantallas grandes
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0)
            contenedor.WidthRequest = Math.Min(width, 450);
    }

    private async void btnIngresar_Clicked(object sender, EventArgs e)
    {
        string usuario = (txtUsuario.Text ?? "").Trim();
        string clave = txtClave.Text ?? "";

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            lblMensaje.Text = "Ingrese usuario y contraseña.";
            return;
        }

        if (usuario == UsuarioValido && CalcularHash(clave) == HashClaveValida)
        {
            lblMensaje.Text = "";
            await DisplayAlertAsync("Bienvenido", $"Acceso correcto. Hola, {usuario}.", "Continuar");

            // Reemplaza la pantalla de login por el formulario
            Application.Current!.Windows[0].Page = new AppShell();
            return;
        }

        intentos++;
        txtClave.Text = "";

        if (intentos >= MaxIntentos)
        {
            btnIngresar.IsEnabled = false;
            lblMensaje.Text = "Demasiados intentos fallidos. Acceso bloqueado.";
        }
        else
        {
            lblMensaje.Text = $"Usuario o contraseña incorrectos. Intentos restantes: {MaxIntentos - intentos}";
        }
    }

    // Convierte la contraseña en su hash SHA-256
    private static string CalcularHash(string texto)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        return Convert.ToHexString(bytes).ToLower();
    }
}
