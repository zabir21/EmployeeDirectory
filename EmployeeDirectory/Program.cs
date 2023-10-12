using EmployeeDirectory.DataBase;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;

namespace EmployeeDirectory
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IEmployeeDb db = new EmployeeDb();

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
                    Console.WriteLine("Введите дату рождения yy.mm.dd");
                    var date = DateTime.Parse(Console.ReadLine());
                    var age = CalculateAge.CalculateAgeEmployee(date);
                    Console.WriteLine("Введите пол");
                    var gender = Console.ReadLine();

                    var employee = new Employee(name, date, gender, age);
                    await db.SaveEmployeeToDatabase(employee);
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
                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
                    break;
                default:
                    Console.WriteLine("Invalid argument");
                    break;
            }
        }
    }
}
