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

        public MainPage()
        {
            InitializeComponent();
            _items = new ObservableCollection<Item>();
            ItemsListView.ItemsSource = _items;
            _dbContext = new AppDbContext();
            LoadItems();
            LoadUserData();
            SavingsSlider.ValueChanged += OnSavingsSliderChanged;
        }

        private async void LoadItems()
        {
            var items = await _dbContext.GetItemsForUser();
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

            string selectedCategory = CategoryEntry.SelectedItem?.ToString() ?? "Bill";
            string selectedStatus = StatusPicker.SelectedItem?.ToString() ?? "Not Paid";

            var item = new Item
            {
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
            if (!double.TryParse(SalaryEntry.Text, out _salary) || _salary <= 0)
            {
                await DisplayAlert("Error", "Please enter a valid salary.", "OK");
                return;
            }

            // Save salary in user data
            var user = await _dbContext.GetUser();
            if (user == null)
            {
                user = new User { Name = "Default User", birth = DateTime.Now };
                await _dbContext.CreateUser(user);
            }

            user.Salary = _salary; 
            await _dbContext.UpdateUser(user);

            double totalBills = _items.Sum(i => i.EstimatedCost);
            _remainingBalance = _salary - totalBills;
            _originalRemainingBalance = _remainingBalance;

            RemainingBalanceLabel.Text = $"Remaining: ${_remainingBalance:F2}";
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

        private void OnSavingsSliderChanged(object? sender, ValueChangedEventArgs e)
        {
            SavingsPercentageLabel.Text = $"Saving: {e.NewValue:F0}%";
        }

        private void OnApplySavingsClicked(object sender, EventArgs e)
        {
            _remainingBalance = _originalRemainingBalance;

            double savingsAmount = (_remainingBalance * SavingsSlider.Value) / 100;
            _remainingBalance -= savingsAmount;

            RemainingBalanceLabel.Text = $"Remaining: ${_remainingBalance:F2}";
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

        private async void LoadUserData()
        {
            var user = await _dbContext.GetUser();
            if (user != null)
            {
                NameLabel.Text = "Hello, " + user.Name;
                SalaryEntry.Text = user.Salary.ToString();
            }
            else
            {
                NameLabel.Text = "Hello, " + "unknown";
                SalaryEntry.Text = "1000";
            }
        }

        private async void OnPageAppearing(object sender, EventArgs e)
        {
            var user = await _dbContext.GetUser();
            if (user != null)
            {
                _salary = user.Salary; 

                var items = await _dbContext.GetItemsForUser();
                foreach (var item in items.Where(i => i.IsRecurring))
                {
                    if (item.NextDueDate <= DateTime.Now)
                    {
                        item.NextDueDate = GetNextDueDate(item.RecurrenceInterval, item.NextDueDate);
                        await _dbContext.Update(item); 
                    }
                }

                double totalBills = items.Sum(i => i.EstimatedCost);
                _remainingBalance = _salary - totalBills;
                _originalRemainingBalance = _remainingBalance;
                RemainingBalanceLabel.Text = $"Remaining: ${_remainingBalance:F2}";
            }
        }

    }
}
