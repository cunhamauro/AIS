using System;
using System.IO;

namespace AIS.Helpers
{
    public interface IPdfHelper
    {
        MemoryStream GenerateTicketPdf(string nameWithTitle, string idNumber, string flightNumber, string originCityCountry, string destinationCityCountry, string seat, DateTime departure, DateTime arrival, MemoryStream qrCode);

        MemoryStream GenerateInvoicePdf(string name, string flightNumber, decimal price, bool userCancel, bool flightCancel);
    }
}
