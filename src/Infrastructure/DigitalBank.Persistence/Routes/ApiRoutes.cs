using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Persistence.Routes
{
    public static class ApiRoutes
    {
        public static class Client
        {
            private const string Base = "api/client";

            public static class Auth
            {
                public const string BaseRoute = $"{Base}/auth";
                public const string Register = "register";
                public const string Login = "login";
                public const string Refresh = "refresh";
                public const string Logout = "logout";
                public const string ConfirmEmail = "confirm-email";
            }
        }
        public static class Admin
        {
            private const string Base = "api/admin";

            public static class Users
            {
                public const string BaseRoute = $"{Base}/users";
            }
        }
    }
}
