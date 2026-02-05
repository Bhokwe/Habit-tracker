using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habit_tracker
{
    public static class Constants
    {

        // the database name
        public const string DatabaseFileName = "HabitTracker.db3";

        //setting up the database to read or write mode
        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode, create it if it doesn't exist, and enable shared cache mode
            //these are used to specify access and how we should interact with the database
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        //the location/path in which the database will be stored on a device. so essentially the app's directory for storing data. 
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
    }
}
