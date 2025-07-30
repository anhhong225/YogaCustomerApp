namespace YogaCustomerApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnListAllClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ClassList(), true);
        }
    }

}
