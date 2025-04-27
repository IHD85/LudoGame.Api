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


    [Fact]
    public void RollDice_ShouldReturnValueBetween1And6()
    {
        var game = new GameController(4);
        int roll = game.RollDice();
        Assert.InRange(roll, 1, 6);
    }

    [Fact]
    public void GetValidMoves_ShouldReturnCorrectMoves_WhenDiceIs6()
    {
        var game = new GameController(2);

        // Ingen brikker på brættet
        var validMoves = game.GetValidMoves(6);

        Assert.NotEmpty(validMoves); // Burde kunne flytte ud af hjem
    }

    [Fact]
    public void GetValidMoves_ShouldReturnEmpty_WhenDiceIsNot6_AndAllPiecesInHome()
    {
        var game = new GameController(2);

        var validMoves = game.GetValidMoves(4);

        Assert.Empty(validMoves); // Kan ikke rykke uden 6'er fra hjem
    }

    [Fact]
    public void MovePiece_ShouldMovePiece_WhenValidMove()
    {
        var game = new GameController(2);

        bool moved = game.MovePiece(0, 6); // Slår en 6'er og flytter ud
        Assert.True(moved);

        var board = game.GetBoardStatus();
        var player = board.Players[0];
        Assert.Equal(0, player.Pieces[0].Position); // Brikken er nu på felt 0
    }

    [Fact]
    public void MovePiece_ShouldNotMove_WhenInvalidMove()
    {
        var game = new GameController(2);

        bool moved = game.MovePiece(0, 4); // Kan ikke flytte uden 6'er
        Assert.False(moved);
    }


    [Fact]
    public void MovePiece_Should_Kick_Opponent_Home_When_Landing_On_Them()
    {
        // Arrange
        var game = new GameController(2);

        // 🚶‍♂️ Sæt en spiller 1 brik på position 5
        game.MovePiece(0, 6); // Først få brik ud (slå 6)
        game.MovePiece(0, 5); // Flyt 5 frem

        // Skift tur til spiller 2
        game.NextTurn();

        // 🚶‍♂️ Spiller 2 får også sin brik ud
        game.MovePiece(0, 6); // Slå 6 og ud

        // Flyt spiller 2 brik til position 5 (samme som spiller 1)
        game.MovePiece(0, 5); // Flyt frem til 5

        // Act
        var board = game.GetBoardStatus();
        var player1Piece = board.Players[0].Pieces.First(p => p.Id == 0);
        var player2Piece = board.Players[1].Pieces.First(p => p.Id == 0);

        // Assert
        Assert.Equal(-1, player1Piece.Position); // ✅ Spiller 1's brik skal være hjemme
        Assert.Equal(5, player2Piece.Position);  // ✅ Spiller 2's brik er nu på felt 5
    }

    [Fact]
    public void SaveAndLoadGame_ShouldRestoreStateCorrectly()
    {
        // Arrange
        var controller = new GameController(2);

        // Simuler spil (slå brikker ud og flyt)
        controller.MovePiece(0, 6); // Ud
        controller.MovePiece(0, 3); // 3 frem

        var savedState = controller.SaveGame();

        var newController = new GameController(2);
        newController.LoadGame(savedState);

        var loadedBoard = newController.GetBoardStatus();

        // Assert
        Assert.Equal(savedState.CurrentPlayer, newController.GetCurrentPlayer());
        Assert.Equal(savedState.WinnerId, newController.CheckWinner());

        var savedPlayer0Piece0 = savedState.Players[0].Pieces.First(p => p.Id == 0);
        var loadedPlayer0Piece0 = loadedBoard.Players[0].Pieces.First(p => p.Id == 0);

        Assert.Equal(savedPlayer0Piece0.Position, loadedPlayer0Piece0.Position);
    }


}
