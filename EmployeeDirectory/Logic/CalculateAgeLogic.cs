namespace EmployeeDirectory.Logic
{
    public class CalculateAgeLogic : ICalculateAgeLogic
    {
        public int CalculateAgeEmployee(DateTime birthDate)
        {
            int age = DateTime.Today.Year - birthDate.Year;

            if (DateTime.Today < birthDate.AddYears(age))
            {
                age--;
            }

            return age;
        }
    }
}
