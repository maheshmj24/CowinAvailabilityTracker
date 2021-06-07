using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CowinAvailabilityTracker
{
    internal class Program
    {
        private static readonly HttpClient Client = new HttpClient();

        private static async Task Main(string[] args)
        {
            if (args.Length == 1 && HelpRequired(args[0]))
            {
                DisplayHelp();
            }
            else
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("Insufficient arguments...");
                    DisplayHelp();
                    Environment.Exit(0);
                }

                Console.WriteLine("Checking available slots...");

                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // District ID comes from the command line argument
                TrackerResponse trackerResponse = await CalendarByDistrict(args[0]);

                if (trackerResponse.AvailableCount != 0)
                {
                    string mailContent = $"Available slots: {trackerResponse.AvailableCount} | Available slots for Age < 45: {trackerResponse.AvailableCountCountLt45} <br> <br> {Environment.NewLine}";
                    foreach (AvailableSession session in trackerResponse.AvailableSessions)
                    {
                        mailContent += $"Name: {session.Name} | Available capacity: {session.Available_capacity} | Min age limit: {session.Min_age_limit} <br> {Environment.NewLine}";
                    }
                    Console.WriteLine(mailContent);

                    await NotifyUser(args[1], args[2], args[3], mailContent);
                }
                else
                {
                    Console.WriteLine("No available slots.");
                }
            }
        }

        private static async Task<TrackerResponse> CalendarByDistrict(string districtID)
        {
            // Create availableSessions List which will hold the results
            List<AvailableSession> availableSessions = new List<AvailableSession>();
            int availableCount = 0;
            int availableLt45Count = 0;

            //Call the calendarByDistrict api
            var streamTask = Client.GetStreamAsync(
                "https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?" +
                "district_id=" + districtID +
                "&date=" + DateTime.Now.ToString("dd-MM-yyyy"));

            var availableCenters = await JsonSerializer.DeserializeAsync<CalendarByDistrictResponse>(await streamTask);

            foreach (var center in availableCenters.Centers)
            {
                foreach (var session in center.Sessions)
                {
                    if (session.Available_capacity != 0)
                    {
                        availableCount++;
                        if (session.Min_age_limit < 45)
                        {
                            availableLt45Count++;
                        }

                        availableSessions.Add(new AvailableSession
                        {
                            Name = center.Name,
                            Available_capacity = session.Available_capacity,
                            Min_age_limit = session.Min_age_limit,
                        });
                    }
                }
            }

            TrackerResponse result = new TrackerResponse()
            {
                AvailableCount = availableCount,
                AvailableCountCountLt45 = availableLt45Count,
                AvailableSessions = availableSessions
            };

            return result;
        }

        private static async Task NotifyUser(string apiKey, string fromEmail, string toEmail, string emailBody)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail);
            var subject = "Vaccination slots available";
            var to = new EmailAddress(toEmail);
            var plainTextContent = emailBody;
            var htmlContent = emailBody;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email notification sent.");
            }
            else
            {
                Console.WriteLine("Email notification failed.");
            }
        }

        private static bool HelpRequired(string param)
        {
            return param == "-h" || param == "--help" || param == "/?";
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\nCowinAvailabilityTracker\n");
            Console.WriteLine("Usage: CowinAvailabilityTracker.exe <DISTRICT ID> <SENDGRID API KEY> <FROM EMAIL> <TO EMAIL>");
        }
    }
}
