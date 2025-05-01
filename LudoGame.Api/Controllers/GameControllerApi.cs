using LudoGame.Api.Dtos;
using LudoGame.Domain;
using Microsoft.AspNetCore.Mvc;

namespace LudoGame.Api.Controllers;

/// <summary>
/// API-controller der eksponerer GameController via HTTP.
/// Viser Dependency Injection og adskillelse af ansvar (S og D i SOLID).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GameControllerApi : ControllerBase
{
    private readonly IGameController _gameController;

    /// <summary>
    /// DI-injektion af GameController (kan udskiftes med mock i tests).
    /// </summary>
    public GameControllerApi(IGameController gameController)
    {
        _gameController = gameController;
    }

    /// <summary>
    /// Henter den aktuelle spiller (0-baseret).
    /// </summary>
    [HttpGet("current")]
    public ActionResult<int> GetCurrentPlayer()
    {
        return Ok(_gameController.GetCurrentPlayer());
    }


    /// <summary>
    /// Skifter til næste spiller.
    /// </summary>
    [HttpPost("next")]
    public IActionResult NextTurn()
    {
        _gameController.NextTurn();
        return Ok();
    }

    /// <summary>
    /// Slår med en terning og returnerer resultatet (1-6).
    /// </summary>
    [HttpPost("roll")]
    public ActionResult<int> RollDice()
    {
        return Ok(_gameController.RollDice());
    }
    [HttpGet("board")]
    public ActionResult<BoardStatusDto> GetBoardStatus()
    {
        return Ok(_gameController.GetBoardStatus());
    }
    /// <summary>
    /// Flytter en brik for den aktuelle spiller baseret på slaget.
    /// </summary>
    [HttpPost("move/{pieceId:int}")]
    public ActionResult<bool> MovePiece(int pieceId, [FromQuery] int dice)
    {
        var success = _gameController.MovePiece(pieceId, dice);
        return Ok(success);
    }
    [HttpGet("winner")]
    public ActionResult<int?> GetWinner()
    {
        return Ok(_gameController.CheckWinner());
    }
    /// <summary>
    /// API-endpoint til at nulstille spillet.
    /// </summary>
    [HttpPost("reset")]
    public IActionResult ResetGame()
    {
        _gameController.Reset();
        return Ok();
    }

    [HttpGet("canmove/{playerId}/{diceRoll}")]
    public ActionResult<bool> CanMove(int playerId, int diceRoll)
    {
        return Ok(_gameController.CanMoveAnyPiece(playerId, diceRoll));
    }

    [HttpGet("validmoves/{diceRoll}")]
    public ActionResult<List<int>> GetValidMoves(int diceRoll)
    {
        return Ok(_gameController.GetValidMoves(diceRoll));
    }

    [HttpGet("save")]
    public ActionResult<GameStateDto> SaveGame()
    {
        var state = _gameController.SaveGame();
        return Ok(state);
    }

    // 🆕 LOAD SPILLET
    [HttpPost("load")]
    public IActionResult LoadGame([FromBody] GameStateDto state)
    {
        _gameController.LoadGame(state);
        return Ok();
    }

    /// <summary>
    /// ✅ NYT: API-endpoint til at finde startspilleren ved at slå med en terning
    /// Matcher Ludo-reglen: højeste kast starter. Lighed → ny runde.
    /// </summary>
    [HttpPost("startplayer")]
    public ActionResult<int> DetermineStartingPlayer()
    {
        return Ok(_gameController.DetermineStartingPlayer());
    }



}
