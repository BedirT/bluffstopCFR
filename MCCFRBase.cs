namespace bluffstopCFR {
    static class Index {
        public static int Regret = 0;
        public static int AvgPolicy = 1;
    }
    public class MCCFROutcomeSampling {
        // init
        public Dictionary<string, List<List<double>>> info_states;
        public double eps;
        public double exploration_constant = 0.6;
        public MCCFROutcomeSampling() {
            info_states = new Dictionary<string, List<List<double>>>();
            eps = 1e6;
        }
        //
        public List<List<double>> info_state_lookup(string info_state, int num_legal_actions){
            // return the list for the info_state from the info_states dictionary
            // if exists. Otherwise create a uniform policy.
            if (info_states.ContainsKey(info_state)) {
                return info_states[info_state];
            } 
            List<double> avg_policy = new List<double>();
            List<double> regret = new List<double>();
            // setup the policy
            for (int i = 0; i < num_legal_actions; i++) {
                avg_policy.Add(1.0 / eps);
                regret.Add(1.0 / eps);
            }
            // add to the info_states dictionary
            info_states.Add(info_state, new List<List<double>> { regret, avg_policy });
            return info_states[info_state];
        }
        //
        public void add_regret(string info_state, int action, double amount){
            info_states[info_state][Index.Regret][action] += amount;
        }
        //
        public void add_avg_policy(string info_state, int action, double amount){
            info_states[info_state][Index.AvgPolicy][action] += amount;
        }
        //
        public List<double> regret_matching(List<double> regrets, int num_legal_actions){
            List<double> positive_regrets = new List<double>();
            // Get positive regrets
            for (int i = 0; i < num_legal_actions; i++) {
                if (regrets[i] > 0) { positive_regrets.Add(regrets[i]); }
                else                { positive_regrets.Add(0.0); }
            }
            // Get regret sum
            double positive_regret_sum = 0.0;
            for (int i = 0; i < num_legal_actions; i++) {
                positive_regret_sum += positive_regrets[i];
            }
            List<double> avg_regret = new List<double>();
            // return avg regret
            if (positive_regret_sum <= 0.0) {
                for (int i = 0; i < num_legal_actions; i++) {
                    avg_regret.Add(1.0 / num_legal_actions);
                }
            }
            else {
                for (int i = 0; i < num_legal_actions; i++) {
                    avg_regret.Add(positive_regrets[i] / positive_regret_sum);
                }
            }
            return avg_regret;
        }
        // action_probs
        public Dictionary<string, double> action_probs_avg_policy(string info_state, int player, List<string> legal_actions){
            // For the given player, return the action probabilities for the given info_state
            Dictionary<string, double> action_probs = new Dictionary<string, double>();
            if (info_states.ContainsKey(info_state)) {
                List<double> avg_policy = info_states[info_state][Index.AvgPolicy];
                double avg_policy_sum = 0.0;
                for (int i = 0; i < avg_policy.Count; i++) {
                    avg_policy_sum += avg_policy[i];
                }
                for (int i = 0; i < legal_actions.Count; i++) {
                    action_probs.Add(legal_actions[i], avg_policy[i] / avg_policy_sum);
                }
            }
            else {
                // return uniform policy
                for (int i = 0; i < legal_actions.Count; i++) {
                    action_probs.Add(legal_actions[i], 1.0 / legal_actions.Count);
                }
            }
            return action_probs;
        }
        // MCCFR - Outcome Sampling
        public void iteration() {
            // Single iteration of MCCFR - Outcome Sampling
            for (int player = 0; player < 2; player++) {
                BluffStop game = new BluffStop();
                episode(game, player, 1.0, 1.0, 1.0);
            }
        }
        // episode
        public double episode(BluffStop game, int update_player, double my_reach, double opp_reach, double sample_reach){
            if (game.isTerminal()) {
                return game.getUtility();
            }
            int cur_player = game.currentPlayer;
            string info_state = game.getInfoState();
            Card lastClaimedCard = game.getLastClaimedCard();
            List<string> legal_actions = game.validMoves(lastClaimedCard);
            int num_legal_actions = legal_actions.Count;
            List<List<double>> lookup_info_state = info_state_lookup(info_state, num_legal_actions);
            List<double> policy = regret_matching(lookup_info_state[Index.Regret], num_legal_actions);

            List<double> sample_policy = new List<double>();
            if (cur_player == update_player) {
                // sample policy will be a combination of uniform policy and current policy
                for (int i = 0; i < num_legal_actions; i++) {
                    double factor_1 = exploration_constant * 1.0 / num_legal_actions;
                    double factor_2 = (1.0 - exploration_constant) * policy[i];
                    sample_policy.Add(factor_1 + factor_2);
                }
            }
            else {
                // sample policy will be the current policy
                sample_policy = policy;
            }
            // sample action
            int sample_action_idx = sample_action_from_policy(sample_policy);
            // apply action
            game.applyAction(legal_actions[sample_action_idx]);

            // update reach probs
            double new_my_reach, new_opp_reach;
            if (cur_player == update_player) {
                new_my_reach = my_reach * policy[sample_action_idx];
                new_opp_reach = opp_reach;
            }
            else {
                new_my_reach = my_reach;
                new_opp_reach = opp_reach * policy[sample_action_idx];
            }
            double new_sample_reach = sample_reach * sample_policy[sample_action_idx];
            // recurse
            double child_value = episode(game, update_player, new_my_reach, new_opp_reach, new_sample_reach);

            // Child value estimates
            List<double> child_values = new List<double>();
            for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                if (action_idx == sample_action_idx) {
                    child_values.Add(child_value / sample_policy[action_idx]);
                }
                else {
                    child_values.Add(0.0);
                }
            }
            double value_estimate = 0.0;
            for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                value_estimate += child_values[action_idx] * policy[action_idx];
            }

            // Regret and avg policy updates
            if (cur_player == update_player) {
                List<double> regret_matching_policy = regret_matching(lookup_info_state[Index.Regret], num_legal_actions);
                double cf_value = value_estimate * opp_reach / sample_reach;
                // Update regrets
                for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                    double cf_action_value = child_values[action_idx] * opp_reach / sample_reach;
                    add_regret(info_state, action_idx, cf_action_value - cf_value);
                }
                // Update avg policy 
                for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                    double pol_increment = my_reach * regret_matching_policy[action_idx] / sample_reach;
                    add_avg_policy(info_state, action_idx, pol_increment);    
                }
            }
            return value_estimate;
        }
        // sample action from policy
        public int sample_action_from_policy(List<double> policy) {
            // Sample an action from the given policy
            Random rand = new Random();
            double rand_num = rand.NextDouble();
            double cumulative_prob = 0.0;
            for (int i = 0; i < policy.Count; i++) {
                cumulative_prob += policy[i];
                if (rand_num < cumulative_prob) {
                    return i;
                }
            }
            return -1;
        }
    } // end of class MCCFROutcomeSampling
} // end namespace MCCFRBase


