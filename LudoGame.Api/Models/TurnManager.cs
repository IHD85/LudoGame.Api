using LudoGame.Api.Dtos;

namespace LudoGame.Api.Models
{
    public class TurnManager
    {
        public int CurrentPlayerIndex { get; private set; }
        public void NextTurn(List<PlayerDto> players) { /* ... */ }
    }
}
