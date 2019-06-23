using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyOwner.App.Data.Entities
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        public Guid PropertyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
    }
}