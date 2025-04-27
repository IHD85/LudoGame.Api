using LudoGame.Api.Dtos;
using LudoGame.Domain; // Reference til din forretningslogik
using Xunit;

namespace LudoGame.Tests;

/// <summary>
/// Unit tests for GameController – TDD, xUnit, Q1 i testkvadranten, SRP fra SOLID.
/// </summary>
public class GameControllerTests
{
    [Fact]
    public void StartsWithPlayer0()
    {
        // Arrange
        var controller = new GameController(4);

        // Act
        var player = controller.GetCurrentPlayer();

        // Assert
        Assert.Equal(0, player);
    }

    [Fact]
    public void NextTurn_MovesToNextPlayer()
    {
        var controller = new GameController(4);
        controller.NextTurn();
        Assert.Equal(1, controller.GetCurrentPlayer());
    }

    [Fact]
    public void NextTurn_RollsOverAfterLastPlayer()
    {
        var controller = new GameController(2);
        controller.NextTurn(); // -> 1
        controller.NextTurn(); // -> 0
        Assert.Equal(0, controller.GetCurrentPlayer());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Constructor_WithInvalidPlayerCount_Throws(int players)
    {
        Assert.Throws<ArgumentException>(() => new GameController(players));
    }

   
        [Fact]
        public void MovePiece_FromHome_WithSix_ShouldPlaceOnBoard()
        {
            // Arrange
            var controller = new GameController(2);
            var playerId = controller.GetCurrentPlayer();
            var pieceId = 0;

            // Act
            controller.MovePiece(pieceId, 6); // 6 = må flytte ud
            var board = controller.GetBoardStatus();

            // Assert
            var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);
            Assert.Equal(0, piece.Position); // position 0 = på brættet
        }

        [Fact]
        public void MovePiece_FromHome_WithoutSix_ShouldStayAtHome()
        {
            // Arrange
            var controller = new GameController(2);
            var playerId = controller.GetCurrentPlayer();
            var pieceId = 1;

            // Act
            controller.MovePiece(pieceId, 4); // ikke 6
            var board = controller.GetBoardStatus();

            // Assert
            var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);
            Assert.Equal(-1, piece.Position); // stadig hjemme
        }

        [Fact]
        public void MovePiece_OnBoard_ShouldMoveForward()
        {
            // Arrange
            var controller = new GameController(2);
            var playerId = controller.GetCurrentPlayer();
            var pieceId = 2;

            controller.MovePiece(pieceId, 6); // flyt ud
            controller.MovePiece(pieceId, 3); // flyt 3 frem

            // Act
            var board = controller.GetBoardStatus();
            var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);

            // Assert
            Assert.Equal(3, piece.Position); // 0 + 3 = 3
        }

        [Fact]
        public void MovePiece_InvalidPieceId_ShouldDoNothing()
        {
            // Arrange
            var controller = new GameController(2);
            var playerId = controller.GetCurrentPlayer();

            // Act
            controller.MovePiece(99, 6); // ugyldig brik-ID
            var board = controller.GetBoardStatus();

            // Assert
            foreach (var piece in board.Players[playerId].Pieces)
            {
                Assert.Equal(-1, piece.Position); // ingen blev flyttet
            }
        }

        [Fact]
        public void MovePiece_ShouldOnlyAffectCurrentPlayer()
        {
            // Arrange
            var controller = new GameController(2);
            var otherPlayerId = (controller.GetCurrentPlayer() + 1) % 2;

            // Act
            controller.MovePiece(0, 6); // prøver at flytte en brik fra den forkerte spiller (brik-ID 0)
            var board = controller.GetBoardStatus();

            // Assert
            foreach (var piece in board.Players[otherPlayerId].Pieces)
            {
                Assert.Equal(-1, piece.Position); // ingen blev flyttet
            }
        }
    [Fact]
    public void CheckWinner_ShouldReturnNull_WhenNoPlayerHasWon()
    {
        // Arrange
        var controller = new GameController(totalPlayers: 2);

        // Act
        var winner = controller.CheckWinner();

        // Assert
        Assert.Null(winner);
    }

    [Fact]
    public void CheckWinner_ShouldReturnPlayerId_WhenAllPiecesAreInGoal()
    {
        // Arrange
        var controller = new GameController(totalPlayers: 2);

        // Sæt alle brikker for spiller 0 til mål (position 100+)
        var board = controller.GetBoardStatus();
        foreach (var piece in board.Players[0].Pieces)
        {
            piece.Position = 100;
        }

        // Act
        var winner = controller.CheckWinner();

        // Assert
        Assert.Equal(0, winner); // Spiller 0 vinder
    }

    [Fact]
    public void CheckWinner_ShouldOnlySetWinnerOnce()
    {
        // Arrange
        var controller = new GameController(totalPlayers: 2);

        // Sæt alle brikker for spiller 0 til mål
        var board = controller.GetBoardStatus();
        foreach (var piece in board.Players[0].Pieces)
        {
            piece.Position = 100;
        }

        // Første vinder-check
        var firstWinner = controller.CheckWinner();
        Assert.Equal(0, firstWinner);

        // Ændr så spiller 1 også er i mål (bør ikke overskrive vinder)
        foreach (var piece in board.Players[1].Pieces)
        {
            piece.Position = 100;
        }

        // Act
        var secondWinner = controller.CheckWinner();

        // Assert
        Assert.Equal(0, secondWinner); // Vinder må ikke ændres
    }

}
