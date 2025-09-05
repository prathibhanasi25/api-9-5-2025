using InsuranceClaimsAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
namespace InsuranceClaimsAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }

        public DbSet<Codes> Codes { get; set; }

        public DbSet<Zipcodes> Zipcodes { get; set; }

        public DbSet<Member> Member { get; set; }
        public DbSet<MemberInsurance> MemberInsurance { get; set; }

        public DbSet<MemberInsEligibility> MemberInsEligibility { get; set; }

        /*
               public DbSet<MemberLedger> MemberLedger { get; set; }

               public DbSet<MemberLedgerDetails> MemberLedgerDetails { get; set; }


               public DbSet<MemberScoreCard> MemberScoreCard { get; set; }

                   public DbSet<MemVault> MemVault { get; set; }

              */

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure tblLogin entity
            builder.Entity<User>()
                .HasIndex(u => u.UserID)
                .IsUnique();
           //Added by Reena
            builder.Entity<User>()
           .HasIndex(u => u.EmailID)
           .IsUnique();
            //Added by Reena
            builder.Entity<User>()
                .HasIndex(u => u.MobileNo)
                .IsUnique();

            // Configure tblMember entity
            builder.Entity<Member>()
                 .HasIndex(u => u.MemberID)
                .IsUnique();

            // PolicyType FK to Codes table
            builder.Entity<Member>()
            .HasOne(mi => mi.RelationshipType)
            .WithMany(c => c.RelationshipTypeUsages)
            .HasForeignKey(mi => mi.RelationshipTypeID)
            .HasPrincipalKey(c => c.CodeID) // assuming Code.Id is the PK
            .OnDelete(DeleteBehavior.Restrict);

            // Users-Role FK to Codes table
            builder.Entity<User>()
                .HasOne(mi => mi.Role)
                .WithMany(c => c.RoleUsages)
                .HasForeignKey(mi => mi.RoleId)
                .HasPrincipalKey(c => c.CodeID)
                .OnDelete(DeleteBehavior.Restrict);


            // Configure tblCodes entity
            builder.Entity<Codes>().HasData(
                new Codes { CodeID = 1, CodeType = "Relationship", CodeValue = "Self", IsActive = true },
                new Codes { CodeID = 2, CodeType = "Relationship", CodeValue = "Spouse", IsActive = true },
                new Codes { CodeID = 3, CodeType = "Relationship", CodeValue = "Son", IsActive = true },
                new Codes { CodeID = 4, CodeType = "Relationship", CodeValue = "Daughter", IsActive = true },
                new Codes { CodeID = 5, CodeType = "Relationship", CodeValue = "Mom", IsActive = true },
                new Codes { CodeID = 6, CodeType = "Relationship", CodeValue = "Dad", IsActive = true },
                new Codes { CodeID = 7, CodeType = "Relationship", CodeValue = "MotherInLaw", IsActive = true },
                new Codes { CodeID = 8, CodeType = "Relationship", CodeValue = "FatherInLaw", IsActive = true },

                new Codes { CodeID = 9, CodeType = "PolicyType", CodeValue = "Primary", IsActive = true },
                new Codes { CodeID = 10, CodeType = "PolicyType", CodeValue = "Secondary", IsActive = true },
                new Codes { CodeID = 11, CodeType = "PolicyType", CodeValue = "Teritiary", IsActive = true },
                new Codes { CodeID = 12, CodeType = "PolicyType", CodeValue = "Other", IsActive = true },

                new Codes { CodeID = 13, CodeType = "BenefitType", CodeValue = "Medical", IsActive = true },
                new Codes { CodeID = 14, CodeType = "BenefitType", CodeValue = "Dental", IsActive = true },
                new Codes { CodeID = 15, CodeType = "BenefitType", CodeValue = "Vision", IsActive = true },
                new Codes { CodeID = 16, CodeType = "BenefitType", CodeValue = "Other", IsActive = true },


                new Codes { CodeID = 17, CodeType = "Role", CodeValue = "Administrator", IsActive = true },
                new Codes { CodeID = 18, CodeType = "Role", CodeValue = "Manager", IsActive = true },
                new Codes { CodeID = 19, CodeType = "Role", CodeValue = "Editor", IsActive = true },
                new Codes { CodeID = 20, CodeType = "Role", CodeValue = "Viewer", IsActive = true },
                new Codes { CodeID = 21, CodeType = "Role", CodeValue = "User", IsActive = true },

                new Codes { CodeID = 22, CodeType = "PaymentMethod", CodeValue = "direct", IsActive = true },
                new Codes { CodeID = 23, CodeType = "PaymentMethod", CodeValue = "paycheck", IsActive = true },
                new Codes { CodeID = 24, CodeType = "PaymentMethod", CodeValue = "Other", IsActive = true },

                new Codes { CodeID = 25, CodeType = "EligibilityStatus", CodeValue = "Open", IsActive = true },
                new Codes { CodeID = 26, CodeType = "EligibilityStatus", CodeValue = "InProgress", IsActive = true },
                new Codes { CodeID = 27, CodeType = "EligibilityStatus", CodeValue = "Completed", IsActive = true },

                new Codes { CodeID = 28, CodeType = "VaultDocType", CodeValue = "EOB", IsActive = true },
                new Codes { CodeID = 29, CodeType = "VaultDocType", CodeValue = "Statement", IsActive = true },
                new Codes { CodeID = 30, CodeType = "VaultDocType", CodeValue = "Receipt", IsActive = true },

                 new Codes { CodeID = 31, CodeType = "Insurance-Coverange-Frequency", CodeValue = "Weekly", IsActive = true },
                new Codes { CodeID = 32, CodeType = "Insurance-Coverange-Frequency", CodeValue = "Monthly", IsActive = true }

                );

            // Configure tblZipcodes entity
            builder.Entity<Zipcodes>()
                .HasIndex(u => u.ZipIntID)
                .IsUnique();

            // Configure MemberInsurance entity
            builder.Entity<MemberInsurance>()
                .HasOne(d => d.Member)
                .WithMany(o => o.MemberInsurance)
                .HasForeignKey(d => d.MemberID)                
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MemberInsurance>()
             .HasOne(d => d.Subscriber)
             .WithMany(o => o.SubscriberMemberInsurance)
             .HasForeignKey(d => d.SubscriberID)
             .OnDelete(DeleteBehavior.Restrict);

            // PolicyType FK to Codes table
            builder.Entity<MemberInsurance>()
            .HasOne(mi => mi.PolicyType)
            .WithMany(c => c.PolicyTypeUsages)
            .HasForeignKey(mi => mi.PolicyTypeID)
            .HasPrincipalKey(c => c.CodeID) // assuming Code.Id is the PK
            .OnDelete(DeleteBehavior.Restrict);

            // BenefitTypeID FK to Codes table
            builder.Entity<MemberInsurance>()
                .HasOne(mi => mi.BenefitType)
                .WithMany(c => c.BenefitTypeUsages)
                .HasForeignKey(mi => mi.BenefitTypeID)
                .HasPrincipalKey(c => c.CodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MemberInsurance entity
            builder.Entity<MemberInsEligibility>()
                            .HasIndex(u => u.MemInsEliID)
                            .IsUnique();

            // BenefitTypeID FK to Codes table
            builder.Entity<Contributions>()
                .HasOne(mi => mi.BenefitType)
                .WithMany(c => c.Contribution_BenefitType_Usages)
                .HasForeignKey(mi => mi.BenefitTypeId)
                .HasPrincipalKey(c => c.CodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // FrequencyTypeID FK to Codes table
            builder.Entity<Contributions>()
                .HasOne(mi => mi.Frequency)
                .WithMany(c => c.Contributions_Frequency_Usages)
                .HasForeignKey(mi => mi.FrequencyId)
                .HasPrincipalKey(c => c.CodeID)
                .OnDelete(DeleteBehavior.Restrict);
            /*
                                    // Configure MemberLedger entity
                                    builder.Entity<MemberLedger>()
                                        .HasIndex(u => u.MemLedID)
                                        .IsUnique();


                                    // Configure MemberLedgerDetails entity
                                    builder.Entity<MemberLedgerDetails>()
                                        .HasIndex(u => u.MemLDID)
                                        .IsUnique();


                                    // Configure MemberScoreCard entity
                                    builder.Entity<MemberScoreCard>()
                                        .HasIndex(u => u.MemSCID)
                                        .IsUnique();

                                     // Configure tblMemVault entity
                                    builder.Entity<MemVault>()
                                        .HasIndex(u => u.MemVaultID)
                                        .IsUnique();
                        */

        }

    }
}
