namespace LudoGame.Api.Dtos;

public class PieceDto
{
    public int Id { get; set; }
    public int Position { get; set; } // -1 = i hjem, 0+ = på brættet
    public int? AbsolutePosition { get; set; }
}

public class PlayerDto
{
    public int Id { get; set; }
    public string Color { get; set; } = string.Empty;
    public List<PieceDto> Pieces { get; set; } = new();
}

public class BoardStatusDto
{
    public List<PlayerDto> Players { get; set; } = new();
    public int CurrentPlayer { get; set; }
    public int? WinnerId { get; set; }
}
