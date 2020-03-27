namespace Chess.Services.Data
{
    using System.Collections.Generic;

    using Chess.Data.Models;
    using Chess.Data.Models.Enums;

    public class ChessEngine : IChessEngine
    {
        public void Start()
        {
            while (true)
            {
                Board board = new Board();
                board.Initialize();

                List<string> playerNames = new List<string>();
                for (int i = 1; i <= 2; i++)
                {
                    // TODO
                    playerNames.Add(null);
                }

                Player player1 = Factory.GetPlayer(playerNames[0].ToUpper(), Color.Light);
                Player player2 = Factory.GetPlayer(playerNames[1].ToUpper(), Color.Dark);

                bool checkmate = false;
                bool stalemate = false;

                while (!checkmate && !stalemate)
                {
                    for (int turn = 1; turn <= 2; turn++)
                    {
                        if (turn % 2 == 1)
                        {
                            if (player1.IsCheckmate)
                            {
                                checkmate = true;
                                break;
                            }

                            if (!player1.IsMoveAvailable)
                            {
                                stalemate = true;
                                break;
                            }

                            // TO DO
                            board.MoveFigure(player1, player2);
                        }
                        else
                        {
                            if (player2.IsCheckmate)
                            {
                                checkmate = true;
                                break;
                            }

                            if (!player2.IsMoveAvailable)
                            {
                                stalemate = true;
                                break;
                            }

                            // TO DO
                            board.MoveFigure(player2, player1);
                        }
                    }
                }
            }
        }
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void Load()
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}
}
