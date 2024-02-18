using MediatR;

using PersonalKanban.Domain.Card;

namespace PersonalKanban.Models;

public class Card
{
    required public Guid Id { get; init; }
    required public string Title { get; init; }
    required public string Description { get; init; }
    required public Guid ColumnId { get; init; }
}


public class CardNotificationHandler(CardsReadModel cardsCollection) : INotificationHandler<CardCreated>
{
    public Task Handle(CardCreated notification, CancellationToken cancellationToken)
    {
        var card = new Card
        {
            Id = notification.Id,
            Title = notification.Title,
            Description = notification.Description,
            ColumnId = notification.ColumnId
        };
        cardsCollection.Add(card);
        return Task.CompletedTask;
    }
}


public interface ICardsProvider
{
    IEnumerable<Card> Cards { get; }
    Card GetById(Guid id);
    // TODO temporary refactor later on
    Task AddCard(string name, string description, Guid columnId);
}

public class CardsService(ISender sender, CardsReadModel cardsReadModel) : ICardsProvider
{
    public IEnumerable<Card> Cards => cardsReadModel.Cards;


    public async Task AddCard(string name, string description, Guid columnId)
    {
        await sender.Send(new CreateCard(name, description, columnId));
    }

    public Card GetById(Guid id)
    {
        return cardsReadModel.Cards.Single(c => c.Id == id);
    }
}

public class CardsReadModel
{
    private readonly ICollection<Card> _cards = [];

    public IEnumerable<Card> Cards => _cards;
    public void Add(Card card)
    {
        _cards.Add(card);
    }
}