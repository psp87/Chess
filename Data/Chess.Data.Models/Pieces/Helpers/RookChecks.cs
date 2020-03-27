namespace Chess.Data.Models.Helpers
{
    using System;

    public static class RookChecks
    {
        public static bool Movement(Position toPos, Position fromPos, Square[][] boardMatrix)
        {
            if (toPos.Y != fromPos.Y)
            {
                int rowDifference = Math.Abs((int)fromPos.Y - (int)fromPos.Y) - 1;

                for (int i = 1; i <= rowDifference; i++)
                {
                    int sign = fromPos.Y < toPos.Y ? i : -i;

                    int rowCheck = (int)fromPos.Y + sign;

                    if (boardMatrix[rowCheck][(int)fromPos.X].Piece != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                int colDifference = Math.Abs((int)fromPos.X - (int)toPos.X) - 1;

                for (int i = 1; i <= colDifference; i++)
                {
                    int sign = fromPos.X < toPos.X ? i : -i;

                    int colCheck = (int)fromPos.X + sign;

                    if (boardMatrix[(int)fromPos.Y][colCheck].Piece != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
