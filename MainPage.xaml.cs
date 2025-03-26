using budget.models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Item = budget.models.Item;

namespace budget
{
    public partial class MainPage : ContentPage
    {
        private double _originalRemainingBalance;
        private double _salary = 0;
        private double _remainingBalance = 0;

        private readonly AppDbContext _dbContext;
        private int _editItemId;
        private ObservableCollection<Item> _items;
        private double _balance;

        public MainPage()
        {
            InitializeComponent();
            _items = new ObservableCollection<Item>();
            ItemsListView.ItemsSource = _items;
            _dbContext = new AppDbContext();
            LoadItems();
            LoadUserData(GetNameLabel());
            SavingsSlider.ValueChanged += OnSavingsSliderChanged;
            BalanceText = $"Balance: ${_balance:F2}";

        }
        public string BalanceText
        {
            get => $"Balance: ${_balance:F2}";
            set
            {
                if (_balance.ToString() != value)
                {
                    _balance = double.Parse(value.Replace("Balance: $", "").Trim());
                    OnPropertyChanged(nameof(BalanceText));  // Notify the UI of the change
                }
            }
        }


        private async void LoadItems()
        {
            var user = await _dbContext.GetUser();

            var items = await _dbContext.GetItemsForUser(user.Id);
            foreach (var item in items)
            {
                // Add items to the UI list
                _items.Add(item);
            }
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Item tappedItem)
            {
                tappedItem.IsSelected = !tappedItem.IsSelected;

                // 🔹 Force UI update
                var index = _items.IndexOf(tappedItem);
                _items.Remove(tappedItem);
                _items.Insert(index, tappedItem);

                Console.WriteLine($"Item {tappedItem.Name} selection state: {tappedItem.IsSelected}");
            }
        }

