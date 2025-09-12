using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    // In your Data or Models folder
    public class ApplicationUsers
    {
        public int Id { get; set; } // Your primary key
        public string EntraObjectId { get; set; } // Stores the unique 'oid' from the token
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
