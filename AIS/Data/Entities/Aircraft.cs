using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace AIS.Data.Entities
{
    public class Aircraft : IEntity
    {
        #region Attributes

        int _rows;

        #endregion

        #region Properties

        public int Id { get; set; }

        [Required(ErrorMessage = "Aircraft model is required!")]
        public string Model { get; set; }

        [Required]
        [Range(10, 260, ErrorMessage = "Aircraft capacity must be between {1} and {2}!")]
        public int Capacity { get; set; }

        public List<string> Seats { get; set; } = new List<string>();

        [Required]
        [Range(5, 26, ErrorMessage = "Aircraft rows must be between {1} and {2}!")]
        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                GenerateSeats();
            }
        }

        [Display(Name = "Active")]
        public bool IsActive{ get; set; } = true;

        public User User { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        [Display(Name = "Image")]
        public string ImageDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                {
                    return $"/images/noimage.jpg";
                }
                else
                {
                    return $"{ImageUrl.Substring(1)}";
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate the list of seats
        /// </summary>
        private void GenerateSeats()
        {
            string[] ROW_LETTER = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            // Ensure we do not exceed the number of available row letters
            if (Rows > ROW_LETTER.Length)
            {
                Rows = ROW_LETTER.Length;
            }

            Seats.Clear();

            int seatsPerRow = Capacity / Rows;
            int extraSeats = Capacity % Rows;

            for (int row = 0; row < Rows; row++)
            {
                int numberOfSeatsInRow = (row < extraSeats) ? seatsPerRow + 1 : seatsPerRow;
                for (int seat = 1; seat <= numberOfSeatsInRow; seat++)
                {
                    Seats.Add($"{ROW_LETTER[row]}{seat}");
                }
            }
        }

        /// <summary>
        /// Format the list of seats for display
        /// </summary>
        public string FormatSeats()
        {
            if (Seats == null || !Seats.Any())
            {
                return string.Empty;
            }

            var seatsFormatted = new StringBuilder();
            string currentRow = string.Empty; // Start with an empty string

            foreach (var seat in Seats)
            {
                string seatRow = seat.Substring(0, 1); // Get the row character as a string

                if (seatRow != currentRow)
                {
                    if (!string.IsNullOrEmpty(currentRow))
                    {
                        // Add the accumulated seats of the previous row to the formatted string
                        seatsFormatted.AppendLine();
                    }

                    // Start a new row
                    currentRow = seatRow;
                }

                seatsFormatted.Append($"{seat} ");
            }

            return seatsFormatted.ToString().TrimEnd(); // Remove any trailing newline characters
        }

        #endregion
    }
}
