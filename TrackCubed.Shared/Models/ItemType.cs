using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    public class ItemType
    {
        [Key] 
        public int Id { get; set; }

        [Required][MaxLength(50)] 
        public string Name { get; set; }
        public int? UserId { get; set; } // Null for system types, set for user types

        [ForeignKey(nameof(UserId))] 
        public ApplicationUser? User { get; set; }
    }
}
