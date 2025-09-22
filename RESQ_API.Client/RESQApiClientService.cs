using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RESQ_API.Client.Models;
using RESQ_API.Client.Models.RESQApi_Models;


namespace RESQ_API.Client
{
    public class RESQApiClientService
    {
        private readonly HttpClient _httpClient;

        public RESQApiClientService(RESQApiClientOptions resqapialientoptions)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri(resqapialientoptions.ApiBaseAddress);
        }

        /*************************************************-------- Users --------**********************************************************/
        public async Task<List<User>?> GetUsersAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<User>>("/api/users");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>($"/api/users/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<User?> RegisterUserAsync(User newUser)
        {
            try
            {
                newUser.EmergencyEvents = null;

                var response = await _httpClient.PostAsJsonAsync("api/users/register", newUser);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<User?> UpdateUserAsync(int id, User updatedUser)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/users/{id}", updatedUser);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        /********************************************** -------- Emergency Events --------******************************************************/
        public async Task<List<EmergencyEvent>?> GetEmergencyEventsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EmergencyEvent>>("/api/emergencyevents");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<EmergencyEvent?> GetEmergencyEventByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<EmergencyEvent>($"/api/emergencyevents/{id}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<EmergencyEvent?> CreateEmergencyEventAsync(EmergencyEvent newEvent)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/emergencyevents", newEvent);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<EmergencyEvent>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
        public async Task<EmergencyEvent?> UpdateEmergencyEventAsync(int id, EmergencyEvent updatedEvent)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/emergencyevents/{id}", updatedEvent);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<EmergencyEvent>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ API Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"➡ Inner: {ex.InnerException.Message}");
                throw;
            }
        }
    }
}
