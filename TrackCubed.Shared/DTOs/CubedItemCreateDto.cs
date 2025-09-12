using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Shared.Models;

namespace TrackCubed.Shared.DTOs
{
    /// <summary>
    /// A Data Transfer Object for creating a new CubedItem.
    /// It only contains the properties the client is allowed to set.
    /// </summary>
    public class CubedItemCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(2048)]
        public string? Link { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Notes { get; set; }

        public CubedItemType ItemType { get; set; }
    }
}
