using System;
using System.Collections.Generic;
using System.Linq;
using PropertyOwner.App.Data;
using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Services
{
    public class PropertyRepository : IPropertyRepository
    {
        private PropertyContext _context;

        public PropertyRepository(PropertyContext context)
        {
            _context = context;
        }

        public IEnumerable<Property> GetProperties()
        {
            return _context.Properties
                .OrderBy(a => a.MarketValue)
                .ToList();
        }

        public Property GetProperty(Guid propertyId)
        {
            return _context.Properties.FirstOrDefault(p => p.Id == propertyId);
        }

        public void AddProperty(Property property)
        {
            property.Id = new Guid();

            if (property.Tenants.Any())
            {
                foreach (var tenant in property.Tenants)
                {
                    tenant.Id = new Guid();
                }
            }

            _context.Properties.Add(property);
        }

        public void DeleteProperty(Property property)
        {
            _context.Properties.Remove(property);
        }

        public void UpdateProperty(Property property)
        {
            // No implementation needed.
            _context.Properties.Update(property);
        }

        public bool PropertyExist(Guid propertyId)
        {
            return _context.Properties.Any(p => p.Id == propertyId);
        }

        public IEnumerable<Tenant> GetTenantsForProperty(Guid propertyId)
        {
            return _context.Tenants.Where(p => p.Property.Id == propertyId)
                .OrderBy(t => t.FirstName)
                .ToList();
        }

        public void AddTenantToProperty(Guid propertyId, Tenant tenant)
        {
            var property = GetProperty(propertyId);
            if (property != null)
            {
                if (tenant.Id == Guid.Empty)
                {
                    tenant.Id = new Guid();
                }

                property.Tenants.Add(tenant);
            }
        }

        public void UpdateTenant(Tenant tenant)
        {
            _context.Tenants.Update(tenant);
        }

        public void DeleteTenant(Tenant tenant)
        {
            _context.Tenants.Remove(tenant);
        }

        public bool Save()
        {
            // Return value is number of rows affected.
            return _context.SaveChanges() >= 0;
        }
    }
}