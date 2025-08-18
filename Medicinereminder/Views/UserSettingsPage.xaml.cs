using Medicinereminder.DataBase;

namespace Medicinereminder.Views;

public partial class UserSettingsPage : ContentPage
{
    private int _userId;
    public string UserName { get; set; }

    public UserSettingsPage(int userId, string userName)
    {
        InitializeComponent();
        _userId = userId;
        UserName = string.IsNullOrWhiteSpace(userName) ? "N/A" : userName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = this;

        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;
        ContentLayout.IsVisible = false;

        await Task.WhenAny(Task.Delay(400));

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