using System.Data.Entity;

namespace WordReview.Model
{
    public class DataContext : DbContext
    {
        public DataContext()
        : base("name=DefaultConnection")
        {
            //Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));
            //Database.CreateIfNotExists();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Migrations.Configuration>(true));
        }

        public DbSet<Answers> EquipmentList { get; set; }
        public DbSet<ResponseCode> ResponseCodes { get; set; }
    }
}
