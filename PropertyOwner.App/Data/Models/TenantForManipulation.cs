using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Data.Models
{
    public abstract class TenantForManipulation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Property Property { get; set; }
    }
}