using Medicinereminder.DataBase;
using Medicinereminder.Models;
using System.Collections.ObjectModel;

namespace Medicinereminder.Views
{
    public partial class MedicineEditPage : ContentPage
    {
        private int _medicineId;
        private Dictionary<DayOfWeek, string> _dailyDosages = new Dictionary<DayOfWeek, string>();
        private ObservableCollection<TimeSpan> _timeList = new ObservableCollection<TimeSpan>();
        private readonly Dictionary<string, (Picker tabletPicker, Picker mgPicker)> _pickerMapping;
        private readonly Dictionary<DayOfWeek, CheckBox> _daySwitches;

        public MedicineEditPage(int medicineId)
        {
            InitializeComponent();
            _medicineId = medicineId;

            TimeListView.ItemsSource = _timeList;

            // Picker mapping
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

            // Day switches
            _daySwitches = new Dictionary<DayOfWeek, CheckBox>
            {
                { DayOfWeek.Monday, MondaySwitch },
                { DayOfWeek.Tuesday, TuesdaySwitch },
                { DayOfWeek.Wednesday, WednesdaySwitch },
                { DayOfWeek.Thursday, ThursdaySwitch },
                { DayOfWeek.Friday, FridaySwitch },
                { DayOfWeek.Saturday, SaturdaySwitch },
                { DayOfWeek.Sunday, SundaySwitch }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;


            LoadingSpinner.IsRunning = true;
            LoadingSpinner.IsVisible = true;
            ContentLayout.IsVisible = false;

            await LoadMedicineDetailsAsync();

            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            ContentLayout.IsVisible = true;

        }

        #region Load Medicine Details From DB
        private async Task LoadMedicineDetailsAsync()
        {
            using (var db = new AppDbContext())
            {
                var medicine = await db.Medicines.FindAsync(_medicineId);
                if (medicine == null)
                    return;

                MedicineNameEntry.Text = medicine.Name;

                // Betöltjük a schedule-okat
                var schedules = db.MedicineSchedules.Where(s => s.MedicineId == _medicineId).ToList();

                foreach (var schedule in schedules)
                {
                    // Dózisok szétbontása tablet/mg formátumban
                    var parts = (schedule.Dosage ?? "").Split(' ');
                    string tablet = parts.Length > 0 ? parts[0] : "";
                    string mg = parts.Length > 1 ? parts[1] : "";

                    _dailyDosages[schedule.Day] = schedule.Dosage;

                    if (_pickerMapping.TryGetValue(schedule.Day + "Tablet", out var pickers))
                    {
                        pickers.tabletPicker.SelectedItem = tablet;
                        pickers.mgPicker.SelectedItem = mg;
                    }

                    if (_daySwitches.TryGetValue(schedule.Day, out var daySwitch))
                    {
                        daySwitch.IsChecked = true;
                    }

                    // Idõk betöltése, pl.: "08:00,12:00" -> TimeSpan list
                    if (!string.IsNullOrEmpty(schedule.Times))
                    {
                        var times = schedule.Times.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => TimeSpan.Parse(t.Trim()));
                        foreach (var time in times)
                        {
                            if (!_timeList.Contains(time))
                                _timeList.Add(time);
                        }
                    }
                }
            }
        }
        #endregion

        #region Day Dosage Selected
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

                _dailyDosages[day] = string.Join(" ", new[] { tablet, mg }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
        }
        #endregion

        #region CheckBox Checked
        private void CheckBoxChecked(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Parent is Grid parentGrid)
            {
                parentGrid.BackgroundColor = e.Value ? Colors.DarkSeaGreen : Colors.White;
                checkBox.Color = e.Value ? Colors.White : Colors.Black;
            }
        }
        #endregion

        #region Add/Remove Time
        private void OnAddTimeClicked(object sender, EventArgs e)
        {
            var selectedTime = NewTimePicker.Time;
            if (!_timeList.Contains(selectedTime))
                _timeList.Add(selectedTime);
        }

        private void OnRemoveTimeClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is TimeSpan time)
                _timeList.Remove(time);
        }
        #endregion

        #region On Save Clicked
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            using (var db = new AppDbContext())
            {
                var medicine = await db.Medicines.FindAsync(_medicineId);
                if (medicine == null)
                    return;

                medicine.Name = MedicineNameEntry.Text;

                // Töröljük a régi schedule-okat
                var oldSchedules = db.MedicineSchedules.Where(s => s.MedicineId == _medicineId);
                db.MedicineSchedules.RemoveRange(oldSchedules);

                // Új schedule-ok mentése
                var selectedDays = _daySwitches.Where(kv => kv.Value.IsChecked).Select(kv => kv.Key).ToList();
                if (!selectedDays.Any())
                {
                    await DisplayAlert("Message", "Choose at least one day!", "OK");
                    return;
                }

                var timesString = string.Join(",", _timeList.Select(t => t.ToString(@"hh\:mm")));

                foreach (var day in selectedDays)
                {
                    _dailyDosages.TryGetValue(day, out string? dosage);
                    dosage ??= "N/A";

                    var schedule = new MedicineSchedule
                    {
                        MedicineId = _medicineId,
                        Day = day,
                        Dosage = dosage,
                        Times = timesString
                    };
                    db.MedicineSchedules.Add(schedule);
                }

                await db.SaveChangesAsync();
                await Navigation.PopAsync();
            }
        }
        #endregion

        #region On Back Clicked
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        #endregion

    }

}
