using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Diagnostics;

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
            _connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
            _connection.CreateTableAsync<User>();
            _connection.CreateTableAsync<Item>();
            
        }

        public async Task<List<Item>> GetItemsForUser(int id)
        {
            var user = await GetUser();
            if (user != null)
            {
                return await _connection.Table<Item>().Where(i => i.UserId == user.Id).ToListAsync();
            }
            return new List<Item>();  
        }

        public async Task<Item> GetById(int id)
        {
            return await _connection.Table<Item>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> Create(Item item)
        {
            var existingItem = await _connection.Table<Item>().Where(x => x.Name == item.Name).FirstOrDefaultAsync();
            if (existingItem != null)
            {
                Debug.WriteLine($"Name already taken: {item.Name}");
                return 0; 
            }

            int result = await _connection.InsertAsync(item);
            return result;
        }

        public async Task LogAllItems()
        {
            var items = await _connection.Table<Item>().ToListAsync();
            foreach (var item in items)
            {
                Debug.WriteLine($" ID: {item.Id}, Name: {item.Name}");
            }
        }

        public async Task Update(Item item)
        {
            await _connection.UpdateAsync(item);
        }

        public async Task Delete(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            try
            {
                var existingItem = await _connection.FindAsync<Item>(item.Id);

                if (existingItem != null)
                {
                    await _connection.DeleteAsync(existingItem);
                    Debug.WriteLine($"Item with ID {item.Id} deleted.");
                }
                else
                {
                    Debug.WriteLine($"Item with ID {item.Id} not found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting item: {ex.Message}");
            }
        }

        public async Task<User> GetUser()
        {
            return await _connection.Table<User>().FirstOrDefaultAsync();
        }

        public async Task<int> CreateUser(User user)
        {
            try
            {
                return await _connection.InsertAsync(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting user: {ex.Message}");
                return 0;
            }
        }


        public async Task<int> UpdateUser(User user)
        {
            return await _connection.UpdateAsync(user);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _connection.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        internal async Task<IEnumerable<object>> GetItemsForUser()
        {
            throw new NotImplementedException();
        }
    }
}