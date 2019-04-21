using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FP.OAuth.AuthorizationServer.Configuration
{
    public class Auth
    {
        public string ConnectionString { get; set; }

        public string IdentityDatabase { get; set; }

        public string TokenDatabase { get; set; }
    }
}
