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
        public Dictionary<string, Dictionary<int, double>> getPolicy() {
            var info_states = mccfr.info_states;
            Dictionary<string, Dictionary<int, double>> policy = new Dictionary<string, Dictionary<int, double>>();
            foreach (var info_state in info_states) {
                policy[info_state.Key] = mccfr.action_probs_avg_policy(info_state.Key, (int)info_state.Key[1], new List<int> { 0, 1});
            }
            return policy;
        }
    }
} // end namespace bluffstopCFR