        private async void OnSaveItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(NameEntry.Text))
            {
                await DisplayAlert("Error", "Name is required.", "OK");
                return;
            }

            if (!double.TryParse(EstimatedCostEntry.Text, out double estimatedCost))
            {
                await DisplayAlert("Error", "Estimated Cost must be a valid number.", "OK");
                return;
            }

            var user = await _dbContext.GetUser();
            if (user == null)
            {
                await DisplayAlert("Error", "User not found.", "OK");
                return;
            }


            string selectedCategory = CategoryEntry.SelectedItem?.ToString() ?? "Bill";
            string selectedStatus = StatusPicker.SelectedItem?.ToString() ?? "Not Paid";

            var item = new Item
            {
                UserId = user.Id,
                Name = NameEntry.Text,
                Description = DescriptionEntry.Text,
                Category = selectedCategory,
                Priority = PriorityEntry.Text,
                EstimatedCost = estimatedCost,
                CreatedAt = CreatedAtPicker.Date,
                IsSelected = false,
                Status = selectedStatus,
                IsRecurring = IsRecurringSwitch.IsToggled,
                RecurrenceInterval = RecurrencePicker.SelectedItem?.ToString() ?? "None",
                NextDueDate = IsRecurringSwitch.IsToggled ? NextDueDatePicker.Date : (DateTime?)null
            };

            var result = await _dbContext.Create(item);
            if (result > 0)
            {
                _items.Add(item);
                await DisplayAlert("Success", "Item saved successfully.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to save item.", "OK");
            }
        }


        private async void OnDeleteItemClicked(object sender, EventArgs e)
        {
            var itemsToDelete = _items.Where(item => item.IsSelected).ToList();

            if (!itemsToDelete.Any())
            {
                await DisplayAlert("Error", "No items selected for deletion.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirm Delete",
                $"Are you sure you want to delete {itemsToDelete.Count} item(s)?", "Yes", "No");

            if (!confirm) return;

            foreach (var item in itemsToDelete)
            {
                await _dbContext.Delete(item);
                _items.Remove(item);
            }

            await DisplayAlert("Success", "Selected items deleted successfully.", "OK");
        }

        private async void OnNavigateToOtherPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Itemviewing());
        }

        private async void OnSetSalaryClicked(object sender, EventArgs e)
        {
            // Validate the salary input
            if (!double.TryParse(SalaryEntry.Text, out _salary) || _salary <= 0)
            {
                await DisplayAlert("Error", "Please enter a valid salary.", "OK");
                return;
            }

            var user = await _dbContext.GetUser();

            if (user == null)
            {
                // New user gets their salary as initial balance
                user = new User
                {
                    Name = "Default User",
                    birth = DateTime.Now,
                    Salary = _salary,
                    Balance = _salary,  // Set balance to salary initially
                    LastUpdated = DateTime.Now
                };

                await _dbContext.CreateUser(user);
            }
            else
            {
                // Update salary and reset balance to salary
                user.Salary = _salary;
                user.Balance = _salary;  // Reset the balance to the salary value

                await _dbContext.UpdateUser(user);
            }

            // Update UI to show the new salary and balance
            RemainingBalanceLabel.Text = $"Remaining: ${user.Balance:F2}";
            await DisplayAlert("Success", "Salary updated successfully.", "OK");
        }








        private DateTime GetNextDueDate(string recurrenceInterval, DateTime? currentDueDate)
        {
            if (currentDueDate == null) return DateTime.Now;

            switch (recurrenceInterval.ToLower())
            {
                case "monthly":
                    return currentDueDate.Value.AddMonths(1);
                case "quarter":
                    return currentDueDate.Value.AddMonths(3);
                case "semester":
                    return currentDueDate.Value.AddMonths(6);
                case "weekly":
                    return currentDueDate.Value.AddDays(7);
                case "yearly":
                    return currentDueDate.Value.AddYears(1);
                default:
                    return currentDueDate.Value; 
            }
        }
        private async void OnApplySavingsClicked(object sender, EventArgs e)
        {
            var user = await _dbContext.GetUser();
            if (user == null)
            {
                await DisplayAlert("Error", "User data not found.", "OK");
                return;
            }

            // Ensure the user has a savings balance field (initialize if missing)
            if (user.SavingsBalance == null)
                user.SavingsBalance = 0;

            // Reset the savings balance to 0 before applying new savings
            user.SavingsBalance = 0;

            // Reset the balance to the salary before applying savings
            user.Balance = user.Salary;

            // Calculate savings based on the salary
            double savingsAmount = (user.Salary * SavingsPercentage) / 100;

            // Deduct savings from the balance
            user.Balance -= savingsAmount;

            // Add savings amount to the separate savings balance
            user.SavingsBalance += savingsAmount;

            // Save the updated user data
            await _dbContext.UpdateUser(user);

            // Update the UI
            RemainingBalanceLabel.Text = $"Remaining: ${user.Balance:F2}";
            SavingsBalanceLabel.Text = $"Savings: ${user.SavingsBalance:F2}"; // Display savings balance

            // Display success message
            await DisplayAlert("Success", "Savings applied successfully!", "OK");
        }






        private void OnDistributeWeeklyClicked(object sender, EventArgs e)
        {
            double weeklyAllowance = _remainingBalance / 4;
            AllowanceLabel.Text = $"Weekly Allowance: ${weeklyAllowance:F2}";
        }

        private void OnDistributeMonthlyClicked(object sender, EventArgs e)
        {
            AllowanceLabel.Text = $"Monthly Allowance: ${_remainingBalance:F2}";
        }

        private Label GetNameLabel()
        {
            return NameLabel;
        }

        private async void LoadUserData(Label nameLabel)
        {
            var user = await _dbContext.GetUser();
            if (user == null)
            {
                user = new User
                {
                    Name = "Default User",
                    birth = DateTime.Now,
                    Salary = 0,
                    Balance = 0,
                    LastUpdated = DateTime.Now
                };
                await _dbContext.CreateUser(user);
            }

            nameLabel.Text = $"Welcome, {user.Name}!";

            _salary = user.Salary;
            _remainingBalance = user.Balance;

            SalaryEntry.Text = _salary > 0 ? _salary.ToString("F2") : "";
            RemainingBalanceLabel.Text = $"Remaining: ${_remainingBalance:F2}";
        }



        private async void OnPageAppearing(object sender, EventArgs e)
        {
            var user = await _dbContext.GetUser();
            if (user != null)
            {
                DateTime now = DateTime.Now;
                DateTime lastUpdated = user.LastUpdated;

                // Check if the current month is different from the last update month
                if (lastUpdated.Month != now.Month || lastUpdated.Year != now.Year)
                {
                    // Reset balance to salary at the start of each new month
                    user.Balance = user.Salary;

                    // Apply savings to the new salary (not balance)
                    double savingsAmount = (user.Salary * SavingsPercentage) / 100;
                    user.SavingsBalance += savingsAmount;

                    // Update last updated date
                    user.LastUpdated = now;

                    // Process bills (deduct from balance)
                    var items = await _dbContext.GetItemsForUser(user.Id);
                    foreach (var item in items.Where(i => i.IsRecurring))
                    {
                        // Check if bills are due and deduct them from balance
                        while (item.NextDueDate <= now)
                        {
                            user.Balance -= item.EstimatedCost;
                            item.NextDueDate = GetNextDueDate(item.RecurrenceInterval, item.NextDueDate);
                            await _dbContext.Update(item); // Update the next due date for the recurring item
                        }
                    }

                    await _dbContext.UpdateUser(user);
                    RemainingBalanceLabel.Text = $"Remaining: ${user.Balance:F2}";
                }
            }
        }


        private double _savingsPercentage = 10;

        public double SavingsPercentage
        {
            get => _savingsPercentage;
            set
            {
                if (_savingsPercentage != value)
                {
                    _savingsPercentage = value;
                    OnPropertyChanged(nameof(SavingsPercentage));
                    SavingsPercentageLabel.Text = $"Saving: {value:F0}%";
                }
            }
        }

        private void OnSavingsSliderChanged(object? sender, ValueChangedEventArgs e)
        {
            SavingsPercentage = e.NewValue;
        }


    }
}
