using LudoGame.Domain;
using LudoGame.Domain.Enums;
using Reqnroll;
using Xunit;

namespace LudoGame.Tests.Features;

[Binding]
public class MovePieceSteps
{
    private GameController? _game;
    private int _pieceId;
    private int _opponentPieceId;
    private int _diceRoll;
    private MoveResult _moveResult;

    [Given("en spiller har en brik i hjemmet")]
    public void GivenEnSpillerHarEnBrikIHjemmet()
    {
        _game = new GameController(2);
        _pieceId = 0;
    }

    [When("spilleren slår en 6'er")]
    public void WhenSpillerenSlaarEn6er()
    {
        _diceRoll = 6;
        _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
    }

    [When("spilleren slår en 4'er")]
    public void WhenSpillerenSlaarEn4er()
    {
        _diceRoll = 4;
        _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
    }

    [Then("brikken flyttes ud på startfeltet")]
    public void ThenBrikkenFlyttesUdPaaStartfeltet()
    {
        var piece = _game!.GetBoardStatus().Players[0].Pieces.First(p => p.Id == _pieceId);
        Assert.Equal(0, piece.Position);
        Assert.Equal(MoveResult.MovedAndExtraTurn, _moveResult);
    }

    [Then("brikken forbliver i hjemmet")]
    public void ThenBrikkenForbliverIHjemmet()
    {
        var piece = _game!.GetBoardStatus().Players[0].Pieces.First(p => p.Id == _pieceId);
        Assert.Equal(-1, piece.Position);
        Assert.Equal(MoveResult.Invalid, _moveResult);
    }

    [Given("en spiller har en brik på felt {int}")]
    public void GivenEnSpillerHarEnBrikPaFelt(int felt)
    {
        _game = new GameController(2);
        _pieceId = 0;
        _game.SetPiecePosition(0, _pieceId, felt); // direkte placering
    }

    [Then("brikken flyttes til felt {int}")]
    public void ThenBrikkenFlyttesTilFelt(int felt)
    {
        var piece = _game!.GetBoardStatus().Players[0].Pieces.First(p => p.Id == _pieceId);
        Assert.Equal(felt, piece.Position);
        Assert.Equal(MoveResult.Moved, _moveResult);
    }

    [Given("en modstander har en brik på felt 8")]
    public void GivenEnModstanderHarEnBrikPaaFelt8()
    {
        _game!.NextTurn(); // Gør spiller 1 aktiv
        _opponentPieceId = 0;

        // Sæt modstanderens brik på relativ position 47 (=> abs = 8)
        _game.SetPiecePosition(1, _opponentPieceId, 47);

        int abs = _game.GetAbsoluteBoardPosition(1, 47);
        Assert.Equal(8, abs); // bekræft
        _game.NextTurn(); // Tilbage til spiller 0
        _game.SetPiecePosition(0, _pieceId, 5); // spillerens brik på rel 5 → abs 5
    }

    [When("spilleren slår en 3'er")]
    public void WhenSpillerenSlaarEn3er()
    {
        _diceRoll = 3;
        _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
    }

    [Then("modstanderens brik flyttes hjem")]
    public void ThenModstanderensBrikFlyttesHjem()
    {
        var piece = _game!.GetBoardStatus().Players[1].Pieces.First(p => p.Id == _opponentPieceId);
        Assert.Equal(-1, piece.Position);
    }

    [Then("spillerens brik flyttes til felt 8")]
    public void ThenSpillerensBrikFlyttesTilFelt8()
    {
        var piece = _game!.GetBoardStatus().Players[0].Pieces.First(p => p.Id == _pieceId);
        int abs = _game!.GetAbsoluteBoardPosition(0, piece.Position);
        Assert.Equal(8, abs);
        Assert.Equal(MoveResult.Moved, _moveResult);
    }

    [Given("en spiller prøver at flytte en ugyldig brik")]
    public void GivenEnSpillerProeverAtFlytteEnUgyldigBrik()
    {
        _game = new GameController(2);
        _pieceId = 99; // ugyldig ID
    }

    [When("handlingen udføres")]
    public void WhenHandlingenUdfoeres()
    {
        _diceRoll = 6;
        _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
    }

    [Then("spillets tilstand ændres ikke")]
    public void ThenSpilletsTilstandAEndresIkke()
    {
        Assert.Equal(MoveResult.Invalid, _moveResult);
    }
}
