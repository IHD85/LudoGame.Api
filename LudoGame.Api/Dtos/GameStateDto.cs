// Sti: LudoGame.Api/Dtos/GameStateDto.cs

namespace LudoGame.Api.Dtos
{
    public class GameStateDto
    {
        public int CurrentPlayer { get; set; }
        public int? WinnerId { get; set; }
        public List<PlayerDto> Players { get; set; } = new();
    }
}
