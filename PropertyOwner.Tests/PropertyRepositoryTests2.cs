using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.Tests
{
    public class PropertyRepositoryTests2
    {
        [Test]
        public void GetPropertiesReturnsReturnsTwoProperties()
        {
            // Arrange
            var repositoryMock = new Mock<IPropertyRepository>();
            repositoryMock.Setup(m => m.GetProperties())
                .Returns(GenerateFakeProperties);
            var sut = repositoryMock.Object;
            
            // Act
            var result = sut.GetProperties();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        public void GetPropertyReturnsPropertyWithMatchingId()
        {
            // Arrange
            var propertyId = new Guid("0b396766-0a09-4ce3-95f2-3c5b2735f513");
            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(m => m.GetProperty(propertyId))
                .Returns(GenerateFakeProperties().FirstOrDefault(p => p.Id == propertyId));
            var sut = mockRepository.Object;
            
            // Act
            var result = sut.GetProperty(propertyId);
            
            // Assert
            Assert.That(result.Id, Is.EqualTo(propertyId));
        }

        public void AddPropertyAssingsNewGuidToProperty()
        {
            // Arrange
            var propertyToAdd = new Property()
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
            var mockRepository = new Mock<IPropertyRepository>();
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
            Mapper.Reset();
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