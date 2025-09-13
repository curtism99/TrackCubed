using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    // In your Data or Models folder
    public class ApplicationUser
    {
        public int Id { get; set; } // Your primary key
        public string EntraObjectId { get; set; } // Stores the unique 'oid' from the token
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// The collection of CubedItems created by this user.
        /// This is the "one" side of the one-to-many relationship.
        /// </summary>
        public ICollection<CubedItem> CubedItems { get; set; } = new List<CubedItem>();

        // The collection of tags created by this user.
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public ICollection<UserItemType> CustomItemTypes { get; set; } = new List<UserItemType>();
    }
}
