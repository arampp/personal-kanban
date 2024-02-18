using PersonalKanban.Domain.Board;
using PersonalKanban.Domain.Column;

using static PersonalKanbanTest.Util.MediatRTestHelper;

namespace PersonalKanbanTest.Domain.Column;

public class CreateColumnTests
{
    [Fact]
    public async Task Publishes_a_Column_created_event()
    {
        var context = new TestContext();
        var boardCreated = context.ObserveNotification<BoardCreated>();
        var columnCreated = context.ObserveNotification<ColumnCreated>();
        var mediator = context.BuildMediator();
        await mediator.Send(new CreateBoard("Title", "Description"));


        var boardId = boardCreated.Notification!.Id;
        await mediator.Send(new CreateColumn("Title", boardId));

        columnCreated.Notification!.Title.Should().Be("Title");
        columnCreated.Notification!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Throws_if_board_does_not_exist()
    {
        var context = new TestContext();
        var mediator = context.BuildMediator();

        var boardId = Guid.NewGuid();
        var send = async () => await mediator.Send(new CreateColumn("Title", boardId));

        await send.Should().ThrowAsync<RequestFailed>().WithMessage($"*{boardId}*");
    }

    // [Fact]
    // public async void Fails_if_title_is_empty()
    // {
    //     await SendRequest(new CreateColumn(" "))
    //         .AndExpectException<RequestFailed>()
    //         .WithMessage($"*${nameof(ColumnCreated.Title)}*");
    // }
}