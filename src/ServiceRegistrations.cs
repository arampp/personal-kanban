using MediatR;

using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.Serialization.Json;


using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Card;
using PersonalKanban.Domain.Column;
using PersonalKanban.Models;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using System.Data;

namespace PersonalKanban;

public static class ServiceRegistrations
{
    public static IServiceCollection AddPersonalKanbanServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var eventStore = Wireup.Init()
            .UsingSqlPersistence(SqliteFactory.Instance, connectionString)
            .WithDialect(new SqliteDialect())
            .InitializeStorageEngine()
            .UsingJsonSerialization()
            .Build();

        return services.AddPersonalKanbanServices(eventStore);
    }

    public static IServiceCollection AddPersonalKanbanServicesInMemory(this IServiceCollection services)
    {
        var eventStore = Wireup.Init()
            .UsingInMemoryPersistence()
            .InitializeStorageEngine()
            .UsingJsonSerialization()
            .Build();

        return services.AddPersonalKanbanServices(eventStore);
    }

    public static IServiceCollection AddPersonalKanbanServices(this IServiceCollection services, IStoreEvents eventStore)
    {
        return services
            .AddSingleton<IStoreEvents>(eventStore)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CardsReadModel>())
            .AddModels();

    }

    public static Task AddDummyData(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Adding dummy data");
        var mediator = app.Services.GetRequiredService<IMediator>();
        var board = new BoardCreated(Guid.Parse("74540f70-6189-4dcf-bd9c-91a1878c782d"), "Kanban Board", "A board to visualize work");
        mediator.Publish(board);
        var todo = new ColumnCreated(Guid.Parse("e6bffa94-ec10-4924-97fd-c25b6e9544c8"), "To Do", board.Id);
        var doing = new ColumnCreated(Guid.Parse("8dff364b-c2de-45c9-9ecf-d9642d24babb"), "Doing", board.Id);
        var done = new ColumnCreated(Guid.Parse("b2c74738-3998-4e89-bf32-aa8ed7e75fec"), "Done", board.Id);
        mediator.Publish(todo);
        mediator.Publish(doing);
        mediator.Publish(done);

        var cards = new List<CardCreated> {
         new(Guid.Parse("f3e3e3e3-3e3e-3e3e-3e3e-3e3e3e3e3e3e"), "Set up project", "", done.Id),
         new (Guid.Parse("a14af2e6-edc2-4349-bc32-9bf1f1f240c0"), "Remember how to work with MediatR", "", done.Id),
         new (Guid.Parse("a493c10f-b5e2-43d0-8de9-32194d27d96f"), "Get familiar with event sourcing", "", done.Id),
         new (Guid.Parse("03768c53-8d1d-4c96-b4e8-139fc43fc4eb"), "Write basic functionality", "", doing.Id),
         new (Guid.Parse("51e8c5ba-f923-45f5-95bb-55e5164b319b"), "Write unit tests", "", doing.Id),
         new (Guid.Parse("7e634595-d77c-4bad-bfcb-0ee71793a588"), "Integrate with Electron shell", "", todo.Id),
         new (Guid.Parse("8167cd68-18be-4d58-9347-9dd92e111d16"), "Think about a release process", "", todo.Id),
        };

        cards.ForEach(card => mediator.Publish(card));
        return Task.CompletedTask;
    }
}

public class SQLiteConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public SQLiteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Type GetDbProviderFactoryType()
    {
        return typeof(SQLiteConnection);
    }

    public IDbConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}

