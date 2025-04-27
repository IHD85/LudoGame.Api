using LudoGame.Api.Controllers;
using LudoGame.Api.Dtos;
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



    [Fact]
    public void SaveGame_CallsSaveGameOnController()
    {
        // Arrange
        var mockController = new Mock<IGameController>();
        var dummyState = new GameStateDto
        {
            CurrentPlayer = 0,
            WinnerId = null,
            Players = new List<PlayerDto>()
        };
        mockController.Setup(x => x.SaveGame()).Returns(dummyState); // 🛠 her!!

        var api = new GameControllerApi(mockController.Object);

        // Act
        var result = api.SaveGame();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result); // OkObjectResult ✅
        Assert.IsType<GameStateDto>(okResult.Value); // GameStateDto ✅
        mockController.Verify(x => x.SaveGame(), Times.Once);
    }




    [Fact]
    public void LoadGame_CallsLoadGameOnController()
    {
        var mockController = new Mock<IGameController>();
        var api = new GameControllerApi(mockController.Object);

        var dummyState = new GameStateDto
        {
            CurrentPlayer = 0,
            WinnerId = null,
            Players = new List<PlayerDto>
        {
            new PlayerDto
            {
                Id = 0,
                Color = "Rød",
                Pieces = new List<PieceDto>
                {
                    new PieceDto { Id = 0, Position = 0 },
                    new PieceDto { Id = 1, Position = -1 },
                    new PieceDto { Id = 2, Position = -1 },
                    new PieceDto { Id = 3, Position = -1 }
                }
            }
        }
        };

        var result = api.LoadGame(dummyState);

        Assert.IsType<OkResult>(result);
        mockController.Verify(x => x.LoadGame(It.IsAny<GameStateDto>()), Times.Once);
    }


}
