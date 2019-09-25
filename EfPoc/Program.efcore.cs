using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EfPoc.Models;
using EfPoc.Infra;
using EfPoc.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;
using System.Reflection;

namespace EfPoc
{
    partial class Program
    {

        private static async Task EFCoreCreateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                using (var transaction = context.Database.BeginTransaction(IsolationLevel.RepeatableRead)) 
                {
                    try
                    {
                        var member = new Members
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

                        var span = new Models.Spans
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


        private static async Task EFCoreGetAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"{firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
            }
        }


        private static async Task EFCoreUpdateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"Before Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
                firstMember.FirstName = firstMember.FirstName + "_Updated";
                await context.SaveChangesAsync();

                firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"After Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
            }
        }

        private static async Task EFCoreDTOUpdateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Update using a simple DTO and Automapper");

            var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<MemberDTO, Members>()
                        .ForMember(x => x.Id, opt => opt.Ignore())
                        .ForAllMembers(opt => opt.Condition(src => src != null));
                    cfg.CreateMap<SpanDTO, Spans>()
                        .ForAllMembers(opt => opt.Condition(src => src != null));
                }) ;
                
            var mapper = config.CreateMapper();

            SpanDTO newSpan = new SpanDTO();
            newSpan.SpanValue = "item1";
            newSpan.SpanType = "";

            MemberDTO dto = new MemberDTO
            {
                FirstName = "TestUser from DTO", 
                Spans = new List<SpanDTO>
                {
                    newSpan
                }
            };

            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"Before Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");

                mapper.Map(dto, firstMember, typeof(MemberDTO), typeof(Members));

                firstMember.FirstName = firstMember.FirstName + "_Updated";
                await context.SaveChangesAsync();

                firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                Console.WriteLine($"After Update: {firstMember.FirstName} {firstMember.LastName} {firstMember.HIC}");
            }
        }


        private static async Task EFCoreDeleteAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                var firstMember = await context.Members.FirstOrDefaultAsync(m => m.HIC == "HIC0001");
                context.Members.Remove(firstMember);

                //var span = await context.Spans.FirstOrDefaultAsync(m => m.MemberId == firstMember.Id);
                var spanList = context.Spans.Where(m => m.MemberId == firstMember.Id);
                context.Spans.RemoveRange(spanList);
                //context.Spans.Remove(span);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EFCoreBulkInsertAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            await EFCoreBulkInsertTraditionalAsync(databaseAccessRequest);
            await EFCoreBulkInsertExtensionMethodAsync(databaseAccessRequest);
        }

        private static async Task EFCoreBulkUpdateAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            await EFCoreBulkUpdateExtensionMethodAsync(databaseAccessRequest);
            await EFCoreBulkUpdateExtensionMethodChilsAsync(databaseAccessRequest);
            await EFCoreBulkUpdateExtensionMethodUpdateChilsAsync(databaseAccessRequest);
        }
        private static async Task EFCoreChangeTrackerAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                Console.WriteLine("Change Tracker");

                var member = new Members
                            {
                                FirstName = "FirstName",
                                LastName = "LastName",
                                HIC = "HIC0001",
                                PlanID = "Plan",
                                PBP = "PBP",
                                SegmentID = "SEG",
                                CurrentEffDate = DateTime.Now
                            };

                var span = new Models.Spans
                            {
                                MemberId = member.Id,
                                SpanType = "SECD",
                                SpanValue = "111",
                                StartDate = DateTime.Now
                            };
                var newspan = new Models.Spans
                            {
                                MemberId = member.Id,
                                SpanType = "SECD",
                                SpanValue = "222",
                                StartDate = DateTime.Now
                            };

                member.Spans.Add(span);
                member.Spans.Add(newspan);

                context.Members.Add(member);

                var member1 = new Members
                            {
                                FirstName = "FirstName1",
                                LastName = "LastName1",
                                HIC = "HIC0001",
                                PlanID = "Plan",
                                PBP = "PBP",
                                SegmentID = "SEG",
                                CurrentEffDate = DateTime.Now
                            };

                var span1 = new Models.Spans
                            {
                                MemberId = member1.Id,
                                SpanType = "SECD",
                                SpanValue = "123",
                                StartDate = DateTime.Now
                            };

                member1.Spans.Add(span1);

                context.Members.Add(member1);

                await SaveChangesExtendedAsync(context.ChangeTracker.Entries(), context);
            }
        }

        private static async Task SaveChangesExtendedAsync(IEnumerable<EntityEntry> entries, EfPocEntitiesEFCore context)
        {
            List<Operation> ops = new List<Operation>();

            foreach (var item in entries)
            {
                Console.WriteLine($"Entity: {item.Entity.GetType().Name}. State: {item.State}");

                //Adding a count per operation to decide when yo use bulk operations or the default savechanges method.
                _addOperation(ref ops, item);

                //foreach (var entry in item.Properties)
                //{
                //    Console.WriteLine("--- " +
                //                $"Property '{entry.Metadata.Name}'" +
                //                $" is {(entry.IsModified ? "modified" : "not modified")} " +
                //                $"Current value: '{entry.CurrentValue}' " +
                //                $"Original value: '{entry.OriginalValue}'");
                //}
            }

            foreach(var op in ops)
            {
                Console.WriteLine("========================================================================");
                Console.WriteLine($"Bulk Operation. Entity: {op.Type.Name}. State: {op.State}");
                Console.WriteLine("========================================================================");

                var items = entries.Where(e => e.Entity.GetType() == op.Type && e.State == op.State);


                Type listType = typeof(List<>).MakeGenericType(op.Type);
                IList entityList = (IList)Activator.CreateInstance(listType);
                    
                foreach (var newItem in items)
                {
                    entityList.Add(newItem.Entity);
                }

                var config = new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true };

                //For now I'm assuming we are always inserting
                switch (op.Type.Name)
                {
                    case "Members":
                        if (op.State == EntityState.Added)
                            await context.BulkInsertAsync((IList<Members>)entityList, config);
                        break; 
                    case "Spans":
                        if (op.State == EntityState.Added)
                            await context.BulkInsertAsync((IList<Spans>)entityList, config);
                        break;
                    default:
                        break;
                }

                List<ForeignKey> fkList = new List<ForeignKey>();

                foreach (var entry in items)
                {
                    //Get Foreign Keys (properties refecencing Ids of parents)
                    _fillfkList(ref fkList, entry);

                    foreach (var fk in fkList)
                    {
                        Console.WriteLine("Updating Foreign Keys");
                        //Getting just 1 collection of "Spans" (IGenericCollection<Spans>)
                        var childItems = entry.Entity.GetType()
                                                .GetProperties()
                                                .Where(p => p.PropertyType.GenericTypeArguments
                                                .Contains(fk.ForeignType))
                                                .First().GetValue(entry.Entity, null);

                        foreach (object child in (IEnumerable)childItems)
                        {
                            Console.WriteLine($"Updating child class: {child.GetType()}");
                            var parentIdValue = child.GetType().GetProperty(fk.ForeignPropertyName).GetValue(child);
                            Console.WriteLine($"Current Value of {fk.ForeignPropertyName}: {parentIdValue}");

                            var parentIdValueNew = entry.Entity.GetType().GetProperty(fk.CurrentPropertyName).GetValue(entry.Entity);
                            Console.WriteLine($"New Value: {parentIdValueNew}");

                            child.GetType().GetProperty(fk.ForeignPropertyName).SetValue(child, parentIdValueNew);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Keys from an entry, and detect Foreign Keys on other classes, to modify the referenced ID.
        /// </summary>
        /// <param name="listFks"></param>
        /// <param name="updated"></param>
        private static void _fillfkList(ref List<ForeignKey> listFks, EntityEntry updated)
        {
            if (listFks.Count == 0)
            {
                foreach (var entry in updated.Properties)
                {
                    if (entry.Metadata.IsKey())
                    {
                        foreach (var containingKeys in entry.Metadata.GetContainingKeys())
                        {
                            foreach (var reference in containingKeys.GetReferencingForeignKeys())
                            {
                                foreach (var p in reference.Properties)
                                {
                                    listFks.Add(
                                        new ForeignKey
                                        {
                                            CurrentPropertyName = entry.Metadata.Name,
                                            ForeignPropertyName = p.Name,
                                            ForeignType = p.DeclaringEntityType.ClrType
                                        });
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void _updateKey(PropertyEntry p)
        {
            foreach(var fk in p.Metadata.GetContainingForeignKeys())
            {

                throw new NotImplementedException();
            }
        }

        private static List<T> _createListOf<T>(T type)
        {
            Type listGenericType = typeof(List<>);
            Type list2 = listGenericType.MakeGenericType(typeof(T));
            ConstructorInfo ci = list2.GetConstructor(new Type[] { });
            List<T> list = (List<T>)ci.Invoke(new object[] { });


            ////var list = new List<T>();
            //foreach (var i in items)
            //{
            //    list.Add((T)i);
            //}
            return (List<T>)list;
        }
        private static void _addOperation(ref List<Operation> ops, EntityEntry item)
        {
            var op = ops.SingleOrDefault(x => x.Type == item.Entity.GetType() && x.State == item.State);
            if (op == null)
            {
                ops.Add(new Operation
                {
                    Type = item.Entity.GetType(),
                    State = item.State,
                    EntityCount = 1
                });
            }
            else
            {
                op.EntityCount++;
            }
        }

        private static async Task EFCoreBulkUpdateExtensionMethodAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                try
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    Console.WriteLine("Bulk Update: Update just 1 property LastName");
                    List<Members> memberList = new List<Members>();
                    for (var i = 1000; i < 2000; i++)
                    {
                        var member = new Members
                        {
                            Id = i,
                            LastName = $"UPDATED LastName"
                        };
                        memberList.Add(member);
                    }

                    //Columns to be included
                    await context.BulkUpdateAsync(memberList, options =>
                    {
                        options.PropertiesToInclude = new List<string> { "LastName" };
                    });

                    // An alternative can be executed like this:
                    //  context.Members.BatchUpdate(a => new Members { LastName = "Updated LastName" });

                    Console.WriteLine($"Finished in {w.ElapsedMilliseconds}ms");

                    Console.WriteLine("Getting a member with no changes");

                    var member12 = await context.Members.FirstOrDefaultAsync(m => m.Id == 12);
                    Console.WriteLine($"{member12.FirstName} | {member12.LastName} | {member12.HIC}");

                    Console.WriteLine("Getting an updated member");

                    var member1200 = await context.Members.FirstOrDefaultAsync(m => m.Id == 1200);
                    Console.WriteLine($"{member1200.FirstName} | {member1200.LastName} | {member1200.HIC}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Add Spans to members to see how it works
        /// </summary>
        /// <param name="databaseAccessRequest"></param>
        /// <returns></returns>
        private static async Task EFCoreBulkUpdateExtensionMethodChilsAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {

                try
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    Console.WriteLine("Bulk Update: Add Child items");

                    //Get all Members
                    var memberList = context.Members.Include(x => x.Spans);
                    List<Spans> spanList = new List<Spans>();

                    foreach(var member in memberList)
                    {
                        var span = new Spans
                        {
                            MemberId = member.Id,
                            SpanType = "SECD",
                            SpanValue = $"SPAN {member.Id}",
                            StartDate = DateTime.Now
                        };

                        //We need to remove this to avoid delays on savechanges
                        //member.Spans.Add(span);

                        spanList.Add(span);


                        if (member.Id == 1200)
                        {
                            member.LastName = "new " + member.LastName;
                        }
                    }

                    await context.SaveChangesAsync(); //For Members
                    await context.BulkInsertAsync(spanList);

                    Console.WriteLine($"Finished in {w.ElapsedMilliseconds}ms");

                    Console.WriteLine("Getting a member with the new span");
                    _showValues(await _getMember(context, 12));

                    Console.WriteLine("Getting an updated member AND new spans");
                    _showValues(await _getMember(context, 1200));


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Method who will add, update and remove one child
        /// </summary>
        /// <param name="databaseAccessRequest"></param>
        /// <returns></returns>
        private static async Task EFCoreBulkUpdateExtensionMethodUpdateChilsAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                //context.ChangeTracker.AutoDetectChangesEnabled = false;
                try
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    Console.WriteLine("Scenario: Add, Update, Remove Childs");

                    //Get all Members
                    var m1200 = await context.Members
                                        .Include(member => member.Spans)
                                        .FirstOrDefaultAsync(m => m.Id == 1200);

                    var m1210 = await context.Members
                                        .Include(member => member.Spans)
                                        .FirstOrDefaultAsync(m => m.Id == 1210);

                    var m1220 = await context.Members
                                        .Include(member => member.Spans)
                                        .FirstOrDefaultAsync(m => m.Id == 1220);

                    //Add new span
                    var span = new Spans
                    {
                        MemberId = m1200.Id,
                        SpanType = "SECD",
                        SpanValue = $"New {m1200.Id}",
                        StartDate = DateTime.Now
                    };
                    m1200.Spans.Add(span);


                    //Update span
                    foreach (var s in m1210.Spans)
                    {
                        s.SpanValue = "UPD Value";
                    }


                    //remove Spans
                    m1220.Spans.Clear();


                    await context.SaveChangesAsync();
                    ///For bulk operations (1000s), we should create different collections for each instruction (add or update and delete)

                    Console.WriteLine($"Finished in {w.ElapsedMilliseconds}ms");


                    Console.WriteLine("Getting member with new spans");
                    _showValues(await _getMember(context, 1200));

                    Console.WriteLine("Getting a member with updated spans");
                    _showValues(await _getMember(context, 1210));

                    Console.WriteLine("Getting a member with deleted spans");
                    _showValues(await _getMember(context, 1220));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task<Members> _getMember(EfPocEntitiesEFCore context, int memberId)
        {
            return await context.Members
                    .Include(member => member.Spans)
                    .FirstOrDefaultAsync(m => m.Id == memberId);
        }

        private static void _showValues(Members member)
        {
            Console.WriteLine($"{member.FirstName} | {member.LastName} | {member.HIC} | Span Count {member.Spans.Count}");
            foreach (var s in member.Spans)
            {
                Console.WriteLine($"-->Span Value: {s.SpanValue} Span ID: {s.Id} Member ID: {s.MemberId}");
            }
            Console.WriteLine("Enter to continue...");
            Console.ReadLine();
        }

        private static async Task EFCoreBulkInsertTraditionalAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                try
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    Console.WriteLine("Bulk Insert: Traditional Approach");
                    List<Members> memberList = new List<Members>();
                    for (var i = 0; i < 1000; i++)
                    {
                        var member = new Members
                        {
                            FirstName = $"FirstName {i}",
                            LastName = $"LastName {i}",
                            HIC = $"HIC{i.ToString("0000")}",
                            PlanID = "Plan",
                            PBP = "PBP",
                            SegmentID = "SEG",
                            CurrentEffDate = DateTime.Now
                        };
                        context.Members.Add(member);
                    }

                    await context.SaveChangesAsync();

                    Console.WriteLine($"Finished in {w.ElapsedMilliseconds}ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private static async Task EFCoreBulkInsertExtensionMethodAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                try
                {
                    Stopwatch w = new Stopwatch();
                    w.Start();
                    Console.WriteLine("Bulk Insert: BulkInsert Approach (Extension Method)");
                    List<Members> memberList = new List<Members>();
                    for (var i = 1000; i < 2000; i++)
                    {
                        var member = new Members
                        {
                            FirstName = $"FirstName {i}",
                            LastName = $"LastName {i}",
                            HIC = $"HIC{i.ToString("0000")}",
                            PlanID = "Plan",
                            PBP = "PBP",
                            SegmentID = "SEG",
                            CurrentEffDate = DateTime.Now
                        };
                        memberList.Add(member);
                    }
                    await context.BulkInsertAsync(memberList);

                    Console.WriteLine($"Finished in {w.ElapsedMilliseconds}ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private static async Task EFCoreBulkInsertCleanUpAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
            {
                await context.Database.ExecuteSqlCommandAsync("EXEC Recreate_Tables");

                string trigger1 = @"
                CREATE TRIGGER trg_members ON Members
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    INSERT INTO audits(
                        notes
                    )
                    SELECT
                        CONCAT('i ', i.LastName)
                    FROM
                        inserted i
                    UNION ALL
                    SELECT
                        CONCAT('i ', d.LastName)
                    FROM
                        deleted d;
                END

                ";
                await context.Database.ExecuteSqlCommandAsync(trigger1);
                string trigger2 = @"

                CREATE TRIGGER trg_spans ON Spans
                AFTER INSERT, UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    INSERT INTO audits(
                        notes
                    )
                    SELECT
                        CONCAT('is ', i.SpanValue)
                    FROM
                        inserted i
                    UNION ALL
                    SELECT
                        CONCAT('ds ', d.SpanValue)
                    FROM
                        deleted d;
                END

                ";
                await context.Database.ExecuteSqlCommandAsync(trigger2);
            }
        }
    }

    internal class Operation
    {
        public Type Type { get; set; }
        public EntityState State { get; set; }
        public int EntityCount { get; set; }
    }
    internal class ForeignKey
    {
        public Type ForeignType { get; set; }
        public string ForeignPropertyName { get; set; }
        public string CurrentPropertyName { get; set; }
    }
}

