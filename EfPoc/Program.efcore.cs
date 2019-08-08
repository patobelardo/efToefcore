﻿using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EfPoc.Models;
using EfPoc.Infra;
using EfPoc.Entities;

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


        private static async Task EFCoreDeleteAsync(DatabaseAccessRequest databaseAccessRequest)
        {
            using (var context = new EfPocEntitiesEFCore(databaseAccessRequest))
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