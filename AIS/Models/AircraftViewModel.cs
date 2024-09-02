using AIS.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class AircraftViewModel : Aircraft
    {
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }    
    }
}
