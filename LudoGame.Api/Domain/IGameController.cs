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
}
