namespace bluffstopCFR
{
    public class MCCFRPlayer {
        Random rnd = new Random();
        public int numRandomMoves = 0;
        public int numMoves = 0;
        public MCCFROutcomeSampling mccfr = new MCCFROutcomeSampling();
        public MCCFRPlayer(MCCFROutcomeSampling mccfr) { 
            this.mccfr = mccfr;
        }
        public int getAction(KuhnPoker game, List<int> legal_actions) {
            // get the action
            string infoState = game.getInfoState();
            Dictionary<int,double> policy = mccfr.action_probs_avg_policy(infoState, game.currentPlayer, legal_actions);
            double marker = rnd.NextDouble();
            double sum_probs = 0;
            for (int action_idx = 0; action_idx < legal_actions.Count; action_idx++) {
                sum_probs += policy[legal_actions[action_idx]];
                if (sum_probs >= marker) {
                    return action_idx;
                }
            }
            // should never reach here
            return -1;
        }
    }
} // end namespace bluffstopCFR