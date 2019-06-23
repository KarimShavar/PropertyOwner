using System;
using System.Collections.Generic;
using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Data.Models
{
    public class PropertyDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Rent { get; set; }
        public decimal Costs { get; set; }
        public int HouseNumber { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }

        public ICollection<Tenant> Tenants { get; set; }
            = new List<Tenant>();
    }
}