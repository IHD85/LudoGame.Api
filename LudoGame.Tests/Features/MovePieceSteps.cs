using LudoGame.Domain;
using Reqnroll;
using Xunit;

namespace LudoGame.Tests.Features
{
    [Binding]
    public class MovePieceSteps
    {
        private GameController? _game;
        private int _pieceId;
        private int _opponentPieceId;
        private int _diceRoll;
        private bool _moveResult;

        private int GetAbsolutePosition(int relative, int playerId)
        {
            var startIndices = new[] { 0, 13, 26, 39 };
            return (startIndices[playerId] + relative) % 52;
        }

        [Given("en spiller har en brik i hjemmet")]
        public void GivenEnSpillerHarEnBrikIHjemmet()
        {
            _game = new GameController(2);
            _pieceId = 0;
        }

        [When("spilleren slår en 6'er")]
        public void WhenSpillerenSlaarEn6er()
        {
            _diceRoll = 6;
            _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
        }

        [When("spilleren slår en 4'er")]
        public void WhenSpillerenSlaarEn4er()
        {
            _diceRoll = 4;
            _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
        }

        [Then("brikken flyttes ud på startfeltet")]
        public void ThenBrikkenFlyttesUdPaaStartfeltet()
        {
            var board = _game!.GetBoardStatus();
            var piece = board.Players[0].Pieces.First(p => p.Id == _pieceId);
            Assert.Equal(0, piece.Position);
        }

        [Then("brikken forbliver i hjemmet")]
        public void ThenBrikkenForbliverIHjemmet()
        {
            var board = _game!.GetBoardStatus();
            var piece = board.Players[0].Pieces.First(p => p.Id == _pieceId);
            Assert.Equal(-1, piece.Position);
        }

        [Given("en spiller har en brik på felt 0")]
        public void GivenEnSpillerHarEnBrikPaaFelt0()
        {
            _game = new GameController(2);
            _pieceId = 0;
            _game.MovePiece(_pieceId, 6);
        }

        [When("spilleren slår en 3'er")]
        public void WhenSpillerenSlaarEn3er()
        {
            _diceRoll = 3;
            _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
        }

        [Then("brikken flyttes til felt 3")]
        public void ThenBrikkenFlyttesTilFelt3()
        {
            var board = _game!.GetBoardStatus();
            var piece = board.Players[0].Pieces.First(p => p.Id == _pieceId);
            Assert.Equal(3, piece.Position);
        }

        [Given("en spiller har en brik på felt 5")]
        public void GivenEnSpillerHarEnBrikPaaFelt5()
        {
            _game = new GameController(2);
            _pieceId = 0;
            _game.MovePiece(_pieceId, 6);
            _game.MovePiece(_pieceId, 5);
        }

        [Given(@"en modstander har en brik på felt 8")]
        public void GivenEnModstanderHarEnBrikPaaFelt8()
        {
            _game!.NextTurn(); // Til spiller 1
            _opponentPieceId = 0;

            _game.MovePiece(_opponentPieceId, 6);   // Ud → rel = 0
            _game.MovePiece(_opponentPieceId, 47);  // 13 + 47 = 60 → %52 = 8 (abs)

            var board = _game.GetBoardStatus();
            var opponentPiece = board.Players[1].Pieces.First(p => p.Id == _opponentPieceId);

            int abs = _game.GetAbsoluteBoardPosition(1, opponentPiece.Position);
            Assert.Equal(8, abs); // Bekræft at brikken landede på absolut felt 8
        }



        [Then("modstanderens brik flyttes hjem")]
        public void ThenModstanderensBrikFlyttesHjem()
        {
            var board = _game!.GetBoardStatus();
            var opponentPiece = board.Players[1].Pieces.First(p => p.Id == _opponentPieceId);
            Assert.Equal(-1, opponentPiece.Position);
        }

        [Then("spillerens brik flyttes til felt 8")]
        public void ThenSpillerensBrikFlyttesTilFelt8()
        {
            var board = _game!.GetBoardStatus();
            var piece = board.Players[0].Pieces.First(p => p.Id == _pieceId);
            int abs = GetAbsolutePosition(piece.Position, 0);
            Assert.Equal(8, abs);
        }

        [Given("en spiller prøver at flytte en ugyldig brik")]
        public void GivenEnSpillerProeverAtFlytteEnUgyldigBrik()
        {
            _game = new GameController(2);
            _pieceId = 99;
        }

        [When("handlingen udføres")]
        public void WhenHandlingenUdfoeres()
        {
            _diceRoll = 6;
            _moveResult = _game!.MovePiece(_pieceId, _diceRoll);
        }

        [Then("spillets tilstand ændres ikke")]
        public void ThenSpilletsTilstandAEndresIkke()
        {
            Assert.False(_moveResult);
        }
    }
}
