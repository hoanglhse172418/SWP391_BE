using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SRoom : IRoom
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SRoom(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<Room>> GetAll()
        {
            return context.Rooms.ToList();
        }

        public async Task<Room> Create(string roomNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomNumber))
                    throw new ArgumentException("Room number cannot be null or empty.");

                // Normalize room number (trim and uppercase for consistency)
                roomNumber = roomNumber.Trim().ToUpper();

                // Check if room number already exists
                var existingRoom = await context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
                if (existingRoom != null)
                    throw new InvalidOperationException("Room number already exists!");

                // Create new room
                var newRoom = new Room
                {
                    RoomNumber = roomNumber
                };

                await context.Rooms.AddAsync(newRoom);
                await context.SaveChangesAsync();

                return newRoom;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Room creation failed: {ex.Message}", ex);
            }
        }

        public async Task<Room> Update(int id, string roomNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomNumber))
                    throw new ArgumentException("Room number cannot be null or empty.");
                // Normalize room number (trim and uppercase for consistency)
                roomNumber = roomNumber.Trim().ToUpper();
                // Check if room number already exists
                var existingRoom = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
                if (existingRoom != null)
                    throw new InvalidOperationException("Room number already exists!");
                // Get room by id
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
                if (room == null)
                    throw new InvalidOperationException("Room not found!");
                // Update room number
                room.RoomNumber = roomNumber;
                context.Rooms.Update(room);
                await context.SaveChangesAsync();
                return room;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Room update failed: {ex.Message}", ex);
            }
        }

        public async Task<Room> GetById(int id)
        {
            return await context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                // Get room by id
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
                if (room == null)
                    throw new InvalidOperationException("Room not found!");
                // Check if room has appointments
                if (room.Appointments.Count > 0)
                    throw new InvalidOperationException("Room has appointments!");
                // Delete room
                context.Rooms.Remove(room);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Room deletion failed: {ex.Message}", ex);
            }
        }
    }
}
