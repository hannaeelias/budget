using Microsoft.Maui.Controls;
using budget.models;
using System;

namespace budget
{
    public partial class LoginPage : ContentPage
    {
        private readonly AppDbContext _dbContext;

        public LoginPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            // Check if user exists
            var user = await _dbContext.GetUserByEmail(email);
            if (user == null || user.PasswordHash != password)  // In a real app, you'd hash and compare passwords
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
                return;
            }

            // Login successful, navigate to the home page
            await Navigation.PushAsync(new MainPage());
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }
    }
}
