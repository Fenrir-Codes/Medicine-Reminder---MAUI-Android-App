using Medicinereminder.DataBase;
using Medicinereminder.Models;
using Microsoft.EntityFrameworkCore;

namespace Medicinereminder.Views;

public partial class MedicineListPage : ContentPage
{
    private int _userId;
    public string _userName;

    public MedicineListPage(int userId, string userName)
    {
        InitializeComponent();
        _userId = userId;
        _userName = string.IsNullOrWhiteSpace(userName) ? "N/A" : userName;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        PageTopLabel.Text = $"{_userName}'s List";
        await LoadMedicines();
    }

    #region Load medicines
    private async Task LoadMedicines()
    {
        using (var db = new AppDbContext())
        {
            var medicines = await db.Medicines.Where(m => m.UserId == _userId).ToListAsync();

            if (medicines.Any())
            {
                InfoLabel.IsVisible = false;
                MedicinesCollectionView.ItemsSource = medicines;
            }
            else
            {
                InfoLabel.IsVisible = true;
                InfoLabel.Text = $"{_userName}, haven't added any medication yet.";
            }

            MedicinesCollectionView.ItemsSource = medicines;
        }
    }
    #endregion

    #region On Medicine Selected
    private async void OnMedicineSelected(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Medicine selectedMedicine)
        {
            await Navigation.PushAsync(new MedicineDetailsPage(selectedMedicine.MedicineId));
        }
    }
    #endregion

    #region On Medicine Clicked
    private async void OnAddMedicineClicked(object sender, EventArgs e)
    {
        // Új gyógyszer hozzáadása - átadjuk a userId-t
        await Navigation.PushAsync(new MedicineEntryPage(_userId));
    }
    #endregion

    #region On Back Button Clicked
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    #endregion

    #region On Settings Icon Clicked
    private async void OnSettingsIconClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserSettingsPage(_userId));
    }
    #endregion

}
