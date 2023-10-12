namespace EmployeeDirectory.DataBase
{
    public interface IEmployeeDb
    {
        Task CreateDatabaseAndTableAsync();
        Task SaveEmployeeToDatabase(Employee employee);
        Task<List<Employee>> GetEmployees();
        Task GenerateAndSaveEmployeesAsync();
        Task<List<Employee>> SelectByCriteria();
    }
}
