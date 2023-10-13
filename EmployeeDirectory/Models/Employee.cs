using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class Employee
    {
        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        public int Age { get; set; }

        public Employee(string fullName, DateTime birthDate, string gender, int age)
        {
            FullName = fullName;
            BirthDate = birthDate;
            Gender = gender;
            Age = age;
        }
    }
}
