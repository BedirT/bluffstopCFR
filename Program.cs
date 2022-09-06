using Numpy;
using Python.Runtime;

namespace bluffstopCFR{
    public class GameTest
    {
        // main function for testing
        public static void Main(String[] args)
        {
            // initialize an array with random integers
            // var a = np.random.randn(3);
            // var sum = np.sum(a);
            // // var a_data = ;
            // Console.WriteLine("a_data: {0}", sum.repr);
            MCCFROutcomeSampling mccfr = TrainMCCFR(500000, 10000);
            Console.WriteLine("Done training");
            double win_r = GamePlay.MCCFRvsRandomGames(10000, mccfr);
            Console.WriteLine("End game win rate: {0}", win_r);
        }

        // public static Dictionary<string, Node> TrainCFR(int numIterations, int print_freq)
        // {
        //     CFRTrainer trainer = new CFRTrainer(print_freq);
        //     trainer.train(numIterations);
        //     // Get the strategy from trainer
        //     Dictionary<string, Node> nodeMap = trainer.getNodeMap();
        //     // Todo: Save the strategy to a file
        //     // make sure its reusable
        //     return nodeMap;
        // }

        public static MCCFROutcomeSampling TrainMCCFR(int numIterations, int print_freq)
        {
            MCCFROutcomeSampling mccfr = new MCCFROutcomeSampling();

            for (int i = 0; i < numIterations; i++)
            {
                mccfr.iteration();
                if (i % print_freq == 0)
                {   
                    double win_r = GamePlay.MCCFRvsRandomGames(10000, mccfr);
                    // Report win rate
                    Console.WriteLine("Iteration {0} / {1}: {2}", i, numIterations, win_r);
                }
            }

            return mccfr;
        }
    }
}