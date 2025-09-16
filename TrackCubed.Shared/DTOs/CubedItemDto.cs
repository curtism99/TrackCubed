using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.DTOs
{
    public class CubedItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }

        // THE CHANGE: The client needs the name for display purposes.
        public string ItemTypeName { get; set; }

        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string? PreviewTitle { get; set; }
        public string? PreviewDescription { get; set; }
        public string? PreviewImageUrl { get; set; }
    }
}
