namespace LudoGame.Api.Models
{
    public class Dice
    {
        public int Roll() => Random.Shared.Next(1, 7);
    }
}
