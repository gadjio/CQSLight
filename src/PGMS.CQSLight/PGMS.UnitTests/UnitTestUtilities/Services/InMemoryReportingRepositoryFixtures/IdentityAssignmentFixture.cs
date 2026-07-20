using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.FakeImpl.DataProvider.Context;
using TestContext = PGMS.FakeImpl.DataProvider.Context.TestContext;

namespace PGMS.UnitTests.UnitTestUtilities.Services.InMemoryReportingRepositoryFixtures;

/// <summary>
/// Guards the in-memory repository's emulation of a SQL Server IDENTITY column. Rows created
/// by command / event handlers don't set their surrogate <c>Id</c>; a real DB fills it on
/// INSERT. Without emulation the in-memory store left <c>Id = 0</c>, so handlers/fixtures that
/// resolve entities by that Id (e.g. a team/message/report SQL Id) matched on 0 and failed.
/// The repository now assigns a monotonic per-type Id when the key is store-generated.
/// </summary>
[TestFixture]
public class IdentityAssignmentFixture
{
    private InMemoryReportingRepository<MyDbContext> entityRepository;

    [SetUp]
    public void SetUp()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseSqlServer("connectionString");
        var dbContext = new MyDbContext(optionsBuilder.Options);

        entityRepository = new InMemoryReportingRepository<MyDbContext>(dbContext);
    }

    [Test]
    public void Assigns_monotonic_identity_when_key_is_store_generated_and_default()
    {
        var first = new GameState { State = "A" };   // Id left at its default (0)
        var second = new GameState { State = "B" };

        entityRepository.Insert(first);
        entityRepository.Insert(second);

        Assert.That(first.Id, Is.EqualTo(1), "First store-generated Id should be 1 (emulated IDENTITY).");
        Assert.That(second.Id, Is.EqualTo(2), "Second insert should increment the per-type identity counter.");
    }

    [Test]
    public async Task Assigns_identity_via_InsertOperationAsync_like_the_command_handlers()
    {
        var state = new GameState { State = "Async" };

        // This is the exact entry point command/event handlers use (InsertOperationAsync).
        await entityRepository.InsertOperationAsync(null, state);

        Assert.That(state.Id, Is.GreaterThan(0),
            "Rows inserted by handlers must receive a non-zero surrogate Id, as in production.");
    }

    [Test]
    public void Preserves_explicit_id_and_keeps_counter_past_it()
    {
        var seeded = new GameState { Id = 42, State = "seed" };
        var auto = new GameState { State = "auto" };

        entityRepository.Insert(seeded);
        entityRepository.Insert(auto);

        Assert.That(seeded.Id, Is.EqualTo(42), "An explicitly-set Id (a seeded row) must not be overwritten.");
        Assert.That(auto.Id, Is.EqualTo(43), "The counter must advance past an explicit Id to avoid a collision.");
    }

    [Test]
    public void Identity_counters_are_independent_per_entity_type()
    {
        var state = new GameState { State = "S" };
        var player = new Player();

        entityRepository.Insert(state);
        entityRepository.Insert(player);

        Assert.That(state.Id, Is.EqualTo(1));
        Assert.That(player.Id, Is.EqualTo(1), "Each CLR type keeps its own identity sequence.");
    }

    [Test]
    public void BulkInsert_assigns_identity_to_every_row()
    {
        var rows = new List<GameState>
        {
            new() { State = "bulk-1" },
            new() { State = "bulk-2" },
            new() { State = "bulk-3" },
        };

        entityRepository.BulkInsertOperationAsync(null, rows).GetAwaiter().GetResult();

        Assert.That(rows.Select(r => r.Id), Is.EqualTo(new long[] { 1, 2, 3 }),
            "Bulk-inserted rows must each get a distinct, monotonic Id.");
    }

    [Test]
    public void Does_not_assign_for_composite_key_entity()
    {
        var repo = BuildTestContextRepo();
        var participant = new ProjectParticipant { ProjectId = 7, ClientId = 9 };

        Assert.DoesNotThrow(() => repo.Insert(participant));

        var found = repo.FindFirst<ProjectParticipant>(p => p.ProjectId == 7 && p.ClientId == 9);
        Assert.That(found, Is.Not.Null, "A composite-key entity inserts normally — there is no single identity to assign.");
    }

    [Test]
    public void Does_not_throw_for_keyless_entity()
    {
        var repo = BuildTestContextRepo();
        var view = new KeylessViewReporting { Name = "v", Value = 5 };

        Assert.DoesNotThrow(() => repo.Insert(view));

        var found = repo.FindFirst<KeylessViewReporting>(v => v.Name == "v");
        Assert.That(found, Is.Not.Null, "A keyless entity (no primary key) inserts normally and is left untouched.");
    }

    private static InMemoryReportingRepository<TestContext> BuildTestContextRepo()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
        optionsBuilder.UseSqlServer("connectionString");
        return new InMemoryReportingRepository<TestContext>(new TestContext(optionsBuilder.Options));
    }
}
