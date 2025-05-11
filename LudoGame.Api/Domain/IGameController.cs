using LudoGame.Api.Dtos;
using LudoGame.Domain.Enums;

namespace LudoGame.Domain;

public interface IGameController // 💬 DIP: GameControllerApi afhænger af IGameController (interface) – ikke den konkrete GameController

{
    int GetCurrentPlayer();
    void NextTurn();
    int RollDice();
    BoardStatusDto GetBoardStatus();
    MoveResult MovePiece(int pieceId, int diceRoll);

    int? CheckWinner();
    void Reset();
    public bool CanMoveAnyPiece(int playerId, int diceRoll);
    List<int> GetValidMoves(int diceRoll);
    GameStateDto SaveGame();
    void LoadGame(GameStateDto state);
    int DetermineStartingPlayer();

}
