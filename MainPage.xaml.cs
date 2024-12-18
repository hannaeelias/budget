using budget.models;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.Linq;
using System.Threading.Tasks;
using Item = budget.models.Item;

namespace budget
{
    public partial class MainPage : ContentPage
    {
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
        }

        private async void LoadItems()
        {
            _items.Clear();
            var items = await _dbContext.GetItems();
            foreach (var item in items)
            {
                item.IsSelected = false; // Reset selection state
                _items.Add(item);
            }
        }
        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Item tappedItem)
            {
                tappedItem.IsSelected = !tappedItem.IsSelected;
                Console.WriteLine($"Item {tappedItem.Name} selection state: {tappedItem.IsSelected}");
            }
        }

        private async void OnSaveItemClicked(object sender, EventArgs e)
        {
            // Validation for required fields
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
            string selectedStatus = StatusPicker.SelectedItem?.ToString() ?? "In Progress";
            var item = new Item
            {
                Name = NameEntry.Text,
                Description = DescriptionEntry.Text,
                Category = CategoryEntry.Text,
                Priority = PriorityEntry.Text,
                EstimatedCost = estimatedCost, 
                CreatedAt = CreatedAtPicker.Date, 
                IsSelected = false,
                PhotoPath = PhotoImage.Source?.ToString() ?? string.Empty,
                Status = selectedStatus
            };

            var items = await _dbContext.GetItems();
            var existingItem = items.FirstOrDefault(i => i.Name == item.Name);

            if (existingItem == null)
            {
                await _dbContext.Create(item);
                _items.Add(item); 
                await DisplayAlert("Success", "Item saved successfully.", "OK");
            }
            else
            {
                // Item already exists, show a warning
                await DisplayAlert("Warning", "An item with the same name already exists. please choose a diffrent name.", "OK");
            }
        }



        private async void OnSelectPhotoClicked(object sender, EventArgs e)
        {
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a Photo"
            });

            if (photo != null)
            {
                // Display the selected photo
                var photoStream = await photo.OpenReadAsync();
                PhotoImage.Source = ImageSource.FromStream(() => photoStream);

                // Save photo path to the item
                string photoPath = photo.FullPath;
                // You can save the photo path to your database if necessary
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
                _items.Remove(item); // Remove from the UI
            }

            await DisplayAlert("Success", "Selected items deleted successfully.", "OK");
        }


        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Item selectedItem)
            {
                _editItemId = selectedItem.Id;
                NameEntry.Text = selectedItem.Name;
                DescriptionEntry.Text = selectedItem.Description;
                CategoryEntry.Text = selectedItem.Category;
                PriorityEntry.Text = selectedItem.Priority;
                EstimatedCostEntry.Text = selectedItem.EstimatedCost.ToString();
            }
        }
        private async void OnNavigateToOtherPageClicked(object sender, EventArgs e)
        {
            // Navigate to OtherPage
            await Navigation.PushAsync(new Itemviewing());
        }


    }
}
