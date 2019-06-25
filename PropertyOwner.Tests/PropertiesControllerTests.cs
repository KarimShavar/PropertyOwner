using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PropertyOwner.App.Controllers;
using PropertyOwner.App.Data;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.Tests
{
    public class PropertiesControllerTests
    {
        private SqliteConnection _connection { get; set; }
        private PropertiesController Sut { get; set; }

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

            // While working with a inMemoryDb there is no need for mocking IPropertyRepository
            // Instead working to implementation provides implementation test without affecting Db

            // var service = new Mock<IPropertyRepository>();
            // service.Setup(p => p.GetProperties()).Returns(inMemoryContext.Properties);
            var service = new PropertyRepository(inMemoryContext);
            Sut = new PropertiesController(service);
        }

        [TearDown]
        public void Teardown()
        {
            Mapper.Reset();
            _connection.Close();
        }

        [Test]
        public void GetRepositoriesReturnsOkResultAndCollectionOfProperties()
        {
            // Arrange
            // Act
            var result = Sut.GetProperties() as ObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<List<PropertyDto>>());
        }

        [Test]
        public void GetRepositoriesReturnsEmptyListIfDbEmpty()
        {
            // Arrange
            var service = new Mock<IPropertyRepository>();
            service.Setup(m => m.GetProperties()).Returns(new List<Property>());
            var sut = new PropertiesController(service.Object);

            // Act
            var result = sut.GetProperties() as OkObjectResult;


            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<List<PropertyDto>>());
            Assert.That(result.Value, Is.Empty);
        }

        [Test]
        public void GetPropertyReturnsOkAndSinglePropertyWithMatchingId()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");

            // Act
            var result = Sut.GetProperty(propertyId) as ObjectResult;

            // Assert
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<PropertyDto>());
            Assert.That(result.Value, Has.Property("Id").EqualTo(propertyId));
        }

        [Test]
        public void GetPropertyReturnsNotFoundIfPropertyIdDoesNotExist()
        {
            // Arrange
            var propertyId = new Guid();

            // Act
            var result = Sut.GetProperty(propertyId) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void CreatePropertyReturnsBadRequestIfRequestBodyHasIncorrectData()
        {
            // Arrange
            PropertyForCreationDto propertyForCreation = null;

            // Act
            var result = Sut.CreateProperty(propertyForCreation) as BadRequestResult;

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestResult>());
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void CreatePropertyAddsNewPropertyToDbReturnsRouteAndObject()
        {
            // Arrange
            var propertyForCreation = new PropertyForCreationDto()
            {
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
                        FirstName = "Some",
                        LastName = "Dude"
                    }
                }
            };

            // Act
            var result = Sut.CreateProperty(propertyForCreation) as CreatedAtRouteResult;

            // Assert
            Assert.That(result, Is.TypeOf<CreatedAtRouteResult>());
            Assert.That(result.StatusCode, Is.EqualTo(201));
            Assert.That(result.RouteValues.Values.FirstOrDefault(), Is.TypeOf<Guid>());
            Assert.That(result.Value, Is.TypeOf<PropertyDto>());
        }

        [Test]
        public void UpdatePropertyReturnsBadRequestWhenNoPatchDoc()
        {
            // Arrange
            JsonPatchDocument<PropertyForUpdateDto> propertyToPatch = null;
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");

            // Act
            var result = Sut.UpdateProperty(propertyId, propertyToPatch) as BadRequestResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void UpdatePropertyReturnsNotFoundIfPropertyNotExisting()
        {
            // Arrange
            var propertyId = new Guid("c8c44473-7c9c-4dd6-9462-d173a8e99da9");
            var patchDocument = new JsonPatchDocument<PropertyForUpdateDto>();

            // Act
            // Assert
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