using Dapper;
using Npgsql;
using System.Data;

namespace EmployeeDirectory.DataBase
{
    public class EmployeeDb : IEmployeeDb
    {
        private static readonly string connectionString = "Server=localhost; Port=5432; Database = employeedirectory;User id=postgres;password=1111;";
        private static readonly string createConString = "Server=localhost; Port=5432; User id=postgres;password=1111;";

        public async Task CreateDatabaseAndTableAsync()
        {
            using var connection = new NpgsqlConnection(createConString);

            // Создание базы данных
            var createDatabaseCommand = "CREATE DATABASE EmployeeDirectory;";
            connection.Open();
            try
            {
                await connection.ExecuteAsync(createDatabaseCommand);
                Console.WriteLine("База данных 'EmployeeDirectory' успешно создана.");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при создании базы данных: {ex.Message}");
            }finally { connection.Close(); }

            
            // Создание таблицы
            var createTableCommand = @"
                    CREATE TABLE Employees (
                        FullName VARCHAR(150),
                        BirthDate DATE,
                        Gender VARCHAR(10),
                        Age INT
                    );";
            try
            {
                using var connection2 = new NpgsqlConnection(connectionString);
                connection2.Open();

                //connection2.ChangeDatabase("emloyeedirectory");
                await connection2.ExecuteAsync(createTableCommand);
                Console.WriteLine("Таблица 'Employees' успешно создана.");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при создании таблицы: {ex.Message}");
            }
            finally { await connection.CloseAsync(); }
        }

        public async Task SaveEmployeeToDatabase(Employee employee)
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                string insertQuery = @"
                INSERT INTO Employees (FullName, BirthDate, Gender, Age) 
                VALUES (@FullName, @BirthDate, @Gender, @Age);";

                await connection.ExecuteAsync(insertQuery, employee);
            }
            Console.WriteLine($"Сотрудник '{employee.FullName}' успешно добавлен в базу данных.");
        }

        public async Task<List<Employee>> GetEmployees()
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                string selectQuery = @"
                SELECT FullName, BirthDate, Gender, Age
                FROM Employees 
                GROUP BY FullName, BirthDate,Gender, Age 
                ORDER BY FullName;";

                var employees = (await connection.QueryAsync<Employee>(selectQuery)).AsList();
                return employees;
            }
        }

        public async Task GenerateAndSaveEmployeesAsync()
        {
            var random = new Random();
            var genders = new[] { "Male", "Female" };
            var names = Enumerable.Range(0, 26).Select(i => ((char)('A' + i)).ToString()).ToArray();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Генерация и сохранение 1 000 000 сотрудников
                for (int i = 0; i < 50; i++)
                {
                    var date = new DateTime(random.Next(1950, 2003), random.Next(1, 13), random.Next(1, 29));
                    var age = CalculateAge.CalculateAgeEmployee(date);

                    var employee = new Employee(
                        fullName: names[random.Next(names.Length)] + " Ivanov",
                        birthDate: date,
                        gender: genders[random.Next(genders.Length)],
                        age: age);

                    await SaveEmployeeToDatabase(employee);
                }

                // Генерация и сохранение 100 сотрудников с полом "Мужской" и фамилией на "F"
                for (int i = 0; i < 30; i++)
                {
                    var date = new DateTime(random.Next(1950, 2003), random.Next(1, 13), random.Next(1, 29));
                    var age = CalculateAge.CalculateAgeEmployee(date);

                    var employee = new Employee(
                        fullName: "F Ivanov",
                        birthDate: date,
                        gender: "Male",
                        age: age);

                    await SaveEmployeeToDatabase(employee);
                }
            }
        }

        public async Task<List<Employee>> SelectByCriteria()
        {
            using var connection = new NpgsqlConnection(connectionString);

            string selectSql = "SELECT * FROM Employees WHERE Gender = @Gender AND FullName LIKE @FullName";
            var result = await connection.QueryAsync<Employee>(selectSql, new { Gender = "Male", FullName = "F%" });
            return result.ToList();
        }
    }
}