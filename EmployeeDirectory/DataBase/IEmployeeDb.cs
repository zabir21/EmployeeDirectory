using EmployeeDirectory.Models;
using System.Data;

namespace EmployeeDirectory.DataBase
{
    public interface IEmployeeDb
    {
        Task CreateDatabaseAndTableAsync();
        Task SaveEmployeeToDatabase(List<Employee> employee, IDbConnection? connection=null);
        Task<List<Employee>> GetEmployees();
        Task GenerateAndSaveEmployeesAsync();
        Task<List<Employee>> SelectByCriteria();
    }
}
