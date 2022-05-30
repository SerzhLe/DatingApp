using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (today.Month < dateOfBirth.Month ||
                today.Month == dateOfBirth.Month && today.Day < dateOfBirth.Day) age--;

            return age;
        }

    }
}