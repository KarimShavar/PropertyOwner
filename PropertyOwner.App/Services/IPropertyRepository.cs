using System;
using System.Collections.Generic;
using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Services
{
    public interface IPropertyRepository
    {
        IEnumerable<Property> GetProperties();
        Property GetProperty(Guid propertyId);

        void AddProperty(Property property);
        void DeleteProperty(Property property);
        void UpdateProperty(Property property);
        bool PropertyExist(Guid propertyId);

        IEnumerable<Tenant> GetTenantsForProperty(Guid propertyId);
        void AddTenantToProperty(Guid propertyId, Tenant tenant);
        void UpdateTenant(Tenant tenant);
        void DeleteTenant(Tenant tenant);
        bool Save();
    }
}