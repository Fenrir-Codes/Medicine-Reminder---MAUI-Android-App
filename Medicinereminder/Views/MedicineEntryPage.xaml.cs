using Medicinereminder.DataBase;
using Medicinereminder.Models;
using System.Collections.ObjectModel;

namespace Medicinereminder.Views;

public partial class MedicineEntryPage : ContentPage
{
    private int _userId;
    private Dictionary<DayOfWeek, string> _dailyDosages = new Dictionary<DayOfWeek, string>();
    private ObservableCollection<TimeSpan> _timeList = new ObservableCollection<TimeSpan>();
    private readonly Dictionary<string, (Picker tabletPicker, Picker mgPicker)> _pickerMapping;

    public MedicineEntryPage(int userId)
    {
        InitializeComponent();
        _userId = userId;

        TimeListView.ItemsSource = _timeList;

        // Picker mapping inicializálása
        _pickerMapping = new Dictionary<string, (Picker, Picker)>
        {
            { "MondayTablet", (MondayTabletPicker, MondayMgPicker) },
            { "MondayMg", (MondayTabletPicker, MondayMgPicker) },

            { "TuesdayTablet", (TuesdayTabletPicker, TuesdayMgPicker) },
            { "TuesdayMg", (TuesdayTabletPicker, TuesdayMgPicker) },

            { "WednesdayTablet", (WednesdayTabletPicker, WednesdayMgPicker) },
            { "WednesdayMg", (WednesdayTabletPicker, WednesdayMgPicker) },

            { "ThursdayTablet", (ThursdayTabletPicker, ThursdayMgPicker) },
            { "ThursdayMg", (ThursdayTabletPicker, ThursdayMgPicker) },

            { "FridayTablet", (FridayTabletPicker, FridayMgPicker) },
            { "FridayMg", (FridayTabletPicker, FridayMgPicker) },

            { "SaturdayTablet", (SaturdayTabletPicker, SaturdayMgPicker) },
            { "SaturdayMg", (SaturdayTabletPicker, SaturdayMgPicker) },

            { "SundayTablet", (SundayTabletPicker, SundayMgPicker) },
            { "SundayMg", (SundayTabletPicker, SundayMgPicker) },
        };
    }

    #region On Add Time Clicked
    private void OnAddTimeClicked(object sender, EventArgs e)
    {
        var selectedTime = NewTimePicker.Time;
        if (!_timeList.Contains(selectedTime))
            _timeList.Add(selectedTime);
    }
    #endregion

    #region On Remove Time Clicked
    private void OnRemoveTimeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is TimeSpan time)
            _timeList.Remove(time);
    }
    #endregion

    #region Day Dosage selector
    private void DayDosageSelected(object sender, EventArgs e)
    {
        if (sender is Picker picker && _pickerMapping.TryGetValue(picker.StyleId, out var pickers))
        {
            DayOfWeek day = picker.StyleId switch
            {
                var s when s.StartsWith("Monday") => DayOfWeek.Monday,
                var s when s.StartsWith("Tuesday") => DayOfWeek.Tuesday,
                var s when s.StartsWith("Wednesday") => DayOfWeek.Wednesday,
                var s when s.StartsWith("Thursday") => DayOfWeek.Thursday,
                var s when s.StartsWith("Friday") => DayOfWeek.Friday,
                var s when s.StartsWith("Saturday") => DayOfWeek.Saturday,
                _ => DayOfWeek.Sunday
            };

            string tablet = pickers.tabletPicker.SelectedItem?.ToString() ?? "";
            string mg = pickers.mgPicker.SelectedItem?.ToString() ?? "";

            // Only include non-empty values, no extra comma
            _dailyDosages[day] = string.Join(" ", new[] { tablet, mg }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
    #endregion

    #region On Save Clicked
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Check if the medicine name field is empty
            if (string.IsNullOrWhiteSpace(MedicineNameEntry?.Text))
            {
                await DisplayAlert("Message", "Please type the name of the medicine!", "OK");
                return;
            }

            // Map each day of the week to its corresponding CheckBox
            var daySwitches = new Dictionary<DayOfWeek, CheckBox>
            {{ DayOfWeek.Monday, MondaySwitch },
            { DayOfWeek.Tuesday, TuesdaySwitch },
            { DayOfWeek.Wednesday, WednesdaySwitch },
            { DayOfWeek.Thursday, ThursdaySwitch },
            { DayOfWeek.Friday, FridaySwitch },
            { DayOfWeek.Saturday, SaturdaySwitch },
            { DayOfWeek.Sunday, SundaySwitch }};

            // Get the list of days that are selected (checked)
            var selectedDays = daySwitches
                .Where(kv => kv.Value.IsChecked)
                .Select(kv => kv.Key)
                .ToList();

            // If no day is selected, show a message and stop
            if (!selectedDays.Any())
            {
                await DisplayAlert("Message", "Choose at least one day!", "OK");
                return;
            }

            // If no time is added to the list, show a message and stop
            if (!_timeList.Any())
            {
                await DisplayAlert("Message", "Add at least one time!", "OK");
                return;
            }

            // Perform database operations asynchronously
            using (var db = new AppDbContext())
            {
                // Create a new Medicine object
                var medicine = new Medicine
                {
                    Name = MedicineNameEntry.Text, // Set the medicine name
                    UserId = _userId               // Set the user ID
                };
                db.Medicines.Add(medicine);       // Add the medicine to the database
                await db.SaveChangesAsync();      // Save changes asynchronously to get the generated MedicineId

                // Convert the list of times into a single string like "06:00, 12:00"
                var timesString = string.Join(", ", _timeList.Select(t => t.ToString(@"hh\:mm")));

                // Loop through all selected days and create schedules
                foreach (var day in selectedDays)
                {
                    // Try to get the dosage for this day; if not found, use empty string
                    _dailyDosages.TryGetValue(day, out string? dosage);
                    dosage ??= "N/A";

                    // Create a new MedicineSchedule object
                    var schedule = new MedicineSchedule
                    {
                        MedicineId = medicine.MedicineId, // Link schedule to the medicine
                        Day = day,                        // Set the day
                        Times = timesString,              // Set the times string
                        Dosage = dosage                   // Set the dosage
                    };
                    db.MedicineSchedules.Add(schedule);  // Add schedule to the database
                }

                // Save all schedules to the database asynchronously
                await db.SaveChangesAsync();

                // Show success message to the user
                await DisplayAlert("Success", "Medicine Added", "OK");

                // Navigate back to the previous page
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            // If any error occurs, display the error message
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
    #endregion

    #region Checkbox checked
    private void CheckBoxChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Parent is Grid parentGrid)
        {
            parentGrid.BackgroundColor = e.Value ? Colors.DarkSeaGreen : Colors.White;
            checkBox.Color = e.Value ? Colors.White : Colors.Black;
        }
    }
    #endregion

    #region On Back Button Clicked
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    #endregion

}
