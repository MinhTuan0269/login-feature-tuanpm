using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Helper
{
    public static class PasswordValidator
    {
        public static bool IsValid(
            string password)
        {
            return Regex.IsMatch(
                password,
                @"^(?=.*[A-Z])(?=.*[\W_]).{12,}$");
        }
    }
}
