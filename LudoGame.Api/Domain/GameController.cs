using LudoGame.Api.Dtos;
using System.IO.Pipelines;
using System.Numerics;



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
        if (_winnerId != null) return;

        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _totalPlayers;
        }
        while (_players[_currentPlayerIndex].Pieces.All(p => p.Position >= 100));
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
                CheckWinner();
                return true;
            }
            else
            {
                return false; // Kan ikke flytte ud uden en 6'er
            }
        }
        else if (piece.Position >= 0)
        {
            piece.Position += diceRoll;

            // 🆕 Tjek om vi lander på en modstander!
            foreach (var opponent in _players.Where(p => p.Id != currentPlayer.Id))
            {
                var hitPiece = opponent.Pieces.FirstOrDefault(p => p.Position == piece.Position);
                if (hitPiece != null)
                {
                    hitPiece.Position = -1; // Slår modstanders brik hjem
                }
            }

            CheckWinner();
            return true;
        }
        else
        {
            return false; // Ugyldigt træk
        }
    }







    public int? CheckWinner()
    {
        if (_winnerId != null)
            return _winnerId;

        foreach (var player in _players)
        {
            // Kun vundet hvis alle 4 brikker er ikke i hjem OG i mål
            bool allPiecesReachedGoal = player.Pieces.All(p => p.Position >= 100);
            bool noPiecesInHome = player.Pieces.All(p => p.Position != -1);

            if (allPiecesReachedGoal && noPiecesInHome)
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

    public bool CanMoveAnyPiece(int playerId, int diceRoll)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return false;

        foreach (var piece in player.Pieces)
        {
            if (piece.Position == -1 && diceRoll == 6)
                return true;
            if (piece.Position >= 0 && piece.Position + diceRoll <= 100)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returnerer ID'er på brikker der kan flyttes med givet terningeslag.
    /// </summary>
    public List<int> GetValidMoves(int diceRoll)
    {
        var validPieceIds = new List<int>();

        var currentPlayer = _players[_currentPlayerIndex];
        foreach (var piece in currentPlayer.Pieces)
        {
            if (piece.Position == -1)
            {
                if (diceRoll == 6)
                    validPieceIds.Add(piece.Id); // Må kun flytte ud med 6
            }
            else if (piece.Position >= 0)
            {
                validPieceIds.Add(piece.Id); // Kan altid flytte på brættet
            }
        }

        return validPieceIds;
    }

    // 🆕 Gemmer hele spillets tilstand
    public GameStateDto SaveGame()
    {
        return new GameStateDto
        {
            CurrentPlayer = _currentPlayerIndex,
            WinnerId = _winnerId,
            Players = _players
        };
    }

    // 🆕 Loader en gemt spiltilstand
    public void LoadGame(GameStateDto state)
    {
        if (state.Players == null || state.Players.Count == 0)
            throw new InvalidOperationException("Spillet kan ikke loades uden spillere.");

        _players.Clear();
        _players.AddRange(state.Players); // Brug PlayerDto direkte

        _currentPlayerIndex = state.CurrentPlayer;
        _winnerId = state.WinnerId;
    }

}
