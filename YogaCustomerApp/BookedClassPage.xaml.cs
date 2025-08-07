using Firebase.Database;
using System.Collections.ObjectModel;
using YogaCustomerApp.Objects;

namespace YogaCustomerApp;

public partial class BookedClassPage : ContentPage
{
    private FirebaseClient firebaseClient = new FirebaseClient("https://yogaadminfirebase-default-rtdb.asia-southeast1.firebasedatabase.app/");
    public ObservableCollection<ClassItem> UserBookings { get; set; } = new();
    String userEmail;
    public BookedClassPage(string email)
	{
		InitializeComponent();
        userEmail = email;
        BindingContext = this;
        LoadUserBookings(userEmail);
    }
    private async void LoadUserBookings(string userEmail)
    {
        var bookings = await firebaseClient
            .Child("Bookings")
            .OnceAsync<Booking>();

        var schedules = await firebaseClient
            .Child("Schedule")
            .OnceAsync<Schedule>();

        var courses = await firebaseClient
            .Child("YogaCourse")
            .OnceAsync<YogaCourse>();

        var bookingList = bookings
            .Where(b => b.Object.customerEmail == userEmail)
            .Select(b => b.Object)
            .ToList();

        UserBookings.Clear();
        if(bookingList != null)
        {
            foreach (var booking in bookingList)
            {
                if (!string.IsNullOrEmpty(booking.scheduleId) &&
                booking.scheduleId.StartsWith("schedule_") &&
                int.TryParse(booking.scheduleId.Replace("schedule_", ""), out int parsedId))
                {
                    var schedule = schedules.FirstOrDefault(s => s.Object.id == parsedId)?.Object;
                    var course = courses.FirstOrDefault(c => c.Object.id == schedule?.yogaCourseId)?.Object;

                    if (schedule != null && course != null)
                    {
                        UserBookings.Add(new ClassItem
                        {
                            Schedule = schedule,
                            Course = course
                        });
                    }
                }
            }
        }
        else
        {
            
        }
    }
    private void OnBackToClassesClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ClassList());
    }

}