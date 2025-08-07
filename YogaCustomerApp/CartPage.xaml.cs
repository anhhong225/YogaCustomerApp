using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.ObjectModel;
using YogaCustomerApp.Objects;

namespace YogaCustomerApp;

public partial class CartPage : ContentPage
{
    private Button submitButton;
    private ListView cartListView;

    public ObservableCollection<ClassItem> ShoppingCart { get; set; }

    public CartPage(ObservableCollection<ClassItem> cartItems)
    {
        ShoppingCart = cartItems;
        nameEntry = new Entry { Placeholder = "Your Name" };
        emailEntry = new Entry { Placeholder = "Your Email", Keyboard = Keyboard.Email };
        submitButton = new Button { Text = "Submit Booking", BackgroundColor = Colors.Orange, TextColor = Colors.White };
        submitButton.Clicked += OnSubmitClicked;

        cartListView = new ListView
        {
            ItemsSource = ShoppingCart,
            ItemTemplate = new DataTemplate(() =>
            {
                var cell = new TextCell();
                cell.SetBinding(TextCell.TextProperty, "Course.type");
                cell.SetBinding(TextCell.DetailProperty, "Schedule.date");
                return cell;
            })
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Children =
                {
                    new Label { Text = "Your Cart", FontSize = 22, HorizontalOptions = LayoutOptions.Center },
                    cartListView,
                    nameEntry,
                    emailEntry,
                    submitButton
                }
            }
        };
    }
    private async void SubmitCart(string customerName, string customerEmail)
    {
        var firebaseClient = new FirebaseClient("https://yogaadminfirebase-default-rtdb.asia-southeast1.firebasedatabase.app/");

        // Get existing bookings to determine the next index
        var existingBookings = await firebaseClient
            .Child("Bookings")
            .OnceAsync<Booking>();

        int maxBookingIndex = 0;
        foreach (var b in existingBookings)
        {
            if (b.Key.StartsWith("booking_") &&
                int.TryParse(b.Key.Replace("booking_", ""), out int number))
            {
                if (number > maxBookingIndex) maxBookingIndex = number;
            }
        }

        int bookingCounter = maxBookingIndex + 1;

        // Create one booking entry per schedule
        foreach (var item in ShoppingCart)
        {
            string bookingIndex = $"booking_{bookingCounter++}";
            var booking = new Booking
            {
                customerEmail = customerEmail,
                customerName = customerName,
                bookingDate = DateTime.Now.ToString("dd-MM-yyyy"),
                status = "Booked",
                scheduleId = $"schedule_{item.Schedule.id}"
            };

            await firebaseClient
                .Child("Bookings")
                .Child(bookingIndex)
                .PutAsync(booking);
        }

        ShoppingCart.Clear();
        await DisplayAlert("Success", "Your classes have been booked!", "OK");
        await Navigation.PushAsync(new BookedClassPage(customerEmail));
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        string name = nameEntry.Text?.Trim();
        string email = emailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            DisplayAlert("Error", "Please enter both name and email", "OK");
            return;
        }

        SubmitCart(name, email);
    }
}