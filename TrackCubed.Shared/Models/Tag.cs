using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// The collection of CubedItems associated with this tag.
        /// This is the other side of the many-to-many relationship.
        /// </summary>
        public ICollection<CubedItem> CubedItems { get; set; } = new List<CubedItem>();
    }
}
