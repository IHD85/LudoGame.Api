using LudoGame.Api.Dtos;

namespace LudoGame.Api.Models
{
    public class Move
    {
        /// <summary>
        /// Attempts to move a piece for the given player.
        /// </summary>
        /// <param name="player">The player making the move.</param>
        /// <param name="pieceId">The ID of the piece to move.</param>
        /// <param name="diceRoll">The dice roll value.</param>
        /// <param name="allPlayers">All players in the game (for hit detection).</param>
        /// <returns>True if the move was successful, false otherwise.</returns>
        public bool TryMovePiece(PlayerDto player, int pieceId, int diceRoll, List<PlayerDto> allPlayers)
        {
            var piece = player.Pieces.FirstOrDefault(p => p.Id == pieceId);
            if (piece == null)
                return false;

            // If piece is at home
            if (piece.Position == -1)
            {
                if (diceRoll == 6)
                {
                    piece.Position = 0; // Move out of home
                    return true;
                }
                return false; // Can't move out without a 6
            }
            // If piece is on the board
            if (piece.Position >= 0)
            {
                int newPosition = piece.Position + diceRoll;
                if (newPosition > 100)
                    return false; // Can't move beyond goal

                piece.Position = newPosition;

                // Check for hitting opponent's piece
                foreach (var opponent in allPlayers.Where(p => p.Id != player.Id))
                {
                    var hitPiece = opponent.Pieces.FirstOrDefault(p => p.Position == piece.Position);
                    if (hitPiece != null)
                    {
                        hitPiece.Position = -1; // Send opponent's piece home
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of piece IDs that can be moved with the given dice roll.
        /// </summary>
        public List<int> GetValidMoves(PlayerDto player, int diceRoll)
        {
            var validPieceIds = new List<int>();
            foreach (var piece in player.Pieces)
            {
                if (piece.Position == -1 && diceRoll == 6)
                    validPieceIds.Add(piece.Id);
                else if (piece.Position >= 0 && piece.Position + diceRoll <= 100)
                    validPieceIds.Add(piece.Id);
            }
            return validPieceIds;
        }
    }
}