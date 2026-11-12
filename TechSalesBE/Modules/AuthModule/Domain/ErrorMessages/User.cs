using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthModule.Domain.ErrorMessages
{
    internal static partial class DomainErrors
    {
        internal static class User
        {
            internal const string EmailInvalid = "Email format is invalid.";
            internal const string PasswordInvalid = "Password is invalid";
            internal const string UsernameInvalid = "Username is invalid";
        }
    }
}