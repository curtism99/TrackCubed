using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Shared.Models
{
    public class UserItemType
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [MaxLength(50)] 
        public string Name { get; set; }

        [Required] 
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))] 
        public ApplicationUser User { get; set; }
    }
}
