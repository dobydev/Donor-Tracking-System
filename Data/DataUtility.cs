using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DonorTrackingSystem.Models;

namespace DonorTrackingSystem.Data
{
    public static class DataUtility
    {
        public static async Task SeedDemoDataAsync(ApplicationDbContext context)
        {
            // Only seed if the tables are empty
            if (await context.Congregants.AnyAsync()) return;

            // Families
            var families = new List<Family>
            {
                new Family { FamilyName = "Johnson Family" },
                new Family { FamilyName = "Martinez Family" },
                new Family { FamilyName = "Thompson Family" },
                new Family { FamilyName = "Williams Family" },
            };
            context.Families.AddRange(families);
            await context.SaveChangesAsync();

            // Congregants (20)
            var congregants = new List<Congregant>
            {
                new Congregant { Name = "Robert Johnson",    EmailAddress = "rjohnson@email.com",   PhoneNumber = "555-101-0001", Address = "101 Maple St, Springfield, IL 62701",  BirthDate = new DateTime(1965, 3, 14),  JoinDate = new DateTime(2005, 6, 12),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[0].ID },
                new Congregant { Name = "Linda Johnson",     EmailAddress = "ljohnson@email.com",   PhoneNumber = "555-101-0002", Address = "101 Maple St, Springfield, IL 62701",  BirthDate = new DateTime(1968, 7, 22),  JoinDate = new DateTime(2005, 6, 12),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[0].ID },
                new Congregant { Name = "Carlos Martinez",   EmailAddress = "cmartinez@email.com",  PhoneNumber = "555-102-0001", Address = "202 Oak Ave, Decatur, IL 62521",        BirthDate = new DateTime(1972, 11, 5),  JoinDate = new DateTime(2010, 1, 8),   ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[1].ID },
                new Congregant { Name = "Maria Martinez",    EmailAddress = "mmartinez@email.com",  PhoneNumber = "555-102-0002", Address = "202 Oak Ave, Decatur, IL 62521",        BirthDate = new DateTime(1975, 4, 19),  JoinDate = new DateTime(2010, 1, 8),   ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[1].ID },
                new Congregant { Name = "James Thompson",    EmailAddress = "jthompson@email.com",  PhoneNumber = "555-103-0001", Address = "303 Pine Rd, Peoria, IL 61602",         BirthDate = new DateTime(1958, 9, 30),  JoinDate = new DateTime(1998, 3, 21),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[2].ID },
                new Congregant { Name = "Susan Thompson",    EmailAddress = "sthompson@email.com",  PhoneNumber = "555-103-0002", Address = "303 Pine Rd, Peoria, IL 61602",         BirthDate = new DateTime(1960, 1, 11),  JoinDate = new DateTime(1998, 3, 21),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[2].ID },
                new Congregant { Name = "Michael Williams",  EmailAddress = "mwilliams@email.com",  PhoneNumber = "555-104-0001", Address = "404 Elm Blvd, Rockford, IL 61101",      BirthDate = new DateTime(1980, 6, 3),   JoinDate = new DateTime(2015, 9, 14),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[3].ID },
                new Congregant { Name = "Ashley Williams",   EmailAddress = "awilliams@email.com",  PhoneNumber = "555-104-0002", Address = "404 Elm Blvd, Rockford, IL 61101",      BirthDate = new DateTime(1983, 2, 27),  JoinDate = new DateTime(2015, 9, 14),  ActiveStatus = ActiveStatus.CurrentMember,    FamilyID = families[3].ID },
                new Congregant { Name = "David Harris",      EmailAddress = "dharris@email.com",    PhoneNumber = "555-201-0001", Address = "505 Cedar Ln, Aurora, IL 60505",         BirthDate = new DateTime(1945, 12, 8),  JoinDate = new DateTime(1985, 4, 2),   ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Patricia Clark",    EmailAddress = "pclark@email.com",     PhoneNumber = "555-202-0001", Address = "606 Birch Way, Joliet, IL 60432",        BirthDate = new DateTime(1952, 8, 17),  JoinDate = new DateTime(1992, 7, 30),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Thomas Lewis",      EmailAddress = "tlewis@email.com",     PhoneNumber = "555-203-0001", Address = "707 Walnut Dr, Naperville, IL 60540",   BirthDate = new DateTime(1988, 5, 25),  JoinDate = new DateTime(2018, 11, 5),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Jennifer Walker",   EmailAddress = "jwalker@email.com",    PhoneNumber = "555-204-0001", Address = "808 Spruce Ct, Waukegan, IL 60085",     BirthDate = new DateTime(1991, 10, 14), JoinDate = new DateTime(2020, 2, 22),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Charles Hall",      EmailAddress = "chall@email.com",      PhoneNumber = "555-205-0001", Address = "909 Aspen St, Evanston, IL 60201",       BirthDate = new DateTime(1970, 3, 7),   JoinDate = new DateTime(2008, 5, 18),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Barbara Young",     EmailAddress = "byoung@email.com",     PhoneNumber = "555-206-0001", Address = "1010 Poplar Ave, Elgin, IL 60120",       BirthDate = new DateTime(1963, 7, 1),   JoinDate = new DateTime(2001, 8, 9),   ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Daniel King",       EmailAddress = "dking@email.com",      PhoneNumber = "555-207-0001", Address = "1111 Magnolia Blvd, Cicero, IL 60804",  BirthDate = new DateTime(1995, 1, 29),  JoinDate = new DateTime(2022, 3, 13),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Nancy Scott",       EmailAddress = "nscott@email.com",     PhoneNumber = "555-208-0001", Address = "1212 Willow Rd, Berwyn, IL 60402",       BirthDate = new DateTime(1977, 9, 18),  JoinDate = new DateTime(2013, 10, 27), ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Paul Green",        EmailAddress = "pgreen@email.com",     PhoneNumber = "555-209-0001", Address = "1313 Chestnut St, Oak Park, IL 60301",  BirthDate = new DateTime(1984, 4, 11),  JoinDate = new DateTime(2016, 6, 4),   ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "Betty Adams",       EmailAddress = "badams@email.com",     PhoneNumber = "555-210-0001", Address = "1414 Hickory Ln, Skokie, IL 60076",      BirthDate = new DateTime(1949, 11, 23), JoinDate = new DateTime(1988, 1, 15),  ActiveStatus = ActiveStatus.CurrentMember },
                new Congregant { Name = "George Baker",                                             PhoneNumber = "555-211-0001", Address = "1515 Locust Ave, Tinley Park, IL 60477", BirthDate = new DateTime(1955, 6, 6),   JoinDate = new DateTime(1995, 9, 3),   ActiveStatus = ActiveStatus.TransferredMembership },
                new Congregant { Name = "Dorothy Nelson",    EmailAddress = "dnelson@email.com",    PhoneNumber = "555-212-0001", Address = "1616 Sycamore Dr, Palatine, IL 60067",  BirthDate = new DateTime(1960, 2, 14),  JoinDate = new DateTime(1999, 4, 20),  ActiveStatus = ActiveStatus.LeftChurch },
            };
            context.Congregants.AddRange(congregants);
            await context.SaveChangesAsync();

            // Non-Congregants (30) 
            var nonCongregants = new List<NonCongregant>
            {
                new NonCongregant { FirstName = "Alan",    LastName = "Foster",     ContactDetails = "alan.foster@mail.com | 555-301-0001",         IsActive = true  },
                new NonCongregant { FirstName = "Brenda",  LastName = "Collins",    ContactDetails = "bcollins@mail.com | 555-301-0002",             IsActive = true  },
                new NonCongregant { FirstName = "Craig",   LastName = "Rivera",     ContactDetails = "crivera@mail.com | 555-301-0003",              IsActive = true  },
                new NonCongregant { FirstName = "Diana",   LastName = "Cooper",     ContactDetails = "dcooper@mail.com | 555-301-0004",              IsActive = true  },
                new NonCongregant { FirstName = "Edward",  LastName = "Reed",       ContactDetails = "ereed@mail.com | 555-301-0005",                IsActive = true  },
                new NonCongregant { FirstName = "Fiona",   LastName = "Bailey",     ContactDetails = "fbailey@mail.com | 555-301-0006",              IsActive = true  },
                new NonCongregant { FirstName = "George",  LastName = "Cox",        ContactDetails = "gcox@mail.com | 555-301-0007",                 IsActive = true  },
                new NonCongregant { FirstName = "Hannah",  LastName = "Howard",     ContactDetails = "hhoward@mail.com | 555-301-0008",              IsActive = true  },
                new NonCongregant { FirstName = "Ian",     LastName = "Ward",       ContactDetails = "iward@mail.com | 555-301-0009",                IsActive = true  },
                new NonCongregant { FirstName = "Julia",   LastName = "Torres",     ContactDetails = "jtorres@mail.com | 555-301-0010",              IsActive = true  },
                new NonCongregant { FirstName = "Kevin",   LastName = "Peterson",   ContactDetails = "kpeterson@mail.com | 555-302-0001",            IsActive = true  },
                new NonCongregant { FirstName = "Laura",   LastName = "Gray",       ContactDetails = "lgray@mail.com | 555-302-0002",                IsActive = true  },
                new NonCongregant { FirstName = "Mark",    LastName = "Ramirez",    ContactDetails = "mramirez@mail.com | 555-302-0003",             IsActive = true  },
                new NonCongregant { FirstName = "Nina",    LastName = "James",      ContactDetails = "njames@mail.com | 555-302-0004",               IsActive = true  },
                new NonCongregant { FirstName = "Oscar",   LastName = "Watson",     ContactDetails = "owatson@mail.com | 555-302-0005",              IsActive = true  },
                new NonCongregant { FirstName = "Paula",   LastName = "Brooks",     ContactDetails = "pbrooks@mail.com | 555-302-0006",              IsActive = true  },
                new NonCongregant { FirstName = "Quinn",   LastName = "Kelly",      ContactDetails = "qkelly@mail.com | 555-302-0007",               IsActive = true  },
                new NonCongregant { FirstName = "Rachel",  LastName = "Sanders",    ContactDetails = "rsanders@mail.com | 555-302-0008",             IsActive = true  },
                new NonCongregant { FirstName = "Samuel",  LastName = "Price",      ContactDetails = "sprice@mail.com | 555-302-0009",               IsActive = true  },
                new NonCongregant { FirstName = "Tina",    LastName = "Bennett",    ContactDetails = "tbennett@mail.com | 555-302-0010",             IsActive = true  },
                new NonCongregant { FirstName = "Ulrich",  LastName = "Wood",       ContactDetails = "uwood@mail.com | 555-303-0001",                IsActive = true  },
                new NonCongregant { FirstName = "Vera",    LastName = "Barnes",     ContactDetails = "vbarnes@mail.com | 555-303-0002",              IsActive = true  },
                new NonCongregant { FirstName = "Walter",  LastName = "Ross",       ContactDetails = "wross@mail.com | 555-303-0003",                IsActive = false },
                new NonCongregant { FirstName = "Xena",    LastName = "Henderson",  ContactDetails = "xhenderson@mail.com | 555-303-0004",           IsActive = true  },
                new NonCongregant { FirstName = "Yusuf",   LastName = "Coleman",    ContactDetails = "ycoleman@mail.com | 555-303-0005",             IsActive = true  },
                new NonCongregant { FirstName = "Zoe",     LastName = "Jenkins",    ContactDetails = "zjenkins@mail.com | 555-303-0006",             IsActive = true  },
                new NonCongregant {                        CompanyOrganization = "Greenfield Community Trust",  ContactDetails = "info@greenfieldtrust.org | 555-400-0001", IsActive = true  },
                new NonCongregant {                        CompanyOrganization = "Sunrise Charity Foundation",  ContactDetails = "give@sunrisefoundation.org | 555-400-0002", IsActive = true  },
                new NonCongregant {                        CompanyOrganization = "Harvest Hope Outreach",       ContactDetails = "outreach@harvesthope.org | 555-400-0003",  IsActive = true  },
                new NonCongregant {                        CompanyOrganization = "Blue Ridge Giving Circle",    ContactDetails = "circle@blueridge.org | 555-400-0004",      IsActive = false },
            };
            context.NonCongregants.AddRange(nonCongregants);
            await context.SaveChangesAsync();

            // Donations
            // Fund IDs: 1=General, 2=Building, 3=Missions, 4=Benevolence
            var donations = new List<Donation>();
            var today = DateTime.Today;
            var currentYear = today.Year;
            var priorYear = currentYear - 1;

            // Congregant donations – spread across current and prior year
            var congregantDonationData = new (int idx, decimal[] amounts, int[] months, int[] years)[]
            {
                (0,  new[]{ 200m,150m,300m,250m,175m,400m }, new[]{ 1,3,5,7,9,11 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (1,  new[]{ 100m,125m,150m,200m,100m,125m }, new[]{ 2,4,6,8,10,12 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (2,  new[]{ 500m,500m,250m,500m,500m,300m }, new[]{ 1,4,7,10,1,7  }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
                (3,  new[]{ 75m,75m,75m,75m,75m,75m },       new[]{ 1,2,3,4,5,6  }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (4,  new[]{ 1000m,1000m,500m,1000m,1000m,750m }, new[]{ 3,6,9,3,6,9 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (5,  new[]{ 200m,200m,200m,200m,200m,200m }, new[]{ 1,3,5,7,9,11 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (6,  new[]{ 50m,100m,50m,75m,100m,50m },     new[]{ 2,5,8,11,2,5 }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
                (7,  new[]{ 300m,300m,150m,300m,300m,200m }, new[]{ 1,4,7,10,1,7 }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
                (8,  new[]{ 250m,250m,125m,250m,250m,175m }, new[]{ 2,5,8,11,2,8 }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
                (9,  new[]{ 150m,150m,150m,150m,150m,150m }, new[]{ 1,2,3,4,5,6  }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (10, new[]{ 400m,200m,400m,400m,200m,300m }, new[]{ 3,6,9,3,6,9  }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (11, new[]{ 60m,60m,60m,60m,60m,60m },       new[]{ 1,3,5,7,9,11 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (12, new[]{ 175m,175m,175m,175m,175m,175m }, new[]{ 2,4,6,8,10,12 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (13, new[]{ 500m,250m,500m,500m,250m,400m }, new[]{ 1,4,7,10,1,7  }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
                (14, new[]{ 80m,80m,80m,80m,80m,80m },       new[]{ 2,4,6,8,10,12 }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (15, new[]{ 350m,350m,175m,350m,350m,275m }, new[]{ 3,6,9,3,6,9  }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (16, new[]{ 125m,125m,125m,125m,125m,125m }, new[]{ 1,2,3,4,5,6  }, new[]{ currentYear,currentYear,currentYear,priorYear,priorYear,priorYear }),
                (17, new[]{ 600m,300m,600m,600m,300m,450m }, new[]{ 2,5,8,11,2,8 }, new[]{ currentYear,currentYear,currentYear,currentYear,priorYear,priorYear }),
            };

            int[] fundCycle = { 1, 2, 3, 4, 1, 2 };

            // For congregants, we will assign DonorIDs starting from 1000 + idx to ensure they are distinct from non-congregant DonorIDs
            foreach (var (idx, amounts, months, years) in congregantDonationData)
            {
                for (int i = 0; i < amounts.Length; i++)
                {
                    donations.Add(new Donation
                    {
                        CongregantID    = congregants[idx].ID,
                        DonorID         = 1000 + idx,
                        DonationAmount  = amounts[i],
                        DonationDate    = new DateTime(years[i], months[i], 15),
                        FundDesignationID = fundCycle[i],
                        StaffMemberID   = 3000,
                        Created         = DateTimeOffset.UtcNow
                    });
                }
            }

            // Non-congregant donations – spread across current and prior year
            var nonCongregantDonationData = new (int idx, decimal amount, int month, int year)[]
            {
                (0,  200m,  2,  currentYear),
                (0,  150m,  8,  priorYear),
                (1,  500m,  1,  currentYear),
                (1,  500m,  6,  priorYear),
                (2,  75m,   3,  currentYear),
                (3,  1200m, 4,  currentYear),
                (3,  1000m, 10, priorYear),
                (4,  300m,  5,  currentYear),
                (4,  275m,  11, priorYear),
                (5,  100m,  6,  currentYear),
                (6,  450m,  7,  currentYear),
                (6,  400m,  2,  priorYear),
                (7,  250m,  8,  currentYear),
                (8,  600m,  9,  currentYear),
                (8,  550m,  4,  priorYear),
                (9,  175m,  10, currentYear),
                (10, 800m,  11, currentYear),
                (10, 750m,  5,  priorYear),
                (11, 125m,  12, priorYear),
                (12, 350m,  1,  currentYear),
                (13, 900m,  2,  currentYear),
                (13, 850m,  7,  priorYear),
                (14, 50m,   3,  currentYear),
                (15, 2000m, 4,  currentYear),
                (15, 1800m, 9,  priorYear),
                (26, 5000m, 1,  currentYear),
                (26, 4500m, 6,  priorYear),
                (27, 3000m, 3,  currentYear),
                (27, 2800m, 9,  priorYear),
                (28, 1500m, 5,  currentYear),
            };

            // For non-congregants, we will assign DonorIDs starting from 2000 + idx to avoid overlap with congregant DonorIDs
            foreach (var (idx, amount, month, year) in nonCongregantDonationData)
            {
                donations.Add(new Donation
                {
                    NonCongregantID   = nonCongregants[idx].ID,
                    DonorID           = 2000 + idx,
                    DonationAmount    = amount,
                    DonationDate      = new DateTime(year, month, 20),
                    FundDesignationID = ((idx % 4) + 1),
                    StaffMemberID     = 3000,
                    Created           = DateTimeOffset.UtcNow
                });
            }

            context.Donations.AddRange(donations);
            await context.SaveChangesAsync();
        }

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Define roles
            string[] roles = { "Administrator", "Office Manager", "Support Staff" };

            // Create roles if they do not exist
            foreach (var role in roles)
            {                               
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed default users for each role

            // Administrator: ID 1000, Password 123456
            await CreateUserIfNotExists(userManager, "1000", "123456", "Administrator");

            // Office Manager: ID 2000, Password 654321
            await CreateUserIfNotExists(userManager, "2000", "654321", "Office Manager");

            // Support Staff: ID 3000, Password 111111
            await CreateUserIfNotExists(userManager, "3000", "111111", "Support Staff");
        }

        // Helper method to create a user if it does not exist
        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string ID, string password, string role)
        {
            // Check if the user already exists
            if (await userManager.FindByNameAsync(ID) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = ID
                };

                // Create the user with the specified password
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
