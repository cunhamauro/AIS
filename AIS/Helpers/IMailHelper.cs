namespace AIS.Helpers
{
    public interface IMailHelper
    {
        Response SendEmail(string to, string subject, string body);

        string GetHtmlTemplate(string title, string subtitle, string button, string tokenLink);
    }
}
