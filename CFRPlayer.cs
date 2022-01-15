using System;

namespace bluffstopCFR
{
    public class CFRPlayer : Player {
        Random rnd = new Random();
        int numRandomMoves = 0;
        public CFRPlayer(Dictionary<string, Node> strategy) : base(strategy) { 
            this.strategy = strategy;
        }
        public override int getAction(BluffStop game, List<string> moves) {
            // get the action
            string infoState = game.getInfoState();
            // infostate is not in the keys act randomly
            if (!strategy.ContainsKey(infoState)) {
                numRandomMoves++;
                return rnd.Next(moves.Count);
            }
            // use strategy[infoState].getAverageStrategy() to get the average strategy
            double[] averageStrategy = strategy[infoState].getAverageStrategy();
            // sample an action using the probability distribution of the average strategy
            // get a random number between 0 and 1
            double r = rnd.NextDouble();
            // get the action corresponding to the random number
            int action = 0;
            while (r > averageStrategy[action]) {
                r -= averageStrategy[action];
                action++;
            }
            return action;
        }
        public override Player clone() {
            return new CFRPlayer(this.strategy);
        }
    }
} // end namespace bluffstopCFR