﻿using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AIS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AircraftsController : Controller
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IFlightRepository _flightRepository;

        public AircraftsController(IAircraftRepository aircraftRepository, IUserHelper userHelper, IImageHelper imageHelper, IConverterHelper converterHelper, IFlightRepository flightRepository)
        {
            _aircraftRepository = aircraftRepository;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
            _flightRepository = flightRepository;
        }

        // GET: Aircrafts
        public IActionResult Index()
        {
            return View(_aircraftRepository.GetAll());
        }

        // GET: Aircrafts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return AircraftNotFound();
            }

            Aircraft aircraft = await _aircraftRepository.GetByIdAsync(id.Value);

            if (aircraft == null)
            {
                return AircraftNotFound();
            }

            return View(aircraft);
        }

        // GET: Aircrafts/Create
        public IActionResult Create()
        {
            AircraftViewModel viewModel = new AircraftViewModel
            {
                IsActive = true // Set Status to true by default
            };

            return View(viewModel);
        }

        // POST: Aircrafts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Assign user
                var currentUser = await _userHelper.GetUserAsync(User);

                viewModel.User = currentUser; // Assign the current user to the aircraft

                // Save image
                var path = string.Empty; // Default no image

                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(viewModel.ImageFile, viewModel.Model, "aircrafts");
                }

                //Convert to aircraft
                var aircraft = _converterHelper.ToAircraft(viewModel, path, true);

                await _aircraftRepository.CreateAsync(aircraft);

                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Aircrafts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return AircraftNotFound();
            }

            Aircraft aircraft = await _aircraftRepository.GetByIdAsync(id.Value);

            if (aircraft == null)
            {
                return AircraftNotFound();
            }

            AircraftViewModel viewModel = _converterHelper.ToAircraftViewModel(aircraft);

            return View(viewModel);
        }

        // POST: Aircrafts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftViewModel viewModel)
        {
            if (await _flightRepository.AircraftInFlights(viewModel.Id) && viewModel.IsActive == false)
            {
                ModelState.AddModelError("IsActive", "This Aircraft is in an active flight, status cannot be updated!");
                viewModel.IsActive = true;
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Update the image
                var path = viewModel.ImageUrl;
                string oldPath = path;

                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(viewModel.ImageFile, viewModel.Model, "aircrafts");
                }

                Aircraft aircraft = _converterHelper.ToAircraft(viewModel, path, false);

                // Update the user
                var currentUser = await _userHelper.GetUserAsync(User);

                aircraft.User = currentUser; // Change the user to the current active one

                await _aircraftRepository.UpdateAsync(aircraft);

                if (path != oldPath && oldPath != @"~/images/noimage.png") // Dont delete if its the no image
                {
                    // Delete old image when it is different from the new one (updated)
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        _imageHelper.DeleteImage(oldPath);
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _aircraftRepository.ExistAsync(viewModel.Id))
                {
                    return AircraftNotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Aircrafts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return AircraftNotFound();
            }

            Aircraft aircraft = await _aircraftRepository.GetByIdAsync(id.Value);

            if (aircraft == null)
            {
                return AircraftNotFound();
            }

            if (await _flightRepository.AircraftInFlights(id.Value))
            {
                ViewBag.ShowMsg = true;
                ViewBag.Status = "disabled";
            }

            return View(aircraft);
        }

        // POST: Aircrafts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Aircraft aircraft = await _aircraftRepository.GetByIdAsync(id);

            if (await _flightRepository.AircraftInFlights(id))
            {
                return RedirectToAction("Delete", "Aircrafts", new { id = id });
            }

            // Delete image when Aircraft is also deleted
            if (!string.IsNullOrEmpty(aircraft.ImageUrl) && aircraft.ImageUrl != @"~/images/noimage.png") // Dont delete if its the no image
            {
                _imageHelper.DeleteImage(aircraft.ImageUrl);
            }

            await _aircraftRepository.DeleteAsync(aircraft);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult AircraftNotFound()
        {
            return View("DisplayMessage", new DisplayMessageViewModel { Title = "Aircraft not found", Message = $"Did it fly over the Bermuda Triangle?" });
        }
    }
}
