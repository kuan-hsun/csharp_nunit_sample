using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using ConsoleApp_Nunit_practice;

// https://www.meziantou.net/testing-ef-core-in-memory-using-sqlite.htm



namespace UnitTestProject.Test
{
    // The model
    public class SampleDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public SampleDbContext() { }
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }





        // 方法1: Testing using In-Memory provider
        [Test]
        public async Task TestMethod_UsingInMemoryProvider()
        {
            // The database name allows the scope of the in-memory database
            // to be controlled independently of the context. The in-memory database is shared
            // anywhere the same name is used.
            var options = new DbContextOptionsBuilder<SampleDbContext>()
                .UseInMemoryDatabase(databaseName: "Test1") //要指定DB名稱
                .Options;

            using (var context = new SampleDbContext(options))
            {
                var user = new User() { Email = "test@sample.com" };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // New context with the data as the database name is the same
            using (var context = new SampleDbContext(options))
            {
                var count = await context.Users.CountAsync();
                Assert.AreEqual(1, count);

                var u = await context.Users.FirstOrDefaultAsync(user => user.Email == "test@sample.com");
                Assert.IsNotNull(u);
            }
        }



        // 方法2:  Testing using SQLite In Memory provider
        [Test]
        public async Task TestMethod_UsingSqliteInMemoryProvider()
        {
            using (var connection = new SqliteConnection("DataSource=:memory:"))
            {
                connection.Open();

                var options = new DbContextOptionsBuilder<SampleDbContext>()
                    .UseSqlite(connection) // Set the connection explicitly, so it won't be closed automatically by EF
                    .Options;

                // 建立資料庫綱要 (dabase schema)
                // You can use MigrateAsync if you use Migrations
                using (var context = new SampleDbContext(options))
                {
                    await context.Database.EnsureCreatedAsync();
                } // The connection is not closed, so the database still exists

                // 對資料庫加入資料
                using (var context = new SampleDbContext(options))
                {
                    var user = new User() { Email = "test@sample.com" };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }

                // Assert
                using (var context = new SampleDbContext(options))
                {
                    var count = await context.Users.CountAsync();
                    Assert.AreEqual(1, count);

                    var u = await context.Users.FirstOrDefaultAsync(user => user.Email == "test@sample.com");
                    Assert.IsNotNull(u);
                }
            }
        }



        // Create a wrapper that handle the connection lifetime
        // 包裹起來統一管理連線的生命週期及資源，實作IDisposable介面
        public class SampleDbContextFactory : IDisposable
        {
            private DbConnection _connection;

            private DbContextOptions<SampleDbContext> CreateOptions()
            {
                return new DbContextOptionsBuilder<SampleDbContext>()
                    .UseSqlite(_connection).Options;
            }

            public SampleDbContext CreateContext()
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection("DataSource=:memory:");
                    _connection.Open();

                    var options = CreateOptions();
                    using (var context = new SampleDbContext(options))
                    {
                        context.Database.EnsureCreated();
                    }
                }

                return new SampleDbContext(CreateOptions());
            }

            // Implement IDisposable.
            public void Dispose()
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }


        // 方法3: 使用工廠模式，統一管理連線生命週期 
        [Test]
        public async Task TestMethod_WithFactory()
        {
            using (var factory = new SampleDbContextFactory())
            {
                // Get a context
                using (var context = factory.CreateContext())
                {
                    var user = new User() { Email = "test@sample.com" };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }

                // Get another context using the same connection
                using (var context = factory.CreateContext())
                {
                    var count = await context.Users.CountAsync();
                    Assert.AreEqual(1, count);

                    var u = await context.Users.FirstOrDefaultAsync(user => user.Email == "test@sample.com");
                    Assert.IsNotNull(u);
                }
            }
        }




        // 測試 GetEmployeeEmail() 方法1: 真正呼叫DB
        [Test]
        public async Task TestDbValue_RealDB()
        {
            var obj1 = new CallDb();
            string emailResult = obj1.GetEmployeeEmail("Amy");
            Assert.AreEqual("Amy.kmail.com", emailResult);
        }

        // 測試 GetEmployeeEmail() 方法2: 使用in-memory


    }


    public class User
    {
        public int id { get; set; }
        public string Email { get; set; }
    }



}


