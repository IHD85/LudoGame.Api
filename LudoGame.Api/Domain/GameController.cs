using LudoGame.Api.Dtos;
using System.IO.Pipelines;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace LudoGame.Domain;

public class GameController : IGameController
{
    private int _currentPlayerIndex = 0;
    private readonly int _totalPlayers;
    private int? _winnerId = null;
    private readonly List<PlayerDto> _players;
    private int _attemptsThisTurn = 0;
    private readonly int[] _entryToGoal = new[] { 50, 11, 24, 37 };
    private readonly Dictionary<int, int> _startIndices = new()
    {
        { 0, 0 }, { 1, 13 }, { 2, 26 }, { 3, 39 }
    };

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
                Pieces = Enumerable.Range(0, 4).Select(pid => new PieceDto { Id = pid, Position = -1 }).ToList()
            });
        }
    }

    public int GetCurrentPlayer() => _currentPlayerIndex;

    public int RollDice() => Random.Shared.Next(1, 7);

    public void NextTurn()
    {
        _attemptsThisTurn = 0;
        if (_winnerId != null) return;
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _totalPlayers;
        } while (_players[_currentPlayerIndex].Pieces.All(p => p.Position >= 100));
    }


    public BoardStatusDto GetBoardStatus() => new()
    {
        Players = _players,
        CurrentPlayer = _currentPlayerIndex,
        WinnerId = _winnerId
    };

    public bool MovePiece(int pieceId, int diceRoll)
    {
        if (_winnerId != null) return false;

        var currentPlayer = _players[_currentPlayerIndex];
        var piece = currentPlayer.Pieces.FirstOrDefault(p => p.Id == pieceId);
        if (piece == null) return false;

        // Forsøg på at komme ud fra hjem
        if (piece.Position == -1)
        {
            _attemptsThisTurn++;
            if (_attemptsThisTurn <= 3 && diceRoll == 6)
            {
                piece.Position = 0;
                _attemptsThisTurn = 0;
                return true;
            }
            return false;
        }

        // Bevægelse i målzone
        if (piece.Position >= 100)
        {
            if (piece.Position + diceRoll <= 105)
            {
                piece.Position += diceRoll;
                return true;
            }
            return false;
        }

        // Bevægelse mod mål (fra hovedruten)
        int newPos = piece.Position + diceRoll;
        if (piece.Position < 52 && newPos >= _entryToGoal[_currentPlayerIndex])
        {
            int goalStep = newPos - _entryToGoal[_currentPlayerIndex];
            if (goalStep <= 5)
            {
                piece.Position = 100 + goalStep;
                return true;
            }
            return false;
        }

        // Beregn ny relativ og absolut position
        int newRelative = piece.Position + diceRoll;
        int newAbs = GetAbsoluteBoardPosition(_currentPlayerIndex, newRelative);

        // Safe zones må ikke slås hjem
        bool isSafeZone = _startIndices.Values.Contains(newAbs);

        // Slå modstanderes brikker hjem FØR vi flytter
        if (!isSafeZone)
        {
            foreach (var opponent in _players.Where(p => p.Id != _currentPlayerIndex))
            {
                foreach (var op in opponent.Pieces)
                {
                    if (op.Position >= 0 && op.Position < 52)
                    {
                        int opAbs = GetAbsoluteBoardPosition(opponent.Id, op.Position);
                        if (opAbs == newAbs)
                        {
                            op.Position = -1; // slå hjem
                        }
                    }
                }
            }
        }

        // Flyt spillerens brik
        piece.Position = newRelative;
        return true;
    }




    public void SetPiecePosition(int playerId, int pieceId, int relativePosition)
    {
        var piece = _players[playerId].Pieces.First(p => p.Id == pieceId);
        piece.Position = relativePosition;
    }






    private int GetAbsolutePosition(int relativePosition, int playerIndex)
    {
        return (_startIndices[playerIndex] + relativePosition) % 52;
    }

    private bool IsStartPosition(int position)
    {
        return _startIndices.Values.Contains(position);
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
        _attemptsThisTurn = 0;
        foreach (var player in _players)
        {
            foreach (var piece in player.Pieces)
            {
                piece.Position = -1;
            }
        }
    }

    public bool CanMoveAnyPiece(int playerId, int diceRoll)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return false;

        foreach (var piece in player.Pieces)
        {
            if (piece.Position == -1 && diceRoll == 6) return true;
            if (piece.Position >= 0 && piece.Position < 100) return true;
            if (piece.Position >= 100 && piece.Position + diceRoll <= 105) return true;
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
            else if (piece.Position >= 0 && piece.Position < 100)
                validPieceIds.Add(piece.Id);
            else if (piece.Position >= 100 && piece.Position + diceRoll <= 105)
                validPieceIds.Add(piece.Id);
        }
        return validPieceIds;
    }

    public GameStateDto SaveGame() => new()
    {
        CurrentPlayer = _currentPlayerIndex,
        WinnerId = _winnerId,
        Players = _players
    };

    public void LoadGame(GameStateDto state)
    {
        if (state.Players == null || state.Players.Count == 0)
            throw new InvalidOperationException("Spillet kan ikke loades uden spillere.");

        _players.Clear();
        _players.AddRange(state.Players);
        _currentPlayerIndex = state.CurrentPlayer;
        _winnerId = state.WinnerId;
    }

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
                if (roll > highestRoll) highestRoll = roll;
            }

            contenders = rolls.Where(kv => kv.Value == highestRoll).Select(kv => kv.Key).ToList();
        }

        _currentPlayerIndex = contenders.First();
        return _currentPlayerIndex;
    }

    private bool CanMoveAnyPieceForCurrentPlayer(int diceRoll)
    {
        var player = _players[_currentPlayerIndex];
        foreach (var piece in player.Pieces)
        {
            if (piece.Position == -1 && diceRoll == 6) return true;
            if (piece.Position >= 0 && piece.Position < 100) return true;
            if (piece.Position >= 100 && piece.Position + diceRoll <= 105) return true;
        }
        return false;
    }

    public void HandleRollResult(int diceRoll)
    {
        if (!CanMoveAnyPieceForCurrentPlayer(diceRoll) && diceRoll != 6)
        {
            NextTurn();
        }
    }

    public int GetAbsoluteBoardPosition(int playerId, int relativePos)
    {
        return (_startIndices[playerId] + relativePos) % 52;
    }

}
