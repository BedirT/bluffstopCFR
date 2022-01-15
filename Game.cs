namespace bluffstopCFR {

    class Game
    {
        public static double RandomPlayerMatch(int numGames, Dictionary<string, Node> strategy, int player)
        {
            int wins = 0, losses = 0, ties = 0;
            CFRPlayer player1; 
            Player player2;
            player1 = new CFRPlayer(strategy);
            player2 = new RandomPlayer();
            
            for (int i = 0; i < numGames; i++)
            {
                int res = playAGame(player1, player2, player);
                if (res == 1)
                    wins++;
                else if (res == 0)
                    ties++;
                else
                    losses++;
            }
            Console.WriteLine("Player had {0} number of random moves from total of {1} moves", player1.numRandomMoves, player1.numMoves);
            return (double)wins / (double)(wins + losses + ties);
        }

        public static double HumanPlayerMatch(int numGames, Dictionary<string, Node> strategy, int player){
            int wins = 0, losses = 0, ties = 0;
            CFRPlayer player1;
            Player player2;
            player1 = new CFRPlayer(strategy);
            player2 = new HumanPlayer();
            for (int i = 0; i < numGames; i++)
            {
                int res = playAGame(player1, player2, player);
                if (res == 1)
                    wins++;
                else if (res == 0)
                    ties++;
                else
                    losses++;
            }
            Console.WriteLine("Player had {0} number of random moves from total of {1} moves", player1.numRandomMoves, player1.numMoves);
            return (double)wins / (double)(wins + losses + ties);
        }

         public static int playAGame(CFRPlayer player1, Player player2, int playerOrder){
            BluffStop game = new BluffStop();
            while (!game.isTerminal())
            {
                // get valid moves
                Card oppClaimedCard = game.getLastClaimedCard();
                List<string> moves = game.validMoves(oppClaimedCard);

                // player gets action
                int action;
                if (game.currentPlayer == 0)
                {
                    // get action
                    action = player1.getAction(game, moves);
                }
                else
                {
                    // get action
                    action = player2.getAction(game, moves);
                }
                game.applyAction(moves[action]);
            }
            // return the value of the game for the player
            return game.getUtility(playerOrder);
        }
    }
}