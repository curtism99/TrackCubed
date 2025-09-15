using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Shared.Models;

namespace TrackCubed.Shared.DTOs
{
    /// <summary>
    /// A safe, flattened DTO for sending CubedItem data to the client.
    /// </summary>
    public class ItemTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public ItemType ItemType { get; set; }
        public string ItemTypeName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? Notes { get; set; }
        // We can include the user's ID if needed
        public int CreatedById { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
