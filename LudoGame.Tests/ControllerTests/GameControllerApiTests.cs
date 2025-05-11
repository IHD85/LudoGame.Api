using FluentAssertions;
using LudoGame.Api.Controllers;
using LudoGame.Api.Dtos;
using LudoGame.Domain;
using LudoGame.Domain.Enums;
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
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(2);
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
        result.Should().BeOfType<OkResult>();
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
        result.Should().BeInRange(1, 6);

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
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GameStateDto>();
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

        result.Should().BeOfType<OkResult>();
        mockController.Verify(x => x.LoadGame(It.IsAny<GameStateDto>()), Times.Once);
    }

    [Fact]
    public void RollDice_ReturnsOk_WithDiceValue()
    {
        // Arrange
        var mockGameController = new Mock<IGameController>();
        mockGameController.Setup(gc => gc.RollDice()).Returns(4); // simulerer at terningen slår 4

        var controller = new GameControllerApi(mockGameController.Object);

        // Act
        var result = controller.RollDice();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(4); // vi forventer at 4 returneres
    }

    [Fact]
    public void GetBoardStatus_ReturnsOk_WithBoardStatusDto()
    {
        // Arrange
        var mockGameController = new Mock<IGameController>();
        var expectedStatus = new BoardStatusDto
        {
            CurrentPlayer = 1,
            WinnerId = null,
            Players = new List<PlayerDto>() // tom liste for test
        };

        mockGameController.Setup(gc => gc.GetBoardStatus()).Returns(expectedStatus);

        var controller = new GameControllerApi(mockGameController.Object);

        // Act
        var result = controller.GetBoardStatus();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDto = okResult.Value.Should().BeOfType<BoardStatusDto>().Subject;
        returnedDto.CurrentPlayer.Should().Be(1);
        returnedDto.Players.Should().BeEmpty();
    }

    [Fact]
    public void MovePiece_ReturnsOk_WithMoveResult()
    {
        // Arrange
        var mockGameController = new Mock<IGameController>();
        int pieceId = 2;
        int dice = 6;
        var expectedResult = MoveResult.MovedAndExtraTurn;

        mockGameController
            .Setup(gc => gc.MovePiece(pieceId, dice))
            .Returns(expectedResult);

        var controller = new GameControllerApi(mockGameController.Object);

        // Act
        var result = controller.MovePiece(pieceId, dice);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public void GetWinner_ReturnsOk_WithWinnerId()
    {
        var mock = new Mock<IGameController>();
        mock.Setup(gc => gc.CheckWinner()).Returns(1); // spiller 1 har vundet

        var controller = new GameControllerApi(mock.Object);
        var result = controller.GetWinner();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(1);
    }

    [Fact]
    public void ResetGame_ReturnsOk()
    {
        var mock = new Mock<IGameController>();
        var controller = new GameControllerApi(mock.Object);

        var result = controller.ResetGame();

        result.Should().BeOfType<OkResult>();
        mock.Verify(gc => gc.Reset(), Times.Once);
    }

    [Theory]
    [InlineData(0, 6, true)]
    [InlineData(1, 3, false)]
    public void CanMove_ReturnsExpected(int playerId, int diceRoll, bool expected)
    {
        var mock = new Mock<IGameController>();
        mock.Setup(x => x.CanMoveAnyPiece(playerId, diceRoll)).Returns(expected);

        var controller = new GameControllerApi(mock.Object);
        var result = controller.CanMove(playerId, diceRoll);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(expected);
    }


    [Fact]
    public void GetValidMoves_ReturnsOk_WithList()
    {
        var mock = new Mock<IGameController>();
        var validMoves = new List<int> { 0, 2 };
        mock.Setup(gc => gc.GetValidMoves(6)).Returns(validMoves);

        var controller = new GameControllerApi(mock.Object);
        var result = controller.GetValidMoves(6);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = ok.Value.Should().BeOfType<List<int>>().Subject;
        returned.Should().BeEquivalentTo(validMoves);
    }


    [Fact]
    public void DetermineStartingPlayer_ReturnsOk_WithPlayerIndex()
    {
        // Arrange
        var mock = new Mock<IGameController>();
        mock.Setup(gc => gc.DetermineStartingPlayer()).Returns(2); // f.eks. spiller 2 starter

        var controller = new GameControllerApi(mock.Object);

        // Act
        var result = controller.DetermineStartingPlayer();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(2);
    }


}
