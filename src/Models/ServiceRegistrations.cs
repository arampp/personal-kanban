namespace PersonalKanban.Models;

public static class ServiceRegistrations
{
    public static IServiceCollection AddModels(this IServiceCollection services)
    {
        services.AddSingleton<BoardsReadModel>();
        services.AddSingleton<ColumnsReadModel>();
        services.AddSingleton<CardsReadModel>();
        services.AddTransient<ICardsProvider, CardsService>();
        services.AddTransient<IColumnsProvider, ColumnsService>();
        services.AddTransient<IBoardsProvider, BoardsService>();

        return services;
    }

}