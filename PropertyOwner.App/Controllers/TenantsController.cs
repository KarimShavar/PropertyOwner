using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.App.Controllers
{
    [Route("/api/properties/{PropertyId}/tenants")]
    public class TenantsController : Controller
    {
        private IPropertyRepository _propertyRepository;

        public TenantsController(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        [HttpGet]
        public IActionResult GetTenantsForProperty(Guid id)
        {
            if (!_propertyRepository.PropertyExist(id))
            {
                return NotFound();
            }

            var tenantsForPropertyFromRepo = _propertyRepository.GetTenantsForProperty(id);
            if (tenantsForPropertyFromRepo == null)
            {
                return NotFound();
            }

            var tenants = Mapper.Map<IEnumerable<TenantDto>>(tenantsForPropertyFromRepo);
            return Ok(tenants);
        }

        [HttpGet("{id}", Name = "GetTenant")]
        public IActionResult GetTenantForProperty(Guid propertyId, Guid tenantId)
        {
            if (!_propertyRepository.PropertyExist(propertyId))
            {
                return NotFound();
            }

            var tenantFromRepo = _propertyRepository.GetTenantsForProperty(propertyId)
                .FirstOrDefault(t => t.Id == tenantId);
            if (tenantFromRepo == null)
            {
                return NotFound();
            }

            var tenant = Mapper.Map<TenantDto>(tenantFromRepo);
            return Ok(tenant);
        }

        [HttpPost]
        public IActionResult CreateTenantForProperty(Guid propertyId,
            [FromBody] TenantForCreationDto tenant)
        {
            if (tenant == null)
            {
                return BadRequest();
            }

            if (!_propertyRepository.PropertyExist(propertyId))
            {
                return NotFound();
            }

            var tenantEntity = Mapper.Map<Tenant>(tenant);

            _propertyRepository.AddTenantToProperty(propertyId, tenantEntity);

            if (!_propertyRepository.Save())
            {
                throw new Exception($"Adding tenant {tenant.FirstName} to property {propertyId} failed on save.");
            }

            var tenantToReturn = Mapper.Map<TenantDto>(tenantEntity);

            return CreatedAtRoute("GetTenant",
                new {propertyId, id = tenantToReturn.Id},
                tenantToReturn);
        }

        [HttpPatch]
        public IActionResult UpdateTenantForProperty(Guid propertyId, Guid tenantId,
            JsonPatchDocument<TenantForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_propertyRepository.PropertyExist(propertyId))
            {
                return NotFound();
            }

            var tenantFromRepo = _propertyRepository.GetTenantsForProperty(propertyId)
                .FirstOrDefault(t => t.Id == tenantId);
            if (tenantFromRepo == null)
            {
                return NotFound();
            }

            var tenantForUpdate = Mapper.Map<TenantForUpdateDto>(tenantFromRepo);
            patchDoc.ApplyTo(tenantForUpdate);

            Mapper.Map(tenantForUpdate, tenantFromRepo);
            _propertyRepository.UpdateTenant(tenantFromRepo);

            if (!_propertyRepository.Save())
            {
                throw new Exception($"Updating tenant {tenantId} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete]
        public IActionResult DeleteTenantFromProperty(Guid propertyId, Guid tenantId)
        {
            if (!_propertyRepository.PropertyExist(propertyId))
            {
                return NotFound();
            }

            var tenantFromRepo = _propertyRepository.GetTenantsForProperty(tenantId)
                .FirstOrDefault(t => t.Id == tenantId);
            if (tenantFromRepo == null)
            {
                return NotFound();
            }

            _propertyRepository.DeleteTenant(tenantFromRepo);

            if (!_propertyRepository.Save())
            {
                throw new Exception($"Deleting tenant {tenantId} failed on save.");
            }

            return NoContent();
        }
    }
}