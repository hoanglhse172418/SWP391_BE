using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SWP391.backend.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimerController : ControllerBase
    {
        private static Timer _timer;
        private static readonly object _lock = new object();

        public TimerController()
        {
            // Initialize the timer if it is not already initialized
            if (_timer == null)
            {
                lock (_lock)
                {
                    if (_timer == null)
                    {
                        _timer = new Timer(UpdateTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                    }
                }
            }
        }

        // This method will be called by the timer
        private void UpdateTime(object state)
        {
            // Your code to update time here
            Console.WriteLine("Time updated: " + DateTime.Now);
        }

        [HttpGet("StartTimer")]
        public IActionResult StartTimer()
        {
            // Start the timer
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Ok("Timer started");
        }

        [HttpGet("StopTimer")]
        public IActionResult StopTimer()
        {
            // Stop the timer
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            return Ok("Timer stopped");
        }
    }
}
