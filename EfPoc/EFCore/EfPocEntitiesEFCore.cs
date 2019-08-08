
namespace EfPoc.Entities
{
    using EfPoc.Infra;
    using EfPoc.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Data.Entity.Infrastructure;

    public partial class EfPocEntitiesEFCore : AtlasDbContextEFCore<EfPocEntitiesEFCore>
    {
        //Venkat: Custom change include deriving from AtlasDbContext instead from DbContext and having overload constructor which takes DatabaseAccessRequest
        public EfPocEntitiesEFCore(DatabaseAccessRequest request)
            : base(request)
        {
            //Disable initializer
            //Database.SetInitializer<EfPocEntities>(null);
        }

        //public EfPocEntitiesEFCore()
        //    : base("name=EfPocEntities")
        //{
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    throw new UnintentionalCodeFirstException();
        //}

        public virtual DbSet<Members> Members { get; set; }
        public virtual DbSet<Spans> Spans { get; set; }
    }
}
