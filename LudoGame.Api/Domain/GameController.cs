using LudoGame.Api.Dtos;
using System.IO.Pipelines;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;


namespace LudoGame.Domain;

/// <summary>
/// GameController styrer spillets gang: aktuel spiller, terningekast, flytning og vindertjek.
/// Opbygget efter SOLID-principper:
/// - S: Single Responsibility (håndterer kun spil-logik)
/// - D: Dependency Inversion (bruges via IGameController-interface i API)
/// </summary>
public class GameController : IGameController
{
    private int _currentPlayerIndex = 0;
    private readonly int _totalPlayers;
    private int? _winnerId = null;
    private readonly List<PlayerDto> _players;
    private int _attemptsThisTurn = 0; // Antal forsøg fra hjem i denne tur

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
                    Position = -1
                }).ToList()
            });
        }
    }

    public int GetCurrentPlayer() => _currentPlayerIndex;

    /// <summary>
    /// Går videre til næste spiller.
    /// Skipper spillere der har vundet (alle brikker i mål).
    /// Hvis kun én spiller er tilbage, afsluttes spillet automatisk.
    /// </summary>

    /// <summary>
    /// Slår en terning og returnerer et tal mellem 1 og 6.
    /// Bruges både i spillets gang og i test.
    /// </summary>
    public int RollDice()
    {
        return Random.Shared.Next(1, 7);
    }

    /// <summary>
    /// Går videre til næste spiller.
    /// Skipper spillere der har vundet (alle brikker i mål).
    /// Hvis kun én spiller er tilbage, sættes de automatisk som vinder.
    /// </summary>
    public void NextTurn()
    {
        _attemptsThisTurn = 0; // 🔁 Nulstil forsøg ved ny tur
        if (_winnerId != null) return; // Spillet er slut

        int activePlayers = _players.Count(p => !HasPlayerWon(p));

        // ✅ Hvis kun én spiller er tilbage og ingen vinder sat endnu
        if (activePlayers <= 1 && _winnerId == null)
        {
            var remaining = _players.FirstOrDefault(p => !HasPlayerWon(p));
            if (remaining != null)
                _winnerId = remaining.Id;

            return;
        }

        // ✅ Find næste aktive spiller
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _totalPlayers;
        }
        while (HasPlayerWon(_players[_currentPlayerIndex]));
    }


    /// <summary>
    /// Tjekker om en spiller har vundet (alle brikker i mål og ikke i hjem).
    /// Bruges internt til skip-logik i NextTurn.
    /// </summary>
    private bool HasPlayerWon(PlayerDto player)
    {
        return player.Pieces.All(p => p.Position >= 100) && player.Pieces.All(p => p.Position != -1);
    }



    public BoardStatusDto GetBoardStatus()
    {
        return new BoardStatusDto { Players = _players };
    }

    public bool MovePiece(int pieceId, int diceRoll)
    {
        if (_winnerId != null) return false;

        var currentPlayer = _players[_currentPlayerIndex];
        var piece = currentPlayer.Pieces.FirstOrDefault(p => p.Id == pieceId);
        if (piece == null) return false;

        if (piece.Position == -1)
        {
            _attemptsThisTurn++; // 📌 Tæl hvert forsøg fra hjem

            if (_attemptsThisTurn <= 3 && diceRoll == 6)
            {
                piece.Position = 0;
                _attemptsThisTurn = 0; // Nulstil ved succes
                CheckWinner();
                return true;
            }

            return false; // Kan ikke komme ud
        }
        else if (piece.Position >= 0)
        {
            piece.Position += diceRoll;

            // ✅ REGEL: Slå modstander hjem
            foreach (var opponent in _players.Where(p => p.Id != currentPlayer.Id))
            {
                var hitPiece = opponent.Pieces.FirstOrDefault(p => p.Position == piece.Position);
                if (hitPiece != null)
                {
                    hitPiece.Position = -1;
                }
            }

            CheckWinner();
            return true;
        }

        return false;
    }

    public int? CheckWinner()
    {
        if (_winnerId != null) return _winnerId;

        foreach (var player in _players)
        {
            bool allPiecesReachedGoal = player.Pieces.All(p => p.Position >= 100);
            bool noPiecesInHome = player.Pieces.All(p => p.Position != -1);

            if (allPiecesReachedGoal && noPiecesInHome)
            {
                _winnerId = player.Id;
                return _winnerId;
            }
        }

        return null;
    }

    public void Reset()
    {
        _currentPlayerIndex = 0;
        _winnerId = null;
        foreach (var player in _players)
        {
            foreach (var piece in player.Pieces)
                piece.Position = -1;
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

    public List<int> GetValidMoves(int diceRoll)
    {
        var validPieceIds = new List<int>();
        var currentPlayer = _players[_currentPlayerIndex];

        foreach (var piece in currentPlayer.Pieces)
        {
            if (piece.Position == -1 && diceRoll == 6)
                validPieceIds.Add(piece.Id);
            else if (piece.Position >= 0)
                validPieceIds.Add(piece.Id);
        }

        return validPieceIds;
    }

    public GameStateDto SaveGame()
    {
        return new GameStateDto
        {
            CurrentPlayer = _currentPlayerIndex,
            WinnerId = _winnerId,
            Players = _players
        };
    }

    public void LoadGame(GameStateDto state)
    {
        if (state.Players == null || state.Players.Count == 0)
            throw new InvalidOperationException("Spillet kan ikke loades uden spillere.");

        _players.Clear();
        _players.AddRange(state.Players);
        _currentPlayerIndex = state.CurrentPlayer;
        _winnerId = state.WinnerId;
    }

    /// <summary>
    /// ✅ NYT: Find startspiller – højeste slag starter, lighed → ny runde kun for dem
    /// SOLID: SRP + testbar metode. Matcher regler og eksamenskrav.
    /// </summary>
    public int DetermineStartingPlayer()
    {
        var contenders = _players.Select(p => p.Id).ToList();

        while (contenders.Count > 1)
        {
            var rolls = new Dictionary<int, int>();
            int highestRoll = 0;

            foreach (var id in contenders)
            {
                int roll = Random.Shared.Next(1, 7);
                rolls[id] = roll;
                if (roll > highestRoll)
                    highestRoll = roll;
            }

            contenders = rolls
                .Where(kv => kv.Value == highestRoll)
                .Select(kv => kv.Key)
                .ToList();
        }

        _currentPlayerIndex = contenders.First();
        return _currentPlayerIndex;
    }

    /// <summary>
    /// Tjekker om aktuel spiller kan rykke mindst én brik med given slag.
    /// </summary>
    private bool CanMoveAnyPieceForCurrentPlayer(int diceRoll)
    {
        var player = _players[_currentPlayerIndex];
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
    /// Behandler resultatet af et kast, uden at flytte.
    /// Hvis ingen brikker kan rykke, og slaget ikke er 6 → skift tur.
    /// </summary>
    public void HandleRollResult(int diceRoll)
    {
        if (!CanMoveAnyPieceForCurrentPlayer(diceRoll) && diceRoll != 6)
        {
            NextTurn(); // ❌ Ingen mulighed for ryk → mister tur
        }
        // 🎲 Hvis man slog en 6’er, får man slå igen (intet sker her)
    }

}

