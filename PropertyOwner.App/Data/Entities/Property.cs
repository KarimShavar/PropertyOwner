using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PropertyOwner.App.Data.Entities
{
    public class Property
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public decimal MarketValue { get; set; }
        public decimal Rent { get; set; }
        public decimal Costs { get; set; }

        [Required]
        [Range(1, 500)]
        public int HouseNumber { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Street { get; set; }

        [Required]
        public string PostCode { get; set; }

        [Required]
        public string Country { get; set; }

        public ICollection<Tenant> Tenants { get; set; }
            = new List<Tenant>();
    }
}