using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/test-exception")]
public sealed class TestExceptionController : ControllerBase
{
    [HttpGet("keynotfound")]
    public IActionResult ThrowKeyNotFound()
    {
        throw new KeyNotFoundException("El producto solicitado no existe.");
    }

    [HttpGet("argument")]
    public IActionResult ThrowArgument()
    {
        throw new ArgumentException("El nombre del producto no puede estar vacio.");
    }

    [HttpGet("invalidoperation")]
    public IActionResult ThrowInvalidOperation()
    {
        throw new InvalidOperationException("No se puede realizar esta operacion en el estado actual.");
    }

    [HttpGet("unauthorized")]
    public IActionResult ThrowUnauthorized()
    {
        throw new UnauthorizedAccessException("No tiene permisos para modificar este recurso.");
    }

    [HttpGet("generic")]
    public IActionResult ThrowGeneric()
    {
        throw new Exception("Ocurrio un error inesperado. Intenta nuevamente o contacta a soporte.");
    }
}
