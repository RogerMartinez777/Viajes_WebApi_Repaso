using Microsoft.AspNetCore.Mvc;
using ViajesRepository.Data.Interfaces;
using ViajesRepository.Domain;
using ViajesWebApi_Repaso.DTOs;

namespace ViajesWebApi_Repaso.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViajesController : ControllerBase
    {
        private readonly IViajeRepository _viajeRepository;
        private readonly IExcursionRepository _excursionRepository;

        // Inyectamos las interfaces de los repositorios
        public ViajesController(IViajeRepository viajeRepository, IExcursionRepository excursionRepository)
        {
            _viajeRepository = viajeRepository;
            _excursionRepository = excursionRepository;
        }

        // Excursiones ------------------------------------------------------

        // GET: api/viajes/excursiones
        [HttpGet("excursiones")]
        public async Task<IActionResult> GetExcursiones()
        {
            var excursiones = await _excursionRepository.GetAllAsync();
            return Ok(excursiones);
        }

        // Viajes -------------------------------------------------------

        // GET: api/viajes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var viajes = await _viajeRepository.GetAllAsync();
            return Ok(viajes);
        }

        // GET: api/viajes/no-cancelados
        [HttpGet("no-cancelados")]
        public async Task<IActionResult> GetNoCancelados()
        {
            var viajes = await _viajeRepository.GetNoCanceladosAsync();
            return Ok(viajes);
        }

        // GET: api/viajes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var viaje = await _viajeRepository.GetByIdAsync(id);
            if (viaje == null)
                return NotFound(new { mensaje = $"No se encontró el viaje con ID {id}." });

            return Ok(viaje);
        }

        // POST: api/viajes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Viaje viaje)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (viaje.ViajeDetalles == null || !viaje.ViajeDetalles.Any())
                return BadRequest(new { mensaje = "Un viaje debe incluir al menos una excursión en sus detalles." });

            try
            {
                var resultado = await _viajeRepository.CreateAsync(viaje);
                if (!resultado)
                    return StatusCode(500, new { mensaje = "Ocurrió un error al procesar el alta del viaje." });

                // Retorna 201 Created llamando al endpoint GetById
                return CreatedAtAction(nameof(GetById), new { id = viaje.Id }, viaje);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        //-------------------------------------------------------------------------------------------------------------------
        // JSON PARA EL POST:
        //{
        //  "destino": "Bariloche",
        //  "fechaInicio": "2026-11-01",
        //  "fechaFin": "2026-11-10",
        //  "estado": "Confirmado",
        //  "viajeDetalles": [
        //    {
        //      "excursionId": 1,
        //      "cantidadPersonas": 2
        //    },
        //    {
        //      "excursionId": 2,
        //      "cantidadPersonas": 1
        //    }
        //  ]
        //}

        // PUT: api/viajes/5/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] ActualizarEstadoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Pasamos el valor encapsulado en el DTO al repositorio
            var actualizado = await _viajeRepository.UpdateEstadoAsync(id, dto.NuevoEstado);
            if (!actualizado)
                return NotFound(new { mensaje = $"No se encontró el viaje con ID {id} para actualizar." });

            return Ok(new { mensaje = $"El estado del viaje {id} fue actualizado a '{dto.NuevoEstado}' correctamente." });
        }

        // DELETE: api/viajes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var viaje = await _viajeRepository.GetByIdAsync(id);
            if (viaje == null)
                return NotFound(new { mensaje = $"No se encontró el viaje con ID {id}." });

            if (viaje.Estado == "Cancelado")
                return BadRequest(new { mensaje = "No se puede eliminar un viaje que ya se encuentra cancelado." });

            var eliminado = await _viajeRepository.DeleteAsync(id);
            return Ok(new { mensaje = "Viaje eliminado correctamente." });
        }
    }
}