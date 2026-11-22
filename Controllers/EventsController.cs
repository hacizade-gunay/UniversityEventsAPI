using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using UniversityEventsAPI.Models;

namespace UniversityEventsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private static List<Event> _events = new List<Event>
        {
            new Event
            {
                Id = 1,
                Name = "AI Conference 2027",
                Description = "Artificial Intelligence Conference",
                Location = "Baku",
                EventDate = new DateTime(2027, 5, 20, 10, 0, 0),
                MaxParticipants = 150
            },
            new Event
            {
                Id = 2,
                Name = "Cyber Security Summit",
                Description = "University Cyber Security Workshop",
                Location = "Sumgayit",
                EventDate = new DateTime(2027, 6, 15, 14, 0, 0),
                MaxParticipants = 80
            }
        };

        // GET: /api/events?name=...&location=...&onlyUpcoming=true&minParticipants=100&sortByDate=true
        [HttpGet]
        public ActionResult<IEnumerable<Event>> GetAllEvents(
            [FromQuery] string? name,
            [FromQuery] string? location,
            [FromQuery] bool onlyUpcoming = false,
            [FromQuery] int? minParticipants = null,
            [FromQuery] bool sortByDate = false)
        {
            IEnumerable<Event> query = _events;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
            }

            if (onlyUpcoming)
            {
                query = query.Where(e => e.EventDate > DateTime.Now);
            }

            if (minParticipants.HasValue)
            {
                query = query.Where(e => e.MaxParticipants > minParticipants.Value);
            }

            if (sortByDate)
            {
                query = query.OrderBy(e => e.EventDate);
            }

            return Ok(query.ToList());
        }

        // GET: /api/events/{id}
        [HttpGet("{id}")]
        public ActionResult<Event> GetEventById(int id)
        {
            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                return NotFound(new { message = $"ID-si {id} olan tədbir tapılmadı." });
            }

            return Ok(ev);
        }

        // POST: /api/events
        [HttpPost]
        public ActionResult<Event> CreateEvent([FromBody] Event newEvent)
        {
            var validationError = ValidateEvent(newEvent);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            newEvent.Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1;
            _events.Add(newEvent);

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // PUT: /api/events/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var existingEvent = _events.FirstOrDefault(e => e.Id == id);
            if (existingEvent == null)
            {
                return NotFound(new { message = $"ID-si {id} olan tədbir tapılmadı." });
            }

            var validationError = ValidateEvent(updatedEvent);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            existingEvent.Name = updatedEvent.Name;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.MaxParticipants = updatedEvent.MaxParticipants;

            return Ok(existingEvent);
        }

        // DELETE: /api/events/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            var existingEvent = _events.FirstOrDefault(e => e.Id == id);
            if (existingEvent == null)
            {
                return NotFound(new { message = $"ID-si {id} olan tədbir tapılmadı." });
            }

            _events.Remove(existingEvent);
            return NoContent();
        }

        // GET: /api/events/export
        [HttpGet("export")]
        public IActionResult ExportEventsToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_events, options);
            byte[] fileBytes = Encoding.UTF8.GetBytes(jsonString);

            return File(fileBytes, "application/json", "university_events.json");
        }

        private string? ValidateEvent(Event ev)
        {
            if (ev == null)
                return "Tədbir məlumatları boş ola bilməz.";

            if (string.IsNullOrWhiteSpace(ev.Name))
                return "Tədbirin adı boş ola bilməz.";

            if (ev.EventDate < DateTime.Now)
                return "Tədbirin tarixi keçmiş tarix ola bilməz.";

            if (ev.MaxParticipants < 1)
                return "İştirakçı sayı 1-dən az ola bilməz.";

            return null;
        }
    }
}
