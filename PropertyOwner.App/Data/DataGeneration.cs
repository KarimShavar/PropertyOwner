using System;
using System.Collections.Generic;
using PropertyOwner.App.Data.Entities;

namespace PropertyOwner.App.Data
{
    public static class DataGeneration
    {
        public static void SeedDataForDevelopment(this PropertyContext context)
        {
            context.Properties.RemoveRange(context.Properties);
            context.SaveChanges();

            var properties = new List<Property>
            {
                new Property()
                {
                    Id = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513"),
                    Description = "Lovely flat based in central Worksop.",
                    MarketValue = 95000,
                    Rent = 390,
                    Costs = 120,
                    HouseNumber = 22,
                    Street = "Potter Street",
                    PostCode = "S802AF",
                    Country = "United Kingdom",
                    Tenants = new List<Tenant>()
                    {
                        new Tenant()
                        {
                            Id = new Guid("a806007b-b643-4c9e-8632-06f5ebf7dd1d"),
                            FirstName = "Adam",
                            LastName = "Ciszewski"
                        },
                        new Tenant()
                        {
                            Id = new Guid("6abbea18-57e4-446b-91a5-6cf26e5ca04b"),
                            FirstName = "Justyna",
                            LastName = "Ciszewska"
                        }
                    }
                },

                new Property()
                {
                    Id = new Guid("c8c44473-7c9c-4dd6-9462-d173a8e99da9"),
                    Description = "Not so lovely flat close to Manton.",
                    MarketValue = 100000,
                    Rent = 450,
                    Costs = 160,
                    HouseNumber = 4,
                    Street = "Maple Leaf Gardens",
                    PostCode = "S802PR",
                    Country = "United Kingdom",
                    Tenants = new List<Tenant>()
                    {
                        new Tenant()
                        {
                            Id = new Guid("bfd7f246-e873-4391-a6e0-af2aa75edc7c"),
                            FirstName = "Marta",
                            LastName = "Lagiewska"
                        }
                    }
                }
            };
            context.Properties.AddRange(properties);
            context.SaveChanges();
        }
    }
}