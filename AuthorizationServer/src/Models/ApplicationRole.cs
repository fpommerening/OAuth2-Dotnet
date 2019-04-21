using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Models;

namespace FP.OAuth.AuthorizationServer.Models
{
    public class ApplicationRole : MongoIdentityRole<Guid>
    {
    }
}
