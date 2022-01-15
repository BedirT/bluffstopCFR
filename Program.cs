// See https://aka.ms/new-console-template for more information
namespace bluffstopCFR{
    public class GameTest
    {
        // main function for testing
        public static void Main(String[] args)
        {
            // TestGame();
            int print_freq = 10000;
            Dictionary<string, Node> nodeMap = TrainCFR(1, print_freq);
            Console.WriteLine("Done training");
            Console.WriteLine("Number of nodes: " + nodeMap.Count);
            // double win_r = Game.HumanPlayerMatch(5, nodeMap, 1);
            double win_r = Game.RandomPlayerMatch(100, nodeMap, 0);
            Console.WriteLine("End win rate: {0}", win_r);
            // Report the number of random moves
            
        }

        public static Dictionary<string, Node> TrainCFR(int numIterations, int print_freq)
        {
            CFRTrainer trainer = new CFRTrainer(print_freq);
            trainer.train(numIterations);
            // Get the strategy from trainer
            Dictionary<string, Node> nodeMap = trainer.getNodeMap();
            // Todo: Save the strategy to a file
            // make sure its reusable
            return nodeMap;
        }
    }
}