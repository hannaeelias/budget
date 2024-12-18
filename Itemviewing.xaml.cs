using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using budget.models;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Item = budget.models.Item;

namespace budget;

public partial class Itemviewing : ContentPage
{
    private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
    private readonly AppDbContext _dbContext;

    public Itemviewing()
    {
        InitializeComponent();
        _dbContext = new AppDbContext(); 
        LoadItems();
        ItemsListView.ItemsSource = _items;
    }


    private async void LoadItems()
    {
        if (_dbContext == null)
        {
            await DisplayAlert("Error", "Database context is not initialized.", "OK");
            return;
        }

        var itemsFromDb = await _dbContext.GetItems();  
        foreach (var item in itemsFromDb)
        {
            _items.Add(item); 
        }
    }

    private void OnSortByNameClicked(object sender, EventArgs e)
    {
        var sortedItems = _items.OrderBy(item => item.Name).ToList();
        ItemsListView.ItemsSource = new ObservableCollection<Item>(sortedItems);
    }

    private void OnSortByPriorityClicked(object sender, EventArgs e)
    {
        var sortedItems = _items.OrderBy(item => item.Priority).ToList();
        ItemsListView.ItemsSource = new ObservableCollection<Item>(sortedItems);
    }

    private void OnSortByEstimatedCostClicked(object sender, EventArgs e)
    {
        var sortedItems = _items.OrderBy(item => item.EstimatedCost).ToList();
        ItemsListView.ItemsSource = new ObservableCollection<Item>(sortedItems);
    }
    private void OnSortBysmallerEstimatedCostClicked(object sender, EventArgs e)
    {
        var sortedItems = _items.OrderByDescending(item => item.EstimatedCost).ToList();
        ItemsListView.ItemsSource = new ObservableCollection<Item>(sortedItems);
    }

    private async void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Item tappedItem)
        {
            await DisplayAlert("Item Details",
                $"Name: {tappedItem.Name}\n" +
                $"Description: {tappedItem.Description}\n" +
                $"Category: {tappedItem.Category}\n" +
                $"Priority: {tappedItem.Priority}\n" +
                $"Estimated Cost: {tappedItem.EstimatedCost:C}\n" +
                $"Status: {tappedItem.Status}\n" +
                $"Created At: {tappedItem.CreatedAt:MM/dd/yyyy}", "OK");
        }
    }
}
