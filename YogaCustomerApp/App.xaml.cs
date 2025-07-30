namespace YogaCustomerApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var naviPage = new NavigationPage(new MainPage());
            //naviPage.BarBackgroundColor = Colors.Teal;
            MainPage = naviPage;
        }
    }
}
