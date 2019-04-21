using System;
using System.Collections.Generic;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;

namespace FP.OAuth.AuthorizationServer.Models
{
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        public List<ObjectId> Applications { get; set; }
    }
}
