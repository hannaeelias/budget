using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace budget.models
{
    public class AppDbContext
    {
        private const string DB_name = "budget_local_db.db3"; 
        private readonly SQLiteAsyncConnection _connection;

        public AppDbContext()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_name);
            _connection = new SQLiteAsyncConnection(dbPath);

            _connection.CreateTableAsync<Item>().Wait(); 
        }

        public async Task<List<Item>> GetItems()
        {
            return await _connection.Table<Item>().ToListAsync();
        }

        public async Task<Item> GetById(int id)
        {
            return await _connection.Table<Item>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task Create(Item item)
        {
            await _connection.InsertAsync(item);
        }
        public async Task Update(Item item)
        {
            await _connection.UpdateAsync(item);
        }

        public async Task Delete(Item item)
        {
            try
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                var existingItem = await _connection.Table<Item>().FirstOrDefaultAsync(i => i.Id == item.Id);

                if (existingItem != null)
                {
                    await _connection.DeleteAsync(existingItem);
                    Console.WriteLine($"Item with ID {item.Id} deleted.");
                }
                else
                {
                    Console.WriteLine($"Item with ID {item.Id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
            }
        }

    }
}
