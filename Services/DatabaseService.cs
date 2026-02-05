using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Habit_tracker.Models;

namespace Habit_tracker.Services
{
    public class DatabaseService
    {
        //this prevents database queries from blocking the UI thread
        SQLiteAsyncConnection _database;

        async Task InIt()
        {
            if (_database != null)
                return;
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            // will only initialise if the table does not exist
            await _database.CreateTableAsync<Habit>(); 

        }

        public async Task<List<Habit>> GetHabitsAsync() 
        {
            await InIt();
            return await _database.Table<Habit>().ToListAsync();

        
        }

        public async Task <int> SaveHabitsAsync(Habit habit)
        {
            await InIt();
            return await _database.InsertAsync(habit);
        }

        public async Task <int> UpdateAsync(Habit habit)
        {
            await InIt();
            return await _database.UpdateAsync(habit);
        }
    }

}
