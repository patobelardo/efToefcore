using System;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using EfPoc.Entities;
using EfPoc.Infra;
using EfPoc.Models;

namespace EfPoc
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Performing CRUD");
            Console.WriteLine();

            var databaseAccessRequest = new DatabaseAccessRequest
            {
                WorkStationId = "venkat",
                DatabaseIdentifier = new DatabaseIdentifier
                {
                    TenantId = 3,
                    ConnectionStringId = 9,
                    UserName = "venkat",
                    DatabaseIdentifierType = DatabaseIdentifierType.Internal
                }
            };


            await CreateAsync(databaseAccessRequest);
            await GetAsync(databaseAccessRequest);
            await UpdateAsync(databaseAccessRequest);
            await DeleteAsync(databaseAccessRequest);

            Console.WriteLine();
            Console.WriteLine("Done Performing CRUD");
            Console.WriteLine();
            Console.WriteLine("==================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Performing CRUD using EF Core");
            Console.WriteLine();

            await EFCoreCreateAsync(databaseAccessRequest);
            await EFCoreGetAsync(databaseAccessRequest);
            await EFCoreUpdateAsync(databaseAccessRequest);
            await EFCoreDeleteAsync(databaseAccessRequest);
            await EFCoreBulkInsertAsync(databaseAccessRequest);
            Console.WriteLine();
            Console.WriteLine("Done Performing CRUD using EF Core");
            Console.ResetColor();
            Console.ReadLine();

        }

        private static async Task CreateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntities(databaseAccessRequest))
            {
                using (var transaction = context.Database.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        var member = new Member
                        {
                            FirstName = "FirstName",
                            LastName = "LastName",
                            HIC = "HIC0001",
                            PlanID = "Plan",
                            PBP = "PBP",
                            SegmentID = "SEG",
                            CurrentEffDate = DateTime.Now
                        };
                        context.Members.Add(member);
                        await context.SaveChangesAsync();

                        var span = new Span
                        {
                            MemberId = member.Id,
                            SpanType = "SECD",
                            SpanValue = "123",
                            StartDate = DateTime.Now
                        };
                        context.Spans.Add(span);
                        await context.SaveChangesAsync();

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Rollback();
                    }
                }
            }
        }


        private static async Task GetAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntities(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"{firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
            }
        }


        private static async Task UpdateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntities(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"Before Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
                firstMember.FirstName = firstMember.FirstName + "_Updated";
                await context.SaveChangesAsync();

                firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"After Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
            }
        }

        private static async Task DeleteAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntities(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                context.Members.Remove(firstMember);

                var span = await context.Spans.FirstOrDefaultAsync(m => m.MemberId == firstMember.Id);
                context.Spans.Remove(span);
                await context.SaveChangesAsync();
            }
        }
    }
}
