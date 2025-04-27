using LudoGame.Api.Controllers;
using LudoGame.Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LudoGame.Tests.ControllerTests;

/// <summary>
/// Enhedstest af API-controller med mock af IGameController (Q1 test, Moq, D i SOLID).
/// </summary>
public class GameControllerApiTests
{
    [Fact]
    public void GetCurrentPlayer_ReturnsExpectedPlayer()
    {
        // Arrange
        var mockController = new Mock<IGameController>();
        mockController.Setup(x => x.GetCurrentPlayer()).Returns(2); // forventet spiller = 2
        var api = new GameControllerApi(mockController.Object);

        // Act
        var result = api.GetCurrentPlayer();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(2, okResult.Value);
    }

    [Fact]
    public void NextTurn_CallsNextTurnOnGameController()
    {
        // Arrange
        var mockController = new Mock<IGameController>();
        var api = new GameControllerApi(mockController.Object);

        // Act
        var result = api.NextTurn();

        // Assert
        Assert.IsType<OkResult>(result);
        mockController.Verify(x => x.NextTurn(), Times.Once);
    }


    [Fact]
    public void RollDice_ReturnsValueBetween1And6()
    {
        // Arrange
        var controller = new GameController(4); // 4 spillere

        // Act
        var result = controller.RollDice();

        // Assert
        Assert.InRange(result, 1, 6);
    }
}
