using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Sofista.Samples.IdentityServer.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        [MaxLength(75)]
        public string Name { get; set; }
    }
}
