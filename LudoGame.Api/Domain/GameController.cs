using LudoGame.Api.Dtos;

namespace LudoGame.Domain;

/// <summary>
/// GameController styrer spillets gang: aktuel spiller, terningekast, flytning og vindertjek.
/// Opbygget efter SOLID-principper:
/// - S: Single Responsibility (håndterer kun spil-logik)
/// - D: Dependency Inversion (bruges via IGameController-interface i API)
/// </summary>
public class GameController : IGameController
{
    private int _currentPlayerIndex = 0; // Holder styr på hvem der har tur
    private readonly int _totalPlayers; // Antal spillere
    private int? _winnerId = null; // Vinderens ID hvis spillet er slut

    private readonly List<PlayerDto> _players; // Spillere og deres brikker

    /// <summary>
    /// Constructor - Initialiserer spillet med spillere, farver og brikker.
    /// Opfylder Single Responsibility ved kun at sætte starttilstanden.
    /// </summary>
    public GameController(int totalPlayers)
    {
        if (totalPlayers < 2 || totalPlayers > 4)
            throw new ArgumentException("Ludo kræver 2-4 spillere.");

        _totalPlayers = totalPlayers;

        var colors = new[] { "Rød", "Grøn", "Blå", "Gul" };
        _players = new();

        for (int i = 0; i < totalPlayers; i++)
        {
            _players.Add(new PlayerDto
            {
                Id = i,
                Color = colors[i],
                Pieces = Enumerable.Range(0, 4).Select(pid => new PieceDto
                {
                    Id = pid,
                    Position = -1 // Starter i hjem (ikke på brættet)
                }).ToList()
            });
        }
    }

    /// <summary>
    /// Returnerer aktuel spillers ID (0-baseret).
    /// </summary>
    public int GetCurrentPlayer() => _currentPlayerIndex;

    /// <summary>
    /// Skifter tur til næste spiller (rundt i cirkel).
    /// </summary>
    public void NextTurn()
    {
        if (_winnerId != null) return; // Ingen skift hvis der er fundet en vinder

        _currentPlayerIndex = (_currentPlayerIndex + 1) % _totalPlayers;
    }

    /// <summary>
    /// Slår en terning (tilfældig mellem 1-6).
    /// </summary>
    public int RollDice()
    {
        return Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Returnerer nuværende status for brættet: spillere og deres brikker.
    /// </summary>
    public BoardStatusDto GetBoardStatus()
    {
        return new BoardStatusDto
        {
            Players = _players
        };
    }

    /// <summary>
    /// Forsøger at flytte en brik for den aktuelle spiller.
    /// </summary>
    public bool MovePiece(int pieceId, int diceRoll)
    {
        if (_winnerId != null) return false; // Spillet er slut

        var currentPlayer = _players[_currentPlayerIndex];
        var piece = currentPlayer.Pieces.FirstOrDefault(p => p.Id == pieceId);
        if (piece == null) return false; // Brik findes ikke

        if (piece.Position == -1)
        {
            if (diceRoll == 6)
            {
                piece.Position = 0; // Flyt ud fra hjem
            }
            else
            {
                return false; // Kan ikke flytte ud uden en 6'er
            }
        }
        else if (piece.Position >= 0)
        {
            piece.Position += diceRoll; // Flyt fremad
        }
        else
        {
            return false; // Ugyldigt træk
        }

        CheckWinner(); // Tjek for vinder
        return true;
    }

    /// <summary>
    /// Tjekker om en spiller har vundet.
    /// </summary>
    public int? CheckWinner()
    {
        if (_winnerId != null)
            return _winnerId;

        if (_players == null || !_players.Any())
            return null;

        foreach (var player in _players)
        {
            if (player.Pieces.All(p => p.Position >= 100))
            {
                _winnerId = player.Id;
                return _winnerId;
            }
        }

        return null; // Ingen vinder endnu
    }

    /// <summary>
    /// Returnerer ID på vinderen hvis der findes én.
    /// </summary>
    public int? GetWinnerId() => _winnerId;

    /// <summary>
    /// Nulstiller spillet helt: starter forfra.
    /// Bruges af API til at starte et nyt spil uden at genstarte server.
    /// </summary>
    public void Reset()
    {
        _currentPlayerIndex = 0;
        _winnerId = null;

        foreach (var player in _players)
        {
            foreach (var piece in player.Pieces)
            {
                piece.Position = -1; // Sæt alle brikker tilbage i hjem
            }
        }
    }

}
