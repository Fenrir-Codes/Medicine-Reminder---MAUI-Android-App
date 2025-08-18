using Medicinereminder.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Medicinereminder.Views;

public partial class MedicineDetailsPage : ContentPage
{
    private int _medicineId;
    public string? _day;
    public string? _dosage;
    public string? _timing;
    public string? FullInfo;

    public MedicineDetailsPage(int medicineId)
    {
        InitializeComponent();
        _medicineId = medicineId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = this;
        SchedulesCollectionView.ItemsSource = null;

        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        ContentLayout.IsVisible = false;

        var loadTask = LoadDetailsAsync();

        await Task.WhenAll(loadTask, Task.Delay(400));

        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
        ContentLayout.IsVisible = true;
    }

    #region Load Details Async
    private async Task LoadDetailsAsync()
    {
        using (var db = new AppDbContext())
        {
            var medicine = await db.Medicines
                .Include(m => m.Schedules)
                .FirstOrDefaultAsync(m => m.MedicineId == _medicineId);

            if (medicine != null)
            {
                NameLabel.Text = medicine.Name;

                var firstSchedule = medicine.Schedules?.FirstOrDefault();
                //DosageLabel.Text = firstSchedule?.Dosage ?? "N/A";

                // Készítünk egy listaelemet minden naphoz, ahol az idõpontokat felbontjuk
                var scheduleList = medicine.Schedules?
                    .Select(s => new
                    {
                        Day = s.Day.ToString(),
                        Dosage = s.Dosage ?? "N/A",
                        // Idõpontok összefûzve egy stringgé
                        Time = s.Times, // már string, pl. "08:00, 12:00, 18:00"
                        _day = $"{s.Day}",
                        _dosage = $"{s.Dosage}",
                        _timing = $"{s.Times}"
                        //FullInfo = $"{s.Day} - {s.Dosage} - {s.Times}"
                    })
                    .ToList();

                SchedulesCollectionView.ItemsSource = scheduleList;
            }
        }
    }
    #endregion

    #region on Refresh Button Clicked
    private async void OnRefreshButtonClicked(object sender, EventArgs e)
    {
        // Clear existing items
        SchedulesCollectionView.ItemsSource = null;
        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        ContentLayout.IsVisible = false;

        await LoadDetailsAsync();

        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
        ContentLayout.IsVisible = true;
    }
    #endregion

    #region On Back Button Clicked
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    #endregion

    #region On Edit Button Clicked
    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MedicineEditPage(_medicineId));
    }
    #endregion

}
