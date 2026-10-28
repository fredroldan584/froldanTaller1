namespace froldanTaller1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // la app arranca primero en el login
        return new Window(new LoginPage());
    }
}
