using Microsoft.Maui.Controls;
using budget.models;
using System;

namespace budget
{
    public partial class SignUpPage : ContentPage
    {
        private readonly AppDbContext _dbContext;

        public SignUpPage()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            // Check if user already exists
            var existingUser = await _dbContext.GetUserByEmail(email);
            if (existingUser != null)
            {
                await DisplayAlert("Error", "User with this email already exists.", "OK");
                return;
            }

            // Create new user
            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = password,  // In a real app, you should hash the password
                birth = DateTime.Now
            };

            await _dbContext.CreateUser(user);
            await DisplayAlert("Success", "Account created successfully!", "OK");

            // Navigate to Login page
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
