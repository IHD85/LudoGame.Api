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
        var controller = new GameController(4);
        var player = controller.GetCurrentPlayer();
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
        var controller = new GameController(2);
        var playerId = controller.GetCurrentPlayer();
        var pieceId = 0;

        controller.MovePiece(pieceId, 6);
        var board = controller.GetBoardStatus();
        var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);

        Assert.Equal(0, piece.Position);
    }

    [Fact]
    public void MovePiece_FromHome_WithoutSix_ShouldStayAtHome()
    {
        var controller = new GameController(2);
        var playerId = controller.GetCurrentPlayer();
        var pieceId = 1;

        controller.MovePiece(pieceId, 4);
        var board = controller.GetBoardStatus();
        var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);

        Assert.Equal(-1, piece.Position);
    }

    [Fact]
    public void MovePiece_OnBoard_ShouldMoveForward()
    {
        var controller = new GameController(2);
        var playerId = controller.GetCurrentPlayer();
        var pieceId = 2;

        controller.MovePiece(pieceId, 6);
        controller.MovePiece(pieceId, 3);

        var board = controller.GetBoardStatus();
        var piece = board.Players[playerId].Pieces.First(p => p.Id == pieceId);

        Assert.Equal(3, piece.Position);
    }

    [Fact]
    public void MovePiece_InvalidPieceId_ShouldDoNothing()
    {
        var controller = new GameController(2);
        var playerId = controller.GetCurrentPlayer();

        controller.MovePiece(99, 6);
        var board = controller.GetBoardStatus();

        foreach (var piece in board.Players[playerId].Pieces)
        {
            Assert.Equal(-1, piece.Position);
        }
    }

    [Fact]
    public void MovePiece_ShouldOnlyAffectCurrentPlayer()
    {
        var controller = new GameController(2);
        var otherPlayerId = (controller.GetCurrentPlayer() + 1) % 2;

        controller.MovePiece(0, 6);
        var board = controller.GetBoardStatus();

        foreach (var piece in board.Players[otherPlayerId].Pieces)
        {
            Assert.Equal(-1, piece.Position);
        }
    }

    [Fact]
    public void CheckWinner_ShouldReturnNull_WhenNoPlayerHasWon()
    {
        var controller = new GameController(2);
        var winner = controller.CheckWinner();
        Assert.Null(winner);
    }

    [Fact]
    public void CheckWinner_ShouldReturnPlayerId_WhenAllPiecesAreInGoal()
    {
        var controller = new GameController(2);
        var board = controller.GetBoardStatus();

        foreach (var piece in board.Players[0].Pieces)
        {
            piece.Position = 100;
        }

        var winner = controller.CheckWinner();
        Assert.Equal(0, winner);
    }

    [Fact]
    public void CheckWinner_ShouldOnlySetWinnerOnce()
    {
        var controller = new GameController(2);
        var board = controller.GetBoardStatus();

        foreach (var piece in board.Players[0].Pieces)
        {
            piece.Position = 100;
        }

        var firstWinner = controller.CheckWinner();
        Assert.Equal(0, firstWinner);

        foreach (var piece in board.Players[1].Pieces)
        {
            piece.Position = 100;
        }

        var secondWinner = controller.CheckWinner();
        Assert.Equal(0, secondWinner);
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
        var validMoves = game.GetValidMoves(6);
        Assert.NotEmpty(validMoves);
    }

    [Fact]
    public void GetValidMoves_ShouldReturnEmpty_WhenDiceIsNot6_AndAllPiecesInHome()
    {
        var game = new GameController(2);
        var validMoves = game.GetValidMoves(4);
        Assert.Empty(validMoves);
    }

    [Fact]
    public void MovePiece_ShouldMovePiece_WhenValidMove()
    {
        var game = new GameController(2);
        bool moved = game.MovePiece(0, 6);
        Assert.True(moved);

        var board = game.GetBoardStatus();
        var player = board.Players[0];
        Assert.Equal(0, player.Pieces[0].Position);
    }

    [Fact]
    public void MovePiece_ShouldNotMove_WhenInvalidMove()
    {
        var game = new GameController(2);
        bool moved = game.MovePiece(0, 4);
        Assert.False(moved);
    }

    [Fact]
    public void MovePiece_Should_Kick_Opponent_Home_When_Landing_On_Them()
    {
        var game = new GameController(2);

        game.MovePiece(0, 6);
        game.MovePiece(0, 5);

        game.NextTurn();
        game.MovePiece(0, 6);
        game.MovePiece(0, 5);

        var board = game.GetBoardStatus();
        var player1Piece = board.Players[0].Pieces.First(p => p.Id == 0);
        var player2Piece = board.Players[1].Pieces.First(p => p.Id == 0);

        Assert.Equal(-1, player1Piece.Position);
        Assert.Equal(5, player2Piece.Position);
    }

    [Fact]
    public void SaveAndLoadGame_ShouldRestoreStateCorrectly()
    {
        var controller = new GameController(2);
        controller.MovePiece(0, 6);
        controller.MovePiece(0, 3);

        var savedState = controller.SaveGame();
        var newController = new GameController(2);
        newController.LoadGame(savedState);

        var loadedBoard = newController.GetBoardStatus();

        Assert.Equal(savedState.CurrentPlayer, newController.GetCurrentPlayer());
        Assert.Equal(savedState.WinnerId, newController.CheckWinner());

        var savedPlayer0Piece0 = savedState.Players[0].Pieces.First(p => p.Id == 0);
        var loadedPlayer0Piece0 = loadedBoard.Players[0].Pieces.First(p => p.Id == 0);

        Assert.Equal(savedPlayer0Piece0.Position, loadedPlayer0Piece0.Position);
    }

    /// <summary>
    /// ✅ NY TEST: Sikrer at startspilleren findes korrekt ved første slag.
    /// Matcher eksamenskrav om testbar kode og Ludo-regel om højeste terningkast.
    /// </summary>
    [Fact]
    public void DetermineStartingPlayer_ShouldReturnValidPlayerId()
    {
        var controller = new GameController(4);
        int starter = controller.DetermineStartingPlayer();
        Assert.InRange(starter, 0, 3);
    }


    [Fact]
    public void NextTurn_ShouldSkipWinner()
    {
        // Arrange
        var controller = new GameController(2);
        var board = controller.GetBoardStatus();

        // 💡 Flyt tur til spiller 1
        controller.NextTurn(); // Nu: CurrentPlayer = 1

        // 🔁 Gør spiller 0 til vinder
        foreach (var piece in board.Players[0].Pieces)
        {
            piece.Position = 100; // I mål
        }

        controller.CheckWinner(); // Registrér vinder

        // Act
        controller.NextTurn(); // Skulle springe spiller 0 over (de har vundet)

        // Assert
        Assert.Equal(1, controller.GetCurrentPlayer()); // Tilbage til spiller 1
    }





    [Fact]
    public void NextTurn_ShouldEndGame_WhenOnlyOnePlayerLeft()
    {
        var controller = new GameController(2);
        var board = controller.GetBoardStatus();

        // 🎯 Gør spiller 1 til vinder → kun spiller 0 tilbage
        foreach (var piece in board.Players[1].Pieces)
            piece.Position = 100;

        controller.CheckWinner(); // Marker spiller 1 som vinder

        controller.NextTurn(); // Skal sætte spiller 0 som ny vinder

        int? winner = controller.CheckWinner();
        Assert.Equal(1, winner);
        
    }

    [Fact]
    public void MovePiece_ShouldOnlyAllowThreeAttemptsFromHome()
    {
        var controller = new GameController(2);
        var pieceId = 0;

        // Tre mislykkede forsøg (ikke 6)
        Assert.False(controller.MovePiece(pieceId, 2));
        Assert.False(controller.MovePiece(pieceId, 3));
        Assert.False(controller.MovePiece(pieceId, 4));

        // Fjerde forsøg – selvom det er 6 – skal stadig afvises
        Assert.False(controller.MovePiece(pieceId, 6));

        // Skift tur (nulstiller forsøg)
        controller.NextTurn();
        controller.NextTurn(); // tilbage til spiller 0

        // Ny chance – 6 virker nu
        Assert.True(controller.MovePiece(pieceId, 6));
    }

    [Fact]
    public void HandleRollResult_ShouldSkipTurn_WhenNoMovesAndNoSix()
    {
        var controller = new GameController(2);
        int current = controller.GetCurrentPlayer();

        // Ingen brikker ude, slag er ikke 6
        controller.HandleRollResult(4);

        // Tur burde være skiftet
        Assert.NotEqual(current, controller.GetCurrentPlayer());
    }

    [Fact]
    public void HandleRollResult_ShouldNotSkipTurn_WhenRollIsSix()
    {
        var controller = new GameController(2);
        int current = controller.GetCurrentPlayer();

        // Ingen brikker ude, men slog en 6’er
        controller.HandleRollResult(6);

        // Tur skal blive hos spilleren
        Assert.Equal(current, controller.GetCurrentPlayer());
    }




}