using Medicinereminder.DataBase;
using Medicinereminder.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Medicinereminder
{
    public partial class MainPage : ContentPage
    {

        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public bool canAddUser { get; set; } = true;
        public bool isDeleteMode { get; set; } = false;
        public int _userCount = 0;
        public string? _userCountText;


        public MainPage()
        {
            InitializeComponent();
            EnsureDbCreated();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;

            LoadingSpinner.IsRunning = true;
            LoadingSpinner.IsVisible = true;
            ContentLayout.IsVisible = false;

            await LoadUsers();

            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
            ContentLayout.IsVisible = true;
        }

        #region Ensure Database is Created
        private void EnsureDbCreated()
        {
            using (var db = new AppDbContext())
            {
                bool created = db.Database.EnsureCreated();
            }
        }
        #endregion

        #region Load Users
        private async Task LoadUsers()
        {
            UsersCollectionView.ItemsSource = null;

            // Open a new database context to work with the data
            using (var db = new AppDbContext())
            {
                // Count how many users are currently in the database
                _userCount = await db.Users.CountAsync();
                _userCountLabel.Text = $"{_userCount}/10";
                _userCountLabel.TextColor = _userCount >= 10 ? Colors.Red : Colors.GreenYellow;

                // Set a flag to allow adding new users only if there are less than 10
                canAddUser = _userCount < 10;

                // Retrieve the list of all users from the database asynchronously
                var users = await db.Users.ToListAsync();

                // Clear the local ObservableCollection to prepare for refreshing the UI
                _users.Clear();

                // Add each user from the database into the local collection
                foreach (var user in users)
                    _users.Add(user);

                // Update the CollectionView to display the current list of users
                UsersCollectionView.ItemsSource = _users;
            }
        }
        #endregion

        #region Get Username From Prompth
        private async Task<string?> GetUserNameFromPromptAsync()
        {
            return await DisplayPromptAsync(
                "New User",
                "Please type a Username:",
                "Add",
                "Cancel",
                placeholder: "Username",
                maxLength: 30,
                keyboard: Keyboard.Text);
        }
        #endregion

        #region On Add User Clicked
        private async void OnAddUserClicked(object sender, EventArgs e)
        {

            // Create a new instance of the database context
            using (var db = new AppDbContext())
            {
                // Count the current number of users in the database
                _userCount = await db.Users.CountAsync();

                // If there are already 10 or more users, show a message and exit
                if (_userCount >= 10)
                {
                    await DisplayAlert("Message", "A maximum of 10 users can be added.", "OK");
                    return;
                }

                // Prompt the user to enter a new username
                string? userName = await GetUserNameFromPromptAsync();

                // If the user clicked Cancel or closed the prompt, exit the method
                if (userName == null)
                {
                    return;
                }

                // If the entered username is empty or whitespace, show an alert and exit
                if (string.IsNullOrWhiteSpace(userName))
                {
                    await DisplayAlert("Message", "Username cannot be empty!", "OK");
                    return;
                }

                // Create a new User object with the trimmed username
                var user = new User { Name = userName.Trim() };

                // Add the new user to the database
                db.Users.Add(user);

                // Save changes asynchronously to the database
                await db.SaveChangesAsync();

                // Refresh the UI to show the updated list of users
                await LoadUsers();
            }
        }
        #endregion

        #region On User Clicked
        private async void OnUserSelected(object sender, EventArgs e)
        {
            // Check if the sender is a Button and its BindingContext is a User object
            if (sender is Button btn && btn.BindingContext is User user)
            {
                // If so, navigate to the MedicineListPage, passing the user's ID and Name
                await Navigation.PushAsync(new Views.MedicineListPage(user.UserId, user.Name));
            }
        }
        #endregion

        #region On Delete Button Clicked
        private async void OnDeleteDatabaseClicked(object sender, EventArgs e)
        {
            // Show a confirmation dialog to the user with "Yes" and "Cancel" buttons
            bool answer = await DisplayAlert("Confirm Delete", "Are you sure you want to delete the database?", "Yes", "Cancel");

            // If the user clicked "Cancel", exit the method without doing anything
            if (!answer)
            {
                return; // Felhasználó Cancel-t nyomott, nem történik semmi
            }

            // If the user confirmed, call the method to delete the database
            await DeleteDatabase();
            // Show a message confirming that the database has been deleted
            await DisplayAlert("Message", "The database has been deleted.", "OK");
        }
        #endregion

        #region Delete database
        private async Task DeleteDatabase()
        {
            // Build the full path to the database file
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "medicinreminder.db3");

            // Check if the database file exists
            if (File.Exists(dbPath))
            {
                try
                {
                    // Attempt to delete the database file
                    File.Delete(dbPath);
                }
                catch (Exception ex)
                {
                    // If an error occurs during deletion, show an error message
                    await DisplayAlert("Error", "Error while deleting: " + ex.Message, "OK");
                }
            }
            else
            {
                // If the database file doesn't exist, show a message
                await DisplayAlert("Message", "No database found.", "OK");
            }

            // Refresh the UI after deletion (e.g., reload the user list)
            await LoadUsers();
        }
        #endregion

    }
}
