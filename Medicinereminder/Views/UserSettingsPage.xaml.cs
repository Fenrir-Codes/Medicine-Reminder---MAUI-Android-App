using Medicinereminder.DataBase;
using Medicinereminder.Models;
using Microsoft.EntityFrameworkCore;

namespace Medicinereminder.Views;

public partial class UserSettingsPage : ContentPage
{
    private int _userId;
    private string _userName = "";
    public string UserName
    {
        get => _userName;
        set { _userName = value; OnPropertyChanged(); }
    }

    public UserSettingsPage(int userId)
    {
        InitializeComponent();
        _userId = userId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = this;

        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        ContentLayout.IsVisible = false;

        var getUser = GetUserInfo();
        await Task.WhenAll(getUser, Task.Delay(400));

        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
        ContentLayout.IsVisible = true;
    }

    #region Change Username From Prompth
    private async Task<string?> GetUserNameFromPromptAsync()
    {
        return await DisplayPromptAsync(
            "Change Username",
            "Please type a new Username:",
            "Save",
            "Cancel",
            placeholder: "Type a new username",
            maxLength: 30,
            keyboard: Keyboard.Text);
    }
    #endregion

    private async Task GetUserInfo()
    {
        try
        {
            using (var db = new AppDbContext())
            {
                var userData = await db.Users.FirstOrDefaultAsync(u => u.UserId == _userId);

                if (userData != null)
                {
                    UserName = userData.Name ?? "N/A";
                }
                else
                {
                    await DisplayAlert("Message", "Something went wrong, no user found!", "Ok");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Database error: {ex.Message}", "Ok");
        }
    }

    private async Task<List<Medicine>> GetUserMedicines(int userId)
    {
        using (var db = new AppDbContext())
        {
            var medicines = await db.Medicines.Where(m => m.UserId == userId).ToListAsync();
            return medicines;
        }
    }

    #region On Update User Clicked
    private async void OnUpdateUserClicked(object sender, EventArgs e)
    {
        using (var db = new AppDbContext())
        {
            string? changedUserName = await GetUserNameFromPromptAsync();

            if (changedUserName == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(changedUserName))
            {
                await DisplayAlert("Message", "Username cannot be empty!", "OK");
                return;
            }

            var user = await db.Users.FindAsync(_userId);
            if (user != null)
            {
                user.Name = changedUserName; // Setting the new name
                await db.SaveChangesAsync();

                await DisplayAlert("Success", "Username changed", "Ok");
                await Navigation.PopToRootAsync();
            }
            else
            {
                await DisplayAlert("Error", "User not found", "Ok");
            }

        }
    }
    #endregion

    #region On Delete User Clicked
    private async void OnDeleteUserClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Message", "Are you sure you want to delete this user account?", "Yes", "Cancel");

        if (!confirm)
            return;

        bool success = await DeleteUser();

        if (success)
            await Navigation.PopToRootAsync();
        else
            await DisplayAlert("Message", "No user found or could not be deleted.", "OK");
    }
    #endregion

    #region Delete User
    private async Task<bool> DeleteUser()
    {
        try
        {
            using (var db = new AppDbContext())
            {
                var user = await db.Users.FindAsync(_userId);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return true; // success
                }
            }
        }
        catch (Exception)
        {
            return false; // Error happend
        }

        return false; // No user found
    }
    #endregion

    #region On Back Button Clicked
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    #endregion

}