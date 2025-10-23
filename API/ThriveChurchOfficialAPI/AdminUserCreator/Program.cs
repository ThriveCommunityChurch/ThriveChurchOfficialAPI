using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.IO;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Core.Utilities;

namespace AdminUserCreator
{
    /// <summary>
    /// Console application to create admin users for the Thrive Church API
    /// </summary>
    class Program
    {
        private static IConfiguration _configuration;
        private static IMongoDatabase _database;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Thrive Church API - Admin User Creator ===");
            Console.WriteLine();

            try
            {
                // Load configuration
                LoadConfiguration();

                // Connect to MongoDB
                ConnectToDatabase();

                // Get admin user details
                var adminUser = GetAdminUserDetails();

                // Create the admin user
                await CreateAdminUser(adminUser);

                Console.WriteLine();
                Console.WriteLine("Admin user created successfully!");
                Console.WriteLine($"Username: {adminUser.Username}");
                Console.WriteLine($"Email: {adminUser.Email}");
                Console.WriteLine();
                Console.WriteLine("You can now use these credentials to log in to the API.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Load configuration from appsettings.json
        /// </summary>
        private static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var connectionString = _configuration["MongoConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("MongoConnectionString not found in appsettings.json");
            }
        }

        /// <summary>
        /// Connect to MongoDB database
        /// </summary>
        private static void ConnectToDatabase()
        {
            var connectionString = _configuration["MongoConnectionString"];
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("SermonSeries");

            Console.WriteLine($"Connected to MongoDB: {connectionString}");
        }

        /// <summary>
        /// Get admin user details from user input
        /// </summary>
        /// <returns>User object with admin details</returns>
        private static User GetAdminUserDetails()
        {
            Console.WriteLine("Enter admin user details:");
            Console.WriteLine();

            // Get username
            Console.Write("Username: ");
            var username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is required");
            }

            // Get email
            Console.Write("Email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("Email is required");
            }

            // Get password
            Console.Write($"Password ({PasswordValidator.GetPasswordRequirements()}): ");
            var password = GetPasswordInput();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Password is required");
            }

            // Validate password complexity
            var passwordValidationError = PasswordValidator.ValidatePasswordWithMessage(password);
            if (passwordValidationError != null)
            {
                throw new Exception(passwordValidationError);
            }

            // Confirm password
            Console.Write($"Confirm Password ({PasswordValidator.GetPasswordRequirements()}): ");
            var confirmPassword = GetPasswordInput();
            if (password != confirmPassword)
            {
                throw new Exception("Passwords do not match");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

            return new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Roles = new[] { "Admin" }
            };
        }

        /// <summary>
        /// Get password input with hidden characters
        /// </summary>
        /// <returns>Password string</returns>
        private static string GetPasswordInput()
        {
            var password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        /// <summary>
        /// Create the admin user in MongoDB
        /// </summary>
        /// <param name="adminUser">Admin user to create</param>
        private static async Task CreateAdminUser(User adminUser)
        {
            var usersCollection = _database.GetCollection<User>("Users");

            // Check if user already exists
            var existingUser = await usersCollection.Find(u => u.Username == adminUser.Username || u.Email == adminUser.Email)
                                                   .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                throw new Exception($"User with username '{adminUser.Username}' or email '{adminUser.Email}' already exists");
            }

            // Create indexes
            try
            {
                var usernameIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
                var usernameIndexOptions = new CreateIndexOptions { Unique = true };
                var usernameIndexModel = new CreateIndexModel<User>(usernameIndexKeys, usernameIndexOptions);

                var emailIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
                var emailIndexOptions = new CreateIndexOptions { Unique = true };
                var emailIndexModel = new CreateIndexModel<User>(emailIndexKeys, emailIndexOptions);

                await usersCollection.Indexes.CreateManyAsync(new[] { usernameIndexModel, emailIndexModel });
            }
            catch
            {
                // Indexes might already exist
            }

            // Insert the admin user
            await usersCollection.InsertOneAsync(adminUser);
        }
    }
}
