using System;
using System.Collections.Generic;

namespace bluffstopCFR
{
    public class Node                  // They didn't do this public but it doesnt work when the Node class is private NEED HELP
    {
        public string infoState;
        public int numActions;
        public double[] regretSum, strategy, strategySum;
        List<string> validActions;
        public Node (string infoState, int numActions, List<string> validActions) {
            this.infoState = infoState;
            this.numActions = numActions;
            this.validActions = validActions;
            this.regretSum = new double[numActions];
            this.strategy = new double[numActions];
            this.strategySum = new double[numActions];
        }
        // Get current information set mixed strategy through regret-matching
        public double[] getStrategy(double realizationWeight)
        {
            double normalizingSum = 0;                      // a double we have created to be able to "normalize" our strategy regrets, normalizing in this context means that we ensure the array entries sum up to 1 which means we can treat them like probabilities 
            for (int a = 0; a < numActions; a++)
            {
                strategy[a] = regretSum[a] > 0 ? regretSum[a] : 0;   // regret matching formula - max(0, regretSum[a])
                normalizingSum += strategy[a];             // updating normalizingSum so it is equal total "regretSum"s
            }
            for (int a = 0; a < numActions; a++)
            {
                if (normalizingSum > 0)                     // Checking if our normalizingSum is above 0,
                    strategy[a] /= normalizingSum;          // if so we divide strategy[a] with normalizingSum
                else
                    strategy[a] = 1.0 / (numActions);      // if normalizingSum isn't above 0 we update strategy[a]'s value to be equal to 1 / numActions 
                strategySum[a] += realizationWeight * strategy[a];              // after we check and update values with our normalizingSum to make sure we can use them as probabilities we add strategy[a] to our strategySum[a]
            }
            return strategy;                                // returning current strategy
        }
        public double[] getAverageStrategy()
        {
            /*
            Get average information set mixed strategy across all training iterations
            we do this because , the regrets may be temporarily skewed in such a way that an important strategy
            in the mix has a negative regret sum and would never be chosen
            we do the same thing we have done above with the exception of us not needing to worry about negative values
            */
            double[] avgStrategy = new double[numActions];              // creating new double array for average strategy
            double normalizingSum = 0;                                  // normalizingSum for making sure we can treat regrets as probabilities
            for (int a = 0; a < numActions; a++)   {
                normalizingSum += strategySum[a];  
            }                     // repeating until we ego through every possible action
                                     // updating normalizingSum with strategySum[a]
            for (int a = 0; a < numActions; a++)                        // repeating until we ego through every possible action
                if (normalizingSum > 0)                                 // checking if it's the first time we're running this
                    avgStrategy[a] = strategySum[a] / normalizingSum;   // if it isn't the first time we're running the program we we set the avgStrategy[a] regret value as strategySum[a] / normalizingSum
                else
                    avgStrategy[a] = 1.0 / numActions;                  // if it is the first time we're running the program we set avg strategy as 1.0 /  numActions  
            return avgStrategy;
        }
        // Get information set string representation
        public override string ToString()
        {
            return string.Format("{0}:\t{1}", this.infoState, string.Join(" ", getAverageStrategy()));
        }
    }

    class CFRTrainer
    {
        public static Random random = new Random();
        public static Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();
        public int eval_freq = 100;
        public CFRTrainer(int eval_freq=100){
            this.eval_freq = eval_freq;
        }
        public void train(int iterations)
        {
            double util = 0;
            using (var progress = new ProgressBar()) {
                for (int i = 0; i < iterations; i++)
                {
                    progress.Report((double) i / iterations);
                    BluffStop game = new BluffStop();
                    util += cfr(game, 1, 1);
                    if (i % eval_freq == 0)
                    {
                        // Get win rate on 50 random games with p1 and 50 p2
                        // double win_r = Game.RandomPlayerMatch(50, nodeMap, 1);
                        // win_r += Game.RandomPlayerMatch(50, nodeMap, 0);
                        // Report win rate
                        // Console.WriteLine("Iteration {0}: {1}", i, win_r / 2);
                    }
                }
            }
            
            Console.WriteLine("Average game value: " + util / iterations);
        }

        public Dictionary<string, Node> getNodeMap()
        {
            return nodeMap;
        }

        // Counterfactual regret minimization iteration
        private double cfr(BluffStop game, double p0, double p1)
        {
            int cur_player = game.currentPlayer;
            if (game.isTerminal())
                return game.getUtility();
            Card lastClaimedCard = game.getLastClaimedCard();
            List<string> validMoves = game.validMoves(lastClaimedCard);
            int numActions = game.numValidActions;
            string infoState = game.getInfoState();
            Node node;
            if (!nodeMap.ContainsKey(infoState))
            {
                node = new Node(infoState, numActions, validMoves);
                nodeMap.Add(infoState, node);
            }
            else {
                node = nodeMap[infoState];
            }
            // For each action, recursively call cfr with additional history and probability
            double[] strategy = node.getStrategy(cur_player == 0 ? p0 : p1);
            double[] util = new double[node.numActions];

            double nodeUtil = 0;
            for (int a = 0; a < validMoves.Count; ++a)
            {
                BluffStop new_game = game.clone();
                // print the actions
                // Console.WriteLine(validMoves[a]);
                
                new_game.applyAction(validMoves[a]);
                
                util[a] = cur_player == 0
                                        ? -cfr(new_game, p0 * strategy[a], p1)
                                        : -cfr(new_game, p0, p1 * strategy[a]);
                nodeUtil += strategy[a] * util[a];
            }
            //For each action, compute and accumulate counterfactual regret
            for (int a = 0; a < validMoves.Count; a++)
            {
                double regret = util[a] - nodeUtil;
                node.regretSum[a] += (cur_player == 0 ? p1 : p0) * regret;
            }
            return nodeUtil;
        }
    
    }
    
    
} // end namespace