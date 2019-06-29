using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PropertyOwner.App.Controllers;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.Tests
{
    public class TenantsControllerTests
    {
        [SetUp]
        public void Setup()
        {
            InitializeMapping();
        }


        [Test]
        public void GetTenantsForPropertyReturnsNotFoundIdDoesNotExist()
        {
            // Arrange
            var propertyId = new Guid();
            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(false);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.GetTenantsForProperty(propertyId) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            Assert.That(result.StatusCode, Is.EqualTo(404));
            mockRepository.Verify(m => m.PropertyExist(propertyId), Times.Once);
        }

        [Test]
        public void GetTenantsForPropertyReturnsNotFoundIfRepositoryGetFails()
        {
            // Arrange
            var propertyId = new Guid();
            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(true);
            mockRepository.Setup(r => r.GetTenantsForProperty(propertyId))
                .Returns((IEnumerable<Tenant>) null);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.GetTenantsForProperty(propertyId) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            Assert.That(result.StatusCode, Is.EqualTo(404));
            mockRepository.Verify(m => m.PropertyExist(propertyId), Times.Once);
            mockRepository.Verify(m => m.GetTenantsForProperty(propertyId), Times.Once);
        }

        [Test]
        public void GetTenantsForPropertyReturnsOkAndTenantDtoCollection()
        {
            // Arrange
            var propertyId = new Guid();
            var fakeTenants = new List<Tenant>();
            fakeTenants.Add(
                new Faker<Tenant>()
                    .RuleFor(t => t.Id, t => new Guid())
                    .RuleFor(t => t.PropertyId, t => propertyId));

            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(true);
            mockRepository.Setup(r => r.GetTenantsForProperty(propertyId))
                .Returns(fakeTenants);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.GetTenantsForProperty(propertyId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.TypeOf<OkObjectResult>());
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<List<TenantDto>>());
        }

        [Test]
        public void GetTenantForPropertyReturnsNotFoundIfPropertyDoesNotExist()
        {
            // Arrange
            var propertyId = new Guid();
            var tenantId = new Guid();
            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(false);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.GetTenantForProperty(propertyId, tenantId) as NotFoundResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            Assert.That(result.StatusCode, Is.EqualTo(404));
            mockRepository.Verify(m => m.PropertyExist(propertyId), Times.Once);
        }

        [Test]
        public void GetTenantForPropertyReturnsOkAndSingleTenant()
        {
            // Arrange
            var tenantId = new Guid();
            var propertyId = new Guid();
            var fakeTenant = new Faker<Tenant>()
                .RuleFor(t => t.Id, t => tenantId);
            var tenantList = new List<Tenant>();
            tenantList.Add(fakeTenant);
            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(true);
            mockRepository.Setup(r => r.GetTenantsForProperty(propertyId))
                .Returns(tenantList);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.GetTenantForProperty(propertyId, tenantId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<TenantDto>());
            mockRepository.Verify(r => r.GetTenantsForProperty(propertyId), Times.Once);
        }

        [Test]
        public void CreateTenantForPropertyRerturnsBadRequestIfTenantIsNull()
        {
            // Arrange
            TenantForCreationDto tenant = null;
            var propertyId = new Guid();

            var mockRepository = new Mock<IPropertyRepository>();
            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.CreateTenantForProperty(propertyId, tenant) as BadRequestResult;

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestResult>());
            Assert.That(result.StatusCode, Is.EqualTo(400));
            mockRepository.Verify(r => r.PropertyExist(propertyId), Times.Never);
        }

        [Test]
        public void CreateTenantForPropertyReturnsNotFoundIfPropertyDoesNotExist()
        {
            // Arrange
            var propertyId = new Guid();
            var newTenant = new Faker<TenantForCreationDto>().Generate();

            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId))
                .Returns(false);

            var sut = new TenantsController(mockRepository.Object);

            // Act
            var result = sut.CreateTenantForProperty(propertyId, newTenant) as NotFoundResult;

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
            Assert.That(result.StatusCode, Is.EqualTo(404));
            mockRepository.Verify(r => r.PropertyExist(propertyId), Times.Once);
        }

        [Test]
        public void CreateTenantForPropertyThrowsExceptionIfRepositorySaveFails()
        {
            // Arrange
            var propertyId = new Guid();
            var fakeTenant = new Faker<TenantForCreationDto>().Generate();

            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId)).Returns(true);
            mockRepository.Setup(r => r.Save()).Returns(false);

            var sut = new TenantsController(mockRepository.Object);
            var tenantDto = Mapper.Map<Tenant>(fakeTenant);

            // Act
            sut.CreateTenantForProperty(propertyId, fakeTenant);
            
            // Assert
            mockRepository.Verify(r => r.PropertyExist(propertyId), Times.Once);
            Assert.Throws<Exception>(
                () => sut.CreateTenantForProperty(propertyId, fakeTenant));
        }

        [Test]
        public void CreateTenantForPropertyReturnsCreatedAtRouteNewRouteAndTenant()
        {
            // Arrange
            var propertyId = new Guid();
            var fakeTenant = new Faker<TenantForCreationDto>().Generate();

            var mockRepository = new Mock<IPropertyRepository>();
            mockRepository.Setup(r => r.PropertyExist(propertyId)).Returns(true);
            mockRepository.Setup(r => r.Save()).Returns(true);

            var sut = new TenantsController(mockRepository.Object);
            var tenant = Mapper.Map<Tenant>(fakeTenant);
            var tenantDto = Mapper.Map<TenantDto>(tenant);

            // Act
            var result = sut.CreateTenantForProperty(propertyId, fakeTenant) as CreatedAtRouteResult;

            // Assert
            mockRepository.Verify(r => r.PropertyExist(propertyId), Times.Once);
            mockRepository.Verify(r => r.Save(), Times.Once);
            Assert.That(result.StatusCode, Is.EqualTo(201));
            Assert.That(result.RouteName, Is.EqualTo("GetTenant"));
            Assert.That(result.Value, Is.TypeOf<TenantDto>());
        } 
        
        private void InitializeMapping()
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