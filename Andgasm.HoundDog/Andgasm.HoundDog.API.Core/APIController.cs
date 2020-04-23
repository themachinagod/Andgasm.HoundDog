using Andgasm.HoundDog.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Andgasm.HoundDog.API.Core
{
    public class APIController : ControllerBase
    {
        private const string ServerErrorKey = "ServerError";

        public IActionResult CreateBadRequestError(string fieldname, string error)
        {
            if (string.IsNullOrWhiteSpace(fieldname)) fieldname = ServerErrorKey;
            ModelState.AddModelError(fieldname, error);
            var response = new ValidationProblemDetails(ModelState);
            return BadRequest(response);
        }

        public IActionResult CreateBadRequestError(IEnumerable<FieldValidationErrorDTO> errors)
        {
            foreach (var error in errors)
            {
                if (string.IsNullOrWhiteSpace(error.Key)) error.Key = ServerErrorKey;
                ModelState.AddModelError(error.Key, error.Description);
            }
            var response = new ValidationProblemDetails(ModelState);
            return BadRequest(response);
        }

        public IActionResult CreateUnauthorizedError(FieldValidationErrorDTO error)
        {
            if (string.IsNullOrWhiteSpace(error.Key)) error.Key = ServerErrorKey;
            ModelState.AddModelError(error.Key, error.Description);
            var response = new ValidationProblemDetails(ModelState);
            return Unauthorized(response);
        }
    }
}
