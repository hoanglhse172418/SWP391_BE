﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Vaccine;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.backend.services
{
    public class SVaccine : IVaccine
    {
        private readonly IConfiguration _configuration;
        private readonly swpContext context;

        public SVaccine(IConfiguration configuration, swpContext context)
        {
            _configuration = configuration;
            this.context = context;
        }

        public async Task<List<VaccineDTO>> GetAllVaccine()
        {
            try
            {
                var vaccineList = await context.Vaccines
                    .Where(v => !v.IsDelete ) // Lọc các vaccine chưa bị xóa
                    .Include(v => v.Diseases) // Load diseases liên quan
                    .OrderBy(v => v.Diseases.Min(d => d.Id))
                    .ToListAsync();

                var result = vaccineList.Select(v => new VaccineDTO
                {
                    Id = v.Id,
                    Name = v.Name,
                    Manufacture = v.Manufacture,
                    Description = v.Description,
                    ImageUrl = v.ImageUrl,
                    RecAgeStart = v.RecAgeStart,
                    RecAgeEnd = v.RecAgeEnd,
                    InStockNumber = v.InStockNumber,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    Price = v.Price,
                    DiseaseIds = v.Diseases?.Select(d => d.Id).ToList() ?? new List<int>()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching vaccine list: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> GetById(int id)
        {
            try
            {
                var vaccine = await context.Vaccines.FirstOrDefaultAsync(v => v.Id == id);
                return vaccine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get vaccine: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> Create(CreateVaccineDTO request, string imageUrl)
        {
            try
            {
                var newVaccine = new Vaccine
                {
                    Name = request.VaccineName,
                    Manufacture = request.Manufacture,
                    Description = request.Description,
                    ImageUrl = imageUrl,
                    RecAgeStart = request.RecAgeStart,
                    RecAgeEnd = request.RecAgeEnd,
                    InStockNumber = request.InStockNumber,
                    Price = request.Price,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Gán danh sách Disease nếu có
                if (request.DiseaseIds != null && request.DiseaseIds.Any())
                {
                    var diseases = await context.Diseases
                        .Where(d => request.DiseaseIds.Contains(d.Id))
                        .ToListAsync();

                    var missingIds = request.DiseaseIds.Except(diseases.Select(d => d.Id)).ToList();
                    if (missingIds.Any())
                    {
                        throw new Exception($"Disease ID(s) not found: {string.Join(", ", missingIds)}");
                    }

                    newVaccine.Diseases = diseases;
                }

                context.Vaccines.Add(newVaccine);
                await context.SaveChangesAsync();
                return newVaccine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating vaccine: {ex.Message}", ex);
            }
        }

        public async Task<Vaccine> Update(int vaccineId, UpdateVaccineDTO request, string imageUrl)
        {
            try
            {
                var foundVaccine = await context.Vaccines
                    .Include(v => v.Diseases)
                    .FirstOrDefaultAsync(v => v.Id == vaccineId);
                    string foundVaccineImg = foundVaccine.ImageUrl;

                if (foundVaccine == null)
                {
                    throw new KeyNotFoundException($"Vaccine with ID: {vaccineId} not found");
                }

                foundVaccine.Name = string.IsNullOrEmpty(request.VaccineName) ? foundVaccine.Name : request.VaccineName;
                foundVaccine.Manufacture = string.IsNullOrEmpty(request.Manufacture) ? foundVaccine.Manufacture : request.Manufacture;
                foundVaccine.Description = string.IsNullOrEmpty(request.Description) ? foundVaccine.Description : request.Description;
                foundVaccine.ImageUrl = string.IsNullOrEmpty(imageUrl) ? foundVaccineImg : imageUrl;
                foundVaccine.RecAgeStart = string.IsNullOrEmpty(request.RecAgeStart.ToString()) ? foundVaccine.RecAgeStart : request.RecAgeStart;
                foundVaccine.RecAgeEnd = string.IsNullOrEmpty(request.RecAgeEnd.ToString()) ? foundVaccine.RecAgeEnd : request.RecAgeEnd;
                foundVaccine.Notes = string.IsNullOrEmpty(request.Notes) ? foundVaccine.Notes : request.Notes;
                foundVaccine.InStockNumber = request.InStockNumber ?? foundVaccine.InStockNumber;
                foundVaccine.Price = string.IsNullOrEmpty(request.Price) ? foundVaccine.Price : request.Price;
                foundVaccine.UpdatedAt = DateTime.UtcNow;

                // Cập nhật Disease nếu có
                if (request.DiseaseIds != null)
                {
                    var diseases = await context.Diseases
                        .Where(d => request.DiseaseIds.Contains(d.Id))
                        .ToListAsync();

                    var missingIds = request.DiseaseIds.Except(diseases.Select(d => d.Id)).ToList();
                    if (missingIds.Any())
                    {
                        throw new Exception($"Disease ID(s) not found: {string.Join(", ", missingIds)}");
                    }

                    foundVaccine.Diseases.Clear();
                    foreach (var disease in diseases)
                    {
                        foundVaccine.Diseases.Add(disease);
                    }
                }

                await context.SaveChangesAsync();
                return foundVaccine;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vaccine: {ex.Message}", ex);
            }
        }

        public async Task<List<Vaccine>> GetAllVaccinesByDiasease(string diaseaseName)
        {
            try
            {
                var vaccines = await context.Vaccines
                    .Where(v => v.Diseases.Any(d => d.Name.ToLower() == diaseaseName.ToLower()))
                    .ToListAsync();

                return vaccines;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving vaccines: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int vaccineId)
        {
            try
            {
                var foundVaccine = await context.Vaccines
                    .FirstOrDefaultAsync(v => v.Id == vaccineId);

                if (foundVaccine == null)
                {
                    throw new KeyNotFoundException($"Vaccine with ID: {vaccineId} not found");
                }

                // Soft delete: đánh dấu IsDelete = true
                foundVaccine.IsDelete = true;

                context.Vaccines.Update(foundVaccine);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting vaccine: {ex.Message}", ex);
            }
        }
    }
}
