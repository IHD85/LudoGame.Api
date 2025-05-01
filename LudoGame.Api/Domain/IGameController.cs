using LudoGame.Api.Dtos;

namespace LudoGame.Domain;

public interface IGameController
{
    int GetCurrentPlayer();
    void NextTurn();
    int RollDice();
    BoardStatusDto GetBoardStatus();
    bool MovePiece(int pieceId, int diceRoll);
    int? CheckWinner();
    void Reset();
    public bool CanMoveAnyPiece(int playerId, int diceRoll);
    List<int> GetValidMoves(int diceRoll);
    GameStateDto SaveGame();
    void LoadGame(GameStateDto state);
    int DetermineStartingPlayer();
}
