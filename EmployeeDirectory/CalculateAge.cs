namespace EmployeeDirectory
{
    public static class CalculateAge
    {
        public static int CalculateAgeEmployee(DateTime birthDate)
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
