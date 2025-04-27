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
    /// </summary>
    public GameController(int totalPlayers)
    {
        if (totalPlayers < 2 || totalPlayers > 4)
            throw new ArgumentException("Ludo kræver 2-4 spillere.");

        _totalPlayers = totalPlayers;

        var colors = new[] { "Rød", "Grøn", "Blå", "Gul" };
        _players = new List<PlayerDto>();

        for (int i = 0; i < totalPlayers; i++)
        {
            _players.Add(new PlayerDto
            {
                Id = i,
                Color = colors[i],
                Pieces = Enumerable.Range(0, 4).Select(pid => new PieceDto
                {
                    Id = pid,
                    Position = -1 // Starter i hjem
                }).ToList()
            });
        }
    }

    public int GetCurrentPlayer() => _currentPlayerIndex;

    public void NextTurn()
    {
        if (_winnerId != null) return; // Ingen skift hvis der er fundet en vinder

        _currentPlayerIndex = (_currentPlayerIndex + 1) % _totalPlayers;
    }

    public int RollDice()
    {
        return Random.Shared.Next(1, 7);
    }

    public BoardStatusDto GetBoardStatus()
    {
        return new BoardStatusDto
        {
            Players = _players
        };
    }

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
        else
        {
            piece.Position += diceRoll; // Flyt fremad
        }

        CheckWinner(); // Tjek for vinder
        return true;
    }

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

    public void Reset()
    {
        _currentPlayerIndex = 0;
        _winnerId = null;

        foreach (var player in _players)
        {
            foreach (var piece in player.Pieces)
            {
                piece.Position = -1; // Brikker tilbage til hjem
            }
        }
    }
}
