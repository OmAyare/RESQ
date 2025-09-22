using Microsoft.EntityFrameworkCore;
using RESQ_API.Domain.Entities;

namespace RESQ_API.Data
{
    public class RESQ_DbContext : DbContext
    {
        public RESQ_DbContext(DbContextOptions dbContextOptions) :base(dbContextOptions) 
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmergencyEvent> EmergencyEvents { get; set; }
    }
}
