// See https://aka.ms/new-console-template for more information
namespace bluffstopCFR{
    public class GameTest
    {
        // main function for testing
        public static void Main(String[] args)
        {
            // TestGame();
            Dictionary<string, Node> nodeMap = TrainCFR(100);
            RandomPlayerMatch(1000, nodeMap);
        }

        public static Dictionary<string, Node> TrainCFR(int numIterations)
        {
            CFRTrainer trainer = new CFRTrainer();
            trainer.train(numIterations);
            // Get the strategy from trainer
            Dictionary<string, Node> nodeMap = trainer.getNodeMap();
            // Todo: Save the strategy to a file
            // make sure its reusable
            return nodeMap;
        }

        public static void RandomPlayerMatch(int numGames, Dictionary<string, Node> strategy)
        {
            // Play agains random opponent
            int numRounds = 100;
            int wins = 0, losses = 0, ties = 0;
            Player player1 = new CFRPlayer(strategy);
            Player player2 = new RandomPlayer();

            for (int i = 0; i < numGames; i++)
            {
                int playerValue = 0;
                int opponentValue = 0;
                for (int r = 0; r < numRounds; ++r){
                    int roundValue = playAGame(player1, player2);
                    if (roundValue > 0)
                        playerValue += roundValue;
                    else
                        opponentValue += roundValue;
                }
                if (playerValue > opponentValue)
                    wins++;
                else if (playerValue < opponentValue)
                    losses++;
                else
                    ties++;
            }
            Console.WriteLine("Wins: " + wins + " Losses: " + losses + " Ties: " + ties);
            Console.WriteLine("Win rate: " + (double)wins / (double)(wins + losses + ties));
        }

        public static int playAGame(Player player1, Player player2){
            Random rnd = new Random();
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
            // return the value of the game for player 1
            return game.getUtility(0);
        }
    }

}