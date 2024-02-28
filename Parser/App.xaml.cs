namespace Parser
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            this.UserAppTheme = AppTheme.Light;
            MainPage = new AppShell();
        }
    }
}
