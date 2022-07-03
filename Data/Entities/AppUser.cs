using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public int Age { get; set; }
    }
}
