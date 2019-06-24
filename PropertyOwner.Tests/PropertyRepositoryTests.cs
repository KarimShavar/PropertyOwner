using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PropertyOwner.App.Data;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.Tests
{
    public class PropertyRepositoryTests
    {
        private IPropertyRepository Sut { get; set; }
        private SqliteConnection _connection { get; set; }

        [SetUp]
        public void Setup()
        {
            InitialiseMapping();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<PropertyContext>()
                .UseSqlite(_connection)
                .Options;

            var inMemoryContext = new PropertyContext(options);
            inMemoryContext.Properties.AddRange(GenerateFakeProperties());
            inMemoryContext.SaveChanges();

            Sut = new PropertyRepository(inMemoryContext);
        }

        [TearDown]
        public void TearDown()
        {
            Mapper.Reset();
            _connection.Close();
        }

        [Test]
        public void GetPropertiesReturnsAllPropertiesInDbOrderedByMarketValue()
        {
            // Arrange
            // Act
            var result = Sut.GetProperties();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Is.Ordered.Descending.By("MarketValue"));
        }

        [Test]
        public void GetPropertyReturnsSinglePropertyByGuid()
        {
            // Arrange
            // Act
            var result = Sut.GetProperty(new Guid("c8c44473-7c9c-4dd6-9462-d173a8e99da9"));

            // Assert
            Assert.That(result, Is.TypeOf<Property>());
            Assert.That(result.MarketValue, Is.EqualTo(100000));
        }

        [Test]
        public void AddPropertyAddsPropertyToDbCorrectlyAssigningNewGuids()
        {
            // Arrange
            var propertyToCreate = new PropertyForCreationDto()
            {
                Description = "Cosy semi-detached house in central Mansfield.",
                MarketValue = 95000,
                Rent = 450,
                Costs = 100,
                HouseNumber = 12,
                Street = "Newcastle Street",
                PostCode = "S70 2FF",
                Country = "United Kingdom",
                Tenants = new List<Tenant>()
                {
                    new Tenant()
                    {
                        FirstName = "Some",
                        LastName = "Dude"
                    }
                }
            };

            // Act
            var propertyToAdd = Mapper.Map<Property>(propertyToCreate);
            Sut.AddProperty(propertyToAdd);
            Sut.Save();

            var actual = Sut.GetProperties().FirstOrDefault(p => p.PostCode == "S70 2FF");

            // Assert 
            Assert.That(actual.Id, Is.Not.Null);
            Assert.That(actual.Tenants.FirstOrDefault().Id, Is.Not.Null);
            Assert.That(actual.Rent, Is.EqualTo(propertyToAdd.Rent));
        }

        [Test]
        public void DeletePropertySuccesfullyRemovesPropertyFromDB()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var propertyToDelete = Sut.GetProperty(propertyId);

            // Act
            Sut.DeleteProperty(propertyToDelete);
            Sut.Save();

            // Assert
            Assert.That(Sut.GetProperty(propertyId), Is.Null);
        }

        [Test]
        public void PropertyExistsReturnsTrueIfPropertyIsInDb()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");

            // Act
            var result = Sut.PropertyExist(propertyId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateProperty_ChangesValuesOfPropertyAndUpdatesDb()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var propertyToUpdate = Sut.GetProperty(propertyId);

            // Act
            propertyToUpdate.HouseNumber = 7;
            propertyToUpdate.MarketValue = 97000;
            Sut.UpdateProperty(propertyToUpdate);
            Sut.Save();

            var actual = Sut.GetProperty(propertyId);

            // Assert
            Assert.That(actual.HouseNumber, Is.EqualTo(7));
            Assert.That(actual.MarketValue, Is.EqualTo(97000));
        }

        [Test]
        public void PropertyExistsReturnsFalseWhenNoPropertyInDb()
        {
            // Arrange
            var propertyId = new Guid();

            // Act
            var result = Sut.PropertyExist(propertyId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetTenantsForPropertyReturnsTenant()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var tenantId = new Guid("a806007b-b643-4c9e-8632-06f5ebf7dd1d");

            // Act
            var tenantsForProperty = Sut.GetTenantsForProperty(propertyId);

            // Assert
            Assert.That(tenantsForProperty.Count(), Is.EqualTo(2));
            Assert.That(tenantsForProperty.FirstOrDefault(t => t.Id == tenantId), Is.Not.Null);
        }

        [Test]
        public void UpdateTenantCorrectlyUpdatesASingleTenant()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var tenantId = new Guid("a806007b-b643-4c9e-8632-06f5ebf7dd1d");

            // Act
            var tenantsForProperty = Sut.GetTenantsForProperty(propertyId);
            var tenantToUpdate = tenantsForProperty.FirstOrDefault(t => t.Id == tenantId);

            tenantToUpdate.FirstName = "Updated";
            Sut.UpdateTenant(tenantToUpdate);
            Sut.Save();

            var actual = Sut.GetTenantsForProperty(propertyId)
                .FirstOrDefault(t => t.Id == tenantId);

            // Assert
            Assert.That(actual.FirstName, Is.EqualTo("Updated"));
        }

        [Test]
        public void DeleteTenantRemovesTenantFromDb()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var tenantId = new Guid("a806007b-b643-4c9e-8632-06f5ebf7dd1d");

            // Act
            var tenantsForProperty = Sut.GetTenantsForProperty(propertyId);
            var tenantToDelete = tenantsForProperty.FirstOrDefault(t => t.Id == tenantId);

            Sut.DeleteTenant(tenantToDelete);
            Sut.Save();

            var actual = Sut.GetTenantsForProperty(propertyId);

            // Assert
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual.FirstOrDefault(t => t.Id == tenantId), Is.Null);
        }

        [Test]
        public void SaveReturnsTrueIfChangesAreMade()
        {
            // Arrange
            var propertyToCreate = new PropertyForCreationDto()
            {
                Description = "Cosy semi-detached house in central Mansfield.",
                MarketValue = 95000,
                Rent = 450,
                Costs = 100,
                HouseNumber = 12,
                Street = "Newcastle Street",
                PostCode = "S70 2FF",
                Country = "United Kingdom",
                Tenants = new List<Tenant>()
                {
                    new Tenant()
                    {
                        FirstName = "Some",
                        LastName = "Dude"
                    }
                }
            };

            // Act
            var propertyToAdd = Mapper.Map<Property>(propertyToCreate);
            Sut.AddProperty(propertyToAdd);

            // Act
            var actual = Sut.Save();
            // Assert
            Assert.That(actual, Is.True);
        }

        private IEnumerable<Property> GenerateFakeProperties()
        {
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
            return properties;
        }

        private void InitialiseMapping()
        {
            Mapper.Initialize(config =>
            {
                config.CreateMap<Tenant, TenantDto>()
                    .ForMember(dest => dest.Name, opt => opt
                        .MapFrom(src => $"{src.FirstName} {src.LastName}"));

                config.CreateMap<Property, PropertyDto>();
                config.CreateMap<PropertyForCreationDto, Property>();
                config.CreateMap<TenantForCreationDto, Tenant>();
            });
        }
    }
}