using EmployeeDirectory.DataBase;
using EmployeeDirectory.Logic;
using EmployeeDirectory.Models;
using EmployeeDirectory.Utility;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace EmployeeDirectory
{
    public class Program
    {
        private static readonly ICalculateAgeLogic _calculate = new CalculateAgeLogic();
        public static async Task Main(string[] args)
        {
            var сonfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var serverConnection = сonfiguration.GetConnectionString("ServerConnection");
            var databaseConnection = сonfiguration.GetConnectionString("DatabaseConnection");

            IEmployeeDb db = new EmployeeDb(serverConnection!, databaseConnection!);

            if (!args.Any())
            {
                Console.WriteLine("Пожалуйста выберите режим работы приложения." +
                    "\n1. Создание таблицы с полями справочника сотрудников, представляющими \"Фамилию Имя Отчество\", \"дату рождения\", \"пол\"." +
                    "\n2. Создание записи справочника сотрудников." +
                    "\n3. Вывод всех строк справочника сотрудников, с уникальным значением ФИО+дата, отсортированным по ФИО. Вывести ФИО, Дату рождения, пол, кол-во полных лет." +
                    "\n4. Заполнение автоматически 1000000 строк справочника сотрудников. Распределение пола в них должно быть относительно равномерным, начальной буквы ФИО также. " +
                    "\n5. Результат выборки из таблицы по критерию: пол мужской, Фамилия начинается с \"F\".");
            }
            var test = Console.ReadLine();

            switch (test)
            {
                case "1":
                    await db.CreateDatabaseAndTableAsync();
                    break;

                case "2":
                    Console.WriteLine("Введите ФИО");
                    var name = Console.ReadLine();

                    DateTime date =DateTime.MinValue;
                    bool isValid = false;

                    while (!isValid)
                    {
                        Console.Write("Введите дату рождения (формат ГГГГ.ММ.ДД): ");
                        string? input = Console.ReadLine();

                        if (DateTime.TryParse(input, out date) && date < DateTime.Now && date > DateTime.MinValue)
                        {
                            isValid = true;                           
                        }
                        else
                        {
                            Console.WriteLine("Введенная дата рождения некорректна");
                        }
                    }

                    int age = _calculate.CalculateAgeEmployee(date);

                    bool isProgress = true;
                    string? gender = string.Empty;

                    while (isProgress)
                    {
                        Console.WriteLine("Введите пол Male/Female");
                        gender = Console.ReadLine();

                        if (gender == Constans.Female || gender == Constans.Male)
                        {
                            isProgress = false;
                        }
                    }

                    var employee = new Employee(name!, date, gender!, age);

                    await db.SaveEmployeeToDatabase(new[] { employee }.ToList());
                    break;

                case "3":
                    var employees = await db.GetEmployees();

                    foreach (var emp in employees)
                    {
                        Console.WriteLine($"ФИО: {emp.FullName}, Дата рождения: {emp.BirthDate.ToShortDateString()}, Пол: {emp.Gender}, Возраст: {emp.Age}");
                    }
                    break;

                case "4":
                    //можно сделать в цикле или в методе изменить цикл на 1млн записей для теста хватит
                    await db.GenerateAndSaveEmployeesAsync();

                    Console.WriteLine("100 записей сотрудников успешно добавлено в базу данных.");
                    break;

                case "5":
                    var watch = Stopwatch.StartNew();
                    await db.SelectByCriteria();
                    watch.Stop();
                    Console.WriteLine($"Время исполнения: {watch.ElapsedMilliseconds} ms");
                    break;

                default:
                    Console.WriteLine("Недействительным аргумент");
                    break;
            }
        }
    }
}
