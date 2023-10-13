using Dapper;
using EmployeeDirectory.Logic;
using EmployeeDirectory.Models;
using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDirectory.DataBase
{
    public class EmployeeDb : IEmployeeDb
    {
        private readonly string _serverConnectionString;
        private readonly string _dbConnectionString;
        private readonly ICalculateAgeLogic _calculateAgeLogic;

        public EmployeeDb(string serverConnection, string dbConnection)
        {
            _serverConnectionString = serverConnection;
            _dbConnectionString = dbConnection;
            _calculateAgeLogic = new CalculateAgeLogic();
        }

        public async Task CreateDatabaseAndTableAsync()
        {
            using var connection = new NpgsqlConnection(_serverConnectionString);

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
            }
            finally 
            {
               await connection.CloseAsync();
            }

            using var connection2 = GetNpgsqlConnection();
            connection2.Open();

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
                await connection2.ExecuteAsync(createTableCommand);
                Console.WriteLine("Таблица 'Employees' успешно создана.");
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при создании таблицы: {ex.Message}");
            }
            finally
            { 
               await connection2.CloseAsync(); 
            }
        }

        public async Task SaveEmployeeToDatabase(List<Employee> employees, IDbConnection? connection = null)
        {
            connection ??= GetNpgsqlConnection();

            var insertQuery = new StringBuilder(@"
                INSERT INTO Employees (FullName, BirthDate, Gender, Age) 
                VALUES
                ");

            foreach (var employee in employees)
            {
                insertQuery.Append($" ('{employee.FullName}', '{employee.BirthDate.ToString("yyyy-MM-dd HH:mm:ss")}', '{employee.Gender}' , '{employee.Age}'),");
            }
            insertQuery.Remove(insertQuery.Length-1, 1);
            insertQuery.Append(';');

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var insertedCount = await connection.ExecuteAsync(insertQuery.ToString());

            Console.WriteLine($"{insertedCount} cотрудников успешно добавлен в базу данных.");
        }

        public async Task<List<Employee>> GetEmployees()
        {
            using (IDbConnection connection = GetNpgsqlConnection())
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
            using (var connection = GetNpgsqlConnection())
            {
                await connection.OpenAsync();

                var sw = new Stopwatch();
                Console.WriteLine("Начало генерации миллиона записей");
                sw.Start();

                var employeesMillion = GenerateEmployees(1000_000, true);
                Console.WriteLine($"Завершили генерацию за {sw.Elapsed}");
                var employeesHundred = GenerateEmployees(100, false);

                sw.Restart();

                var batch = employeesMillion.Chunk(10000);
                foreach (var employee in batch)
                {
                    await SaveEmployeeToDatabase(employee.ToList(), connection);
                }

                await SaveEmployeeToDatabase(employeesHundred, connection);

                Console.WriteLine($"Завершили запись за {sw.Elapsed}");
            }
        }

        private List<Employee> GenerateEmployees(int count, bool fullRandom)
        {
            var random = new Random();
            var genders = new[] { "Male", "Female" };
            var names = Enumerable.Range(0, 26).Select(i => ((char)('A' + i)).ToString()).ToArray();
            var employees = new List<Employee>();

            for (int i = 0; i < count; i++)
            {
                var date = new DateTime(random.Next(1950, 2003), random.Next(1, 13), random.Next(1, 29));
                var age = _calculateAgeLogic.CalculateAgeEmployee(date);

                var employee = new Employee(fullName: fullRandom ? names[random.Next(names.Length)] : "F Ivanov",
                    birthDate: date,
                    gender: fullRandom ? genders[random.Next(genders.Length)] : "Male",
                    age: age);

                employees.Add(employee);
            }

            return employees;
        }

        public async Task<List<Employee>> SelectByCriteria()
        {
            using var connection = GetNpgsqlConnection();

            string selectSql = "SELECT * FROM Employees WHERE Gender = @Gender AND FullName LIKE @FullName";
            var result = await connection.QueryAsync<Employee>(selectSql, new { Gender = "Male", FullName = "F%" });
            return result.ToList();
        }

        private NpgsqlConnection GetNpgsqlConnection()
        {
            return new NpgsqlConnection(_dbConnectionString);
        }
    }
}