using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using PropertyOwner.App.Data.Entities;
using PropertyOwner.App.Data.Models;
using PropertyOwner.App.Services;

namespace PropertyOwner.App.Controllers
{
    [Route("api/properties")]
    public class PropertiesController : Controller
    {
        private readonly IPropertyRepository _propertyRepository;

        public PropertiesController(IPropertyRepository propertyRepository)
        {
            // Todo Implement logging through app.
            _propertyRepository = propertyRepository;
        }

        [HttpGet]
        public IActionResult GetProperties()
        {
            var propertiesFromRepo = _propertyRepository.GetProperties();

            var properties = Mapper.Map<IEnumerable<PropertyDto>>(propertiesFromRepo);

            return Ok(properties);
        }

        [HttpGet("{id}", Name = "GetProperty")]
        public IActionResult GetProperty(Guid id)
        {
            var propertyFromRepo = _propertyRepository.GetProperty(id);
            if (propertyFromRepo == null)
            {
                return NotFound();
            }

            var property = Mapper.Map<PropertyDto>(propertyFromRepo);

            return Ok(property);
        }

        [HttpPost]
        public IActionResult CreateProperty([FromBody] PropertyForCreationDto property)
        {
            if (property == null)
            {
                return BadRequest();
            }

            var propertyEntity = Mapper.Map<Property>(property);
            _propertyRepository.AddProperty(propertyEntity);

            if (!_propertyRepository.Save())
            {
                throw new Exception("Creating property failed on save.");
            }

            var propertyToReturn = Mapper.Map<PropertyDto>(propertyEntity);

            return CreatedAtRoute("GetProperty",
                new {id = propertyToReturn.Id},
                propertyToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockPropertyCreation(Guid id)
        {
            if (_propertyRepository.PropertyExist(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateProperty(Guid id,
            JsonPatchDocument<PropertyForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_propertyRepository.PropertyExist(id))
            {
                return NotFound();
            }

            var propertyFromRepo = _propertyRepository.GetProperty(id);

            var propertyToUpdate = Mapper.Map<PropertyForUpdateDto>(propertyFromRepo);
            patchDoc.ApplyTo(propertyToUpdate);

            Mapper.Map(propertyToUpdate, propertyFromRepo);
            _propertyRepository.UpdateProperty(propertyFromRepo);

            if (!_propertyRepository.Save())
            {
                throw new Exception($"Updating property {id} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProperty(Guid id)
        {
            if (!_propertyRepository.PropertyExist(id))
            {
                return NotFound();
            }

            var propertyToDelete = _propertyRepository.GetProperty(id);

            _propertyRepository.DeleteProperty(propertyToDelete);

            if (!_propertyRepository.Save())
            {
                throw new Exception($"Deleting property {id} failed on save.");
            }

            return NoContent();
        }
    }
}