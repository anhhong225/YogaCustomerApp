using Firebase.Database;
using System.Collections.ObjectModel;
using YogaCustomerApp.Objects;

namespace YogaCustomerApp;

public partial class ClassList : ContentPage
{
    private FirebaseClient firebaseClient = new FirebaseClient("https://yogaadminfirebase-default-rtdb.asia-southeast1.firebasedatabase.app/");
    public ObservableCollection<ClassItem> AllClassItems { get; set; } = new();
    public ObservableCollection<ClassItem> FilteredClassItems { get; set; } = new();
    public ObservableCollection<ClassItem> ShoppingCart { get; set; } = new();

    private HashSet<string> selectedDays = new();
    private DateTime? selectedDate = null;

    public ClassList()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            var rawCourses = await firebaseClient
                .Child("YogaCourse")
                .OnceAsync<YogaCourse>();

            var schedules = await firebaseClient
                .Child("Schedule")
                .OnceAsync<Schedule>();

            var courses = rawCourses.Where(c => c.Object != null).ToList();

            AllClassItems.Clear();

            foreach (var s in schedules)
            {
                var course = courses.FirstOrDefault(c => c.Object.id == s.Object.yogaCourseId)?.Object;

                if (course != null && s.Object != null)
                {
                    AllClassItems.Add(new ClassItem
                    {
                        Schedule = s.Object,
                        Course = course
                    });
                }
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load data:\n{ex.Message}", "OK");
        }
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        selectedDate = e.NewDate;
        selectedDateLabel.Text = $"Selected: {selectedDate:dd-MM-yyyy}";
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        string keyword = searchBar.Text?.ToLower() ?? "";

        var filtered = AllClassItems.Where(item =>
            (selectedDays.Count == 0 || selectedDays.Contains(item.Course.dayofweek)) &&
            (!selectedDate.HasValue || item.Schedule.date == selectedDate.Value.ToString("dd-MM-yyyy")) &&
            (string.IsNullOrEmpty(keyword) ||
            item.Course.type?.ToLower().Contains(keyword) == true ||
            item.Course.dayofweek?.ToLower().Contains(keyword) == true ||
            item.Course.time?.ToLower().Contains(keyword) == true)

        ).ToList();

        FilteredClassItems.Clear();
        foreach (var item in filtered)
            FilteredClassItems.Add(item);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnDayFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string shortDay = button.Text;

            string fullDay = shortDay switch
            {
                "M" => "Mon",
                "T" => "Tue",
                "W" => "Wed",
                "Th" => "Thu",
                "F" => "Fri",
                "Sa" => "Sat",
                "Su" => "Sun",
                _ => null
            };

            if (fullDay == null)
                return;

            // Toggle selected day and update visual style
            if (selectedDays.Contains(fullDay))
            {
                selectedDays.Remove(fullDay);
                button.BackgroundColor = Colors.White;
                button.TextColor = Colors.Black;
            }
            else
            {
                selectedDays.Add(fullDay);
                button.BackgroundColor = Colors.DodgerBlue;
                button.TextColor = Colors.White;
            }

            ApplyFilters();
        }
    }

    private async void OnCalendarClicked(object sender, EventArgs e)
    {
        var picked = await DisplayDatePicker();
        if (picked.HasValue)
        {
            selectedDate = picked.Value;
            selectedDateLabel.Text = $"Selected: {selectedDate:dd-MM-yyyy}";
            ApplyFilters();
        }
    }

    private async Task<DateTime?> DisplayDatePicker()
    {
        var tcs = new TaskCompletionSource<DateTime?>();

        var datePicker = new DatePicker
        {
            Format = "yyyy-MM-dd",
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var okButton = new Button { Text = "OK" };
        okButton.Clicked += (s, e) => tcs.SetResult(datePicker.Date);

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Clicked += (s, e) => tcs.SetResult(null);

        var popup = new ContentPage
        {
            Content = new StackLayout
            {
                Margin = 30,
                Children = { datePicker, new StackLayout { Orientation = StackOrientation.Horizontal, Children = { okButton, cancelButton } } }
            }
        };

        await Navigation.PushModalAsync(popup);
        var date = await tcs.Task;
        await Navigation.PopModalAsync();
        return date;
    }
    private void OnClearFiltersClicked(object sender, EventArgs e)
    {
        selectedDays.Clear();
        selectedDate = null;
        searchBar.Text = string.Empty;

        // Reset button highlights
        foreach (var child in DayButtonStack.Children)
        {
            if (child is Button b)
            {
                b.BackgroundColor = Colors.White;
                b.TextColor = Colors.Black;
            }
        }

        ApplyFilters();
    }

    private void OnAddToCartClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is ClassItem item)
        {
            bool alreadyInCart = ShoppingCart.Any(c => c.Schedule.id == item.Schedule.id);

            if (!alreadyInCart)
            {
                ShoppingCart.Add(item);
                DisplayAlert("Cart", $"Added {item.Course.type} on {item.Schedule.date} to cart", "OK");
            }
            else
            {
                DisplayAlert("Already Added", "This class is already in your cart.", "OK");
            }
        }
    }

    private async void OnViewCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(ShoppingCart));
    }
}
