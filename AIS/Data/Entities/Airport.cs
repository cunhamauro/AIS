﻿using System.ComponentModel.DataAnnotations;

namespace AIS.Data.Entities
{
    public class Airport : IEntity
    {
        #region Properties

        public int Id { get; set; }

        [Required]
        [StringLength(3, ErrorMessage = "Airport IATA codes must have a length of {1} characters!")]
        public string IATA { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public User User { get; set; }

        [Display(Name = "Country Flag")]
        public string ImageUrl {  get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
