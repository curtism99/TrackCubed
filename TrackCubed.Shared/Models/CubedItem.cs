using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    public class CubedItem
    {
        /// <summary>
        /// The unique identifier for the Cubed Item. Using a Guid is excellent for a distributed system.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The user-friendly name for the item. Required.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// A direct URL for web links. Nullable for items that are not links.
        /// </summary>
        [MaxLength(2048)]
        public string? Link { get; set; }

        /// <summary>
        /// A URL pointing to a stored file (e.g., in Azure Blob Storage).
        /// We store a URL, not the file itself, in the database for performance and scalability.
        /// </summary>
        [MaxLength(2048)]
        public string? SourceFileUrl { get; set; }

        /// <summary>
        /// A short, optional description of the item.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Longer, personal notes about the item. Can be extensive.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// The type of item, used for filtering and displaying the correct UI.
        /// </summary>
        public CubedItemType ItemType { get; set; }

        /// <summary>
        /// The date and time when the item was first saved.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// The last time the item was accessed or updated. Useful for sorting by relevance.
        /// </summary>
        public DateTime DateLastAccessed { get; set; }

        // --- Navigation Properties for Relationships ---

        /// <summary>
        /// The collection of tags associated with this item.
        /// This creates a many-to-many relationship with the Tag model.
        /// </summary>
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// The foreign key pointing to the user who owns this item.
        /// </summary>
        [Required]
        public int CreatedById { get; set; }

        /// <summary>
        /// The user who created and owns this item.
        /// This creates a one-to-many relationship with the ApplicationUser model.
        /// </summary>
        [ForeignKey(nameof(CreatedById))]
        public ApplicationUser CreatedBy { get; set; }
    }

    /// <summary>
    /// An enum to categorize the type of CubedItem.
    /// </summary>
    public enum CubedItemType
    {
        Link,
        Image,
        Document,
        Song,
        Note,
        Other
    }
}
