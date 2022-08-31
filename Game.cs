namespace bluffstopCFR {

    class Game
    {
        public static double MCCFRvsRandomGames(int numGames, MCCFROutcomeSampling mccfr)
        {
            double win_r = 0.0;
            MCCFRPlayer mccfr_player = new MCCFRPlayer(mccfr);
            RandomPlayer random_player = new RandomPlayer();
            
            for (int i = 0; i < numGames; i++)
            {
                int mccfr_id = i % 2;
                win_r += playAGame(mccfr_player, random_player, mccfr_id);
            }
            return win_r / numGames;
        }

        // public static double HumanPlayerMatch(int numGames, Dictionary<string, Node> strategy, int player){
        //     int wins = 0, losses = 0, ties = 0;
        //     CFRPlayer player1;
        //     Player player2;
        //     player1 = new CFRPlayer(strategy);
        //     player2 = new HumanPlayer();
        //     for (int i = 0; i < numGames; i++)
        //     {
        //         int res = playAGame(player1, player2, player);
        //         if (res == 1)
        //             wins++;
        //         else if (res == 0)
        //             ties++;
        //         else
        //             losses++;
        //     }
        //     Console.WriteLine("Player had {0} number of random moves from total of {1} moves", player1.numRandomMoves, player1.numMoves);
        //     return (double)wins / (double)(wins + losses + ties);
        // }

         public static int playAGame(MCCFRPlayer player_mccfr, RandomPlayer ramdom_player, int mccfr_id){
            BluffStop game = new BluffStop();
            while (!game.isTerminal())
            {
                // get valid moves
                Card oppClaimedCard = game.getLastClaimedCard();
                List<string> legal_actions = game.validMoves(oppClaimedCard);

                // player gets action
                int action;
                if (game.currentPlayer == mccfr_id)
                {
                    // get action
                    action = player_mccfr.getAction(game, legal_actions);
                }
                else
                {
                    // get action
                    action = ramdom_player.getAction(game, legal_actions);
                }
                game.applyAction(legal_actions[action]);
            }
            // return the value of the game for the player
            return game.getUtility(mccfr_id);
        }
    }
}