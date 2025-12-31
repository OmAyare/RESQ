using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESQ_API.Data;
using RESQ_API.Domain.Entities;

namespace RESQ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyEventsController : ControllerBase
    {
        private readonly RESQ_DbContext _resq_dbcontext;

        public EmergencyEventsController(RESQ_DbContext resq_dbcontext) => _resq_dbcontext = resq_dbcontext;

        // GET: api/emergencyevents/byuser/{userId}
        [HttpGet("byuser/{userId}")]
        public async Task<ActionResult<IEnumerable<EmergencyEvent>>> GetByUser(int userId)
        {
            return await _resq_dbcontext.EmergencyEvents
                                 .Where(e => e.UserId == userId)
                                 .ToListAsync();
        }


        // GET: api/emergencyevents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencyEvent>> GetById(int id)
        {
            var ev = await _resq_dbcontext.EmergencyEvents
                                   .Include(e => e.User)
                                   .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return ev;
        }

        // POST: api/emergencyevents
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmergencyEvent ev)
        {
            // ignore incoming User navigation if present
            ev.User = null;
            ev.SessionId = Guid.NewGuid();
            ev.EventDateTime = DateTime.UtcNow;

            _resq_dbcontext.EmergencyEvents.Add(ev);
            await _resq_dbcontext.SaveChangesAsync();


            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
        }

        //[HttpPost]
        //public async Task<IActionResult> Create(EmergencyEvent ev)
        //{
        //    _resq_dbcontext.EmergencyEvents.Add(ev);
        //    await _resq_dbcontext.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
        //}

        // PUT: api/emergencyevents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EmergencyEvent updatedEvent)
        {
            if (id != updatedEvent.EventId)
            {
                return BadRequest("Event ID mismatch.");
            }

            var existingEvent = await _resq_dbcontext.EmergencyEvents.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound("Event not found.");
            }

            // Update mutable fields
            existingEvent.Latitude = updatedEvent.Latitude;
            existingEvent.Longitude = updatedEvent.Longitude;
            existingEvent.Status = updatedEvent.Status;

            await _resq_dbcontext.SaveChangesAsync();
            return NoContent(); // 204
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] EmergencyEvent ev)
        {
            ev.User = null;

            // Create session id
            ev.SessionId = Guid.NewGuid();
            ev.EventDateTime = DateTime.UtcNow;

            _resq_dbcontext.EmergencyEvents.Add(ev);
            await _resq_dbcontext.SaveChangesAsync();

            return Ok(new
            {
                SessionId = ev.SessionId,
                EventId = ev.EventId
            });
        }


        [HttpPut("update/{sessionId}")]
        public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] EmergencyEvent updatedEvent)
        {
            var ev = await _resq_dbcontext.EmergencyEvents
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.EventDateTime)
                .FirstOrDefaultAsync();

            if (ev == null)
                return NotFound();

            ev.Latitude = updatedEvent.Latitude;
            ev.Longitude = updatedEvent.Longitude;
            ev.Status = updatedEvent.Status;
            ev.EventDateTime = DateTime.UtcNow;

            await _resq_dbcontext.SaveChangesAsync();
            return NoContent(); // 204
        }


        [HttpPut("end/{sessionId}")]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            var list = await _resq_dbcontext.EmergencyEvents
                .Where(x => x.SessionId == sessionId)
                .ToListAsync();

            if (!list.Any())
                return NotFound();

            foreach (var item in list)
                item.Status = "Safe";

            await _resq_dbcontext.SaveChangesAsync();
            return NoContent(); // 204
        }


    }
}
