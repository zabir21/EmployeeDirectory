using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDirectory
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(1)]
        public string Gender { get; set; }

        [Required]
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
