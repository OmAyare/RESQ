using SQLite;
using RESQ.Models;

namespace RESQ.Data
{
    public class LocalDatabase
    {
        private readonly SQLiteAsyncConnection _db;

        public LocalDatabase(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Customer>().Wait();
            _db.CreateTableAsync<MedicalInfo>().Wait();
            _db.CreateTableAsync<EmergencyContact>().Wait();
            _db.CreateTableAsync<EmergencyEvent>().Wait();

        }

        public async Task<EmergencyContact?> GetContactByNumberAsync(string number)
        {
            return await _db.Table<EmergencyContact>()
                            .Where(c => c.PhoneNumber == number)
                            .FirstOrDefaultAsync();
        }

        public async Task SaveEmergencyContactAsync(EmergencyContact contact)
        {
            await _db.InsertAsync(contact);
        }

        public async Task DeleteEmergencyContactAsync(EmergencyContact contact)
        {
            await _db.DeleteAsync(contact);
        }

        public Task<List<EmergencyContact>> GetAllEmergencyContactsAsync()
        {
            return _db.Table<EmergencyContact>().ToListAsync();
        }

        //public async Task ResetCustomerTableAsync()
        //{
        //    await _db.DropTableAsync<Customer>();
        //    await _db.CreateTableAsync<Customer>();
        //}

        /************************************Saving or updating the Customer **************************************************/
        public async Task SaveCustomerAsync(Customer customer)
        {
            var existing = await _db.Table<Customer>().FirstOrDefaultAsync();
            if (existing != null)
            {
                customer.Cust_Id = existing.Cust_Id;
                customer.Username = existing.Username;   
                customer.Password = existing.Password; 
                customer.UserID = existing.UserID;
                await _db.UpdateAsync(customer);
            }
            else
            {
                await _db.InsertAsync(customer);
            }
        }

        public async Task<Customer> GetCustomerAsync()
        {
            return await _db.Table<Customer>().FirstOrDefaultAsync();
        }

        /************************************Saving or updating the MedicalInfo **************************************************/
        public async Task SaveMedicalInfoAsync(MedicalInfo medicalInfo)
        {
            var existing = await _db.Table<MedicalInfo>().FirstOrDefaultAsync();
            if (existing != null)
            {
                medicalInfo.MedID = existing.MedID;
                await _db.UpdateAsync(medicalInfo);
            }
            else
            {
                await _db.InsertAsync(medicalInfo);
            }
        }

        public async Task<MedicalInfo> GetMedicalInfoAsync()
        {
            return await _db.Table<MedicalInfo>().FirstOrDefaultAsync();
        }
        /***************************************cheking the Login credentials ***********************************************/

        // LocalDatabase.cs
        public async Task<Customer?> ValidateLoginAsync(string username, string password)
        {
            return await _db.Table<Customer>()
                            .Where(c => c.Username == username && c.Password == password)
                            .FirstOrDefaultAsync();
        }

        /************************************** ***********************************************/

        public Task<List<Customer>> GetCustomersAsync()
        {
            return _db.Table<Customer>().ToListAsync();
        }

        /************************************** Get one by username ***********************************************/
        public Task<Customer> GetCustomerByUsernameAsync(string username)
        {
            return _db.Table<Customer>()
                            .Where(c => c.Username == username)
                            .FirstOrDefaultAsync();
        }

        /**************************************Delete***********************************************/
        public Task<int> DeleteCustomerAsync(Customer customer)
        {
            return _db.DeleteAsync(customer);
        }
        /************************************ Saving EmergencyEvent **************************************************/
        public async Task SaveEmergencyEventAsync(EmergencyEvent ev)
        {
            await _db.InsertAsync(ev);
        }

        public async Task UpdateEmergencyEventAsync(EmergencyEvent ev)
        {
            await _db.UpdateAsync(ev);
        }

        public async Task<List<EmergencyEvent>> GetEmergencyEventsAsync()
        {
            return await _db.Table<EmergencyEvent>().ToListAsync();
        }

        public async Task<EmergencyEvent?> GetEmergencyEventByIdAsync(int id)
        {
            return await _db.Table<EmergencyEvent>()
                            .Where(e => e.EventID == id)
                            .FirstOrDefaultAsync();
        }

        public async Task DeleteEmergencyEventAsync(EmergencyEvent ev)
        {
            await _db.DeleteAsync(ev);
        }


    }

}
