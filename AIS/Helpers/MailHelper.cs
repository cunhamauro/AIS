using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Net.Mail;

namespace AIS.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generate AIS email template
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="subtitle">Subtitle</param>
        /// <param name="button">Button text</param>
        /// <param name="tokenLink">Token link</param>
        /// <returns>Email HTML body</returns>
        public string GetHtmlTemplate(string title, string subtitle, string button, string tokenLink)
        {
            return $@"<body class=""body"" style=""background-color:#005a9c;margin:0;padding:0;-webkit-text-size-adjust:none;text-size-adjust:none"">
                        <table class=""nl-container"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;background-color:#005a9c"">
                            <tbody>
                                <tr>
                                    <td>
                                        <table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;background-color:#fff;color:#000;width:510px;margin:0 auto"" width=""510"">
                                                            <tbody>
                                                                <tr>
                                                                    <td class=""column column-1"" width=""100%"" style=""mso-table-lspace:0;mso-table-rspace:0;font-weight:400;text-align:left;padding-bottom:30px;padding-left:30px;padding-right:30px;padding-top:30px;vertical-align:top;border-top:0;border-right:0;border-bottom:0;border-left:0"">
                                                                        <table class=""image_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                                                            <tr>
                                                                                <td class=""pad"" style=""padding-bottom:30px;padding-top:25px;width:100%;padding-right:0;padding-left:0"">
                                                                                    <div class=""alignment"" align=""center"" style=""line-height:10px"">
                                                                                        <div style=""max-width:158px"">
                                                                                            <img src=""https://d15k2d11r6t6rl.cloudfront.net/pub/r388/l239mmxz/ia0/otr/bc8/AIS-logo.png"" style=""display:block;height:auto;border:0;width:100%"" width=""158"" alt=""Alternate text"" title=""Alternate text"" height=""auto"">
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""text_block block-2"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;word-break:break-word"">
                                                                            <tr>
                                                                                <td class=""pad"">
                                                                                    <div style=""font-family:sans-serif"">
                                                                                        <div class style=""font-size:12px;font-family:Tahoma,Verdana,Segoe,sans-serif;mso-line-height-alt:14.399999999999999px;color:#000;line-height:1.2"">
                                                                                            <p style=""margin:0;font-size:14px;text-align:center;mso-line-height-alt:16.8px"">
                                                                                                <span style=""word-break: break-word; font-size: 26px;"">{title}!</span>
                                                                                            </p>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""text_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0;word-break:break-word"">
                                                                            <tr>
                                                                                <td class=""pad"" style=""padding-bottom:25px;padding-left:10px;padding-right:10px;padding-top:20px"">
                                                                                    <div style=""font-family:sans-serif"">
                                                                                        <div class style=""font-size:12px;font-family:Tahoma,Verdana,Segoe,sans-serif;mso-line-height-alt:14.399999999999999px;color:#8c8c8c;line-height:1.2"">
                                                                                            <p style=""margin:0;font-size:14px;text-align:center;mso-line-height-alt:16.8px"">
                                                                                                <span style=""word-break: break-word; font-size: 17px;"">Click the button below to<strong> {subtitle}</strong>:</span>
                                                                                            </p>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                        <table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace:0;mso-table-rspace:0"">
                                                                            <tr>
                                                                                <td class=""pad"">
                                                                                    <div class=""alignment"" align=""center"">
                                                                                        <div style=""background-color:#005a9c;border-bottom:0 solid transparent;border-left:0 solid transparent;border-radius:50px;border-right:0 solid transparent;border-top:0 solid transparent;color:#fff;display:block;font-family:Tahoma,Verdana,Segoe,sans-serif;font-size:18px;font-weight:700;mso-border-alt:none;padding-bottom:10px;padding-top:10px;text-align:center;text-decoration:none;width:35%;word-break:keep-all"">
                                                                                            <span style=""word-break: break-word; padding-left: 20px; padding-right: 20px; font-size: 18px; display: inline-block; letter-spacing: normal;""><span style=""word-break: break-word; line-height: 36px;""><strong><a href=""{tokenLink}"" style=""color:#fff;text-decoration:none;"">{button}</a></strong></span></span>
                                                                                        </div>
                                                                                    </div>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </body>";
        }

        public Response SendEmail(string to, string subject, string body)
        {
            var nameFrom = _configuration["Mail:NameFrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body,
            };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }

            return new Response
            {
                IsSuccess = true,
            };
        }
    }
}
