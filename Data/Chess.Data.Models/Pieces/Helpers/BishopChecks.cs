namespace Chess.Data.Models.Helpers
{
    using System;

    public static class BishopChecks
    {
        public static bool Movement(Position toPos, Position fromPos, Square[][] boardMatrix)
        {
            int squaresCount = Math.Abs((int)fromPos.Y - (int)toPos.Y) - 1;

            if (toPos.Y < fromPos.Y && toPos.X < fromPos.X)
            {
                if (HelperFunction(-1, -1, fromPos, squaresCount, boardMatrix))
                {
                    return true;
                }
            }

            if (toPos.Y > fromPos.Y && toPos.X > fromPos.X)
            {
                if (HelperFunction(-1, 1, fromPos, squaresCount, boardMatrix))
                {
                    return true;
                }
            }

            if (toPos.Y < fromPos.Y && toPos.X > fromPos.X)
            {
                if (HelperFunction(1, -1, fromPos, squaresCount, boardMatrix))
                {
                    return true;
                }
            }

            if (toPos.Y > fromPos.Y && toPos.X < fromPos.X)
            {
                if (HelperFunction(1, 1, fromPos, squaresCount, boardMatrix))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HelperFunction(int signX, int signY, Position fromPos, int squaresCount, Square[][] boardMatrix)
        {
            for (int i = 1; i <= squaresCount; i++)
            {
                int colCheck = (int)fromPos.X + (signX * i);
                int rowCheck = (int)fromPos.Y + (signY * i);

                if (boardMatrix[colCheck][rowCheck].Piece != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
