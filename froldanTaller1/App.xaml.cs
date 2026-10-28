namespace froldanTaller1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // La app arranca en la pantalla de login.
        // Al ingresar correctamente, el login cambia la pantalla al formulario (AppShell).
        return new Window(new LoginPage());
    }
}
