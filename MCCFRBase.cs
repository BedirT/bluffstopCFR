using System;
using Numpy;

namespace bluffstopCFR {
    static class Index {
        public static int Regret = 0;
        public static int AvgPolicy = 1;
    }
    public class MCCFROutcomeSampling {
        // init
        public Dictionary<string, List<NDarray>> info_states;
        public double eps;
        // public Game game;
        public double exploration_constant = 0.6;
        public MCCFROutcomeSampling() {
            // this.game = game;
            info_states = new Dictionary<string, List<NDarray>>();
            eps = 1e6;
        }
        //
        public List<NDarray> info_state_lookup(string info_state, int num_legal_actions){
            // return the list for the info_state from the info_states dictionary
            // if exists. Otherwise create a uniform policy.
            if (info_states.ContainsKey(info_state)) {
                return info_states[info_state];
            } 
            // 
            var shape = new Numpy.Models.Shape(num_legal_actions);
            info_states[info_state] = new List<NDarray> { 
                np.ones(shape, np.float64) / eps, 
                np.ones(shape, np.float64) / eps };
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
        public NDarray regret_matching(NDarray regrets, int num_legal_actions){
            var shape = new Numpy.Models.Shape(num_legal_actions);
            NDarray positive_regrets = np.maximum(regrets, np.zeros(shape, np.float64));
            // Get positive regret sum
            double positive_regret_sum = (double)np.sum(positive_regrets);
            // return avg regret
            if (positive_regret_sum <= 0) {
                return np.ones(shape, np.float64) / num_legal_actions;
            }
            return positive_regrets / positive_regret_sum;
        }
        // action_probs
        public Dictionary<int, double> action_probs_avg_policy(string info_state, int player, List<int> legal_actions){
            // For the given player, return the action probabilities for the given info_state
            Dictionary<int, double> action_probs = new Dictionary<int, double>();
            if (info_states.ContainsKey(info_state)) {
                NDarray avg_policy = info_states[info_state][Index.AvgPolicy];
                double avg_policy_sum = (double)np.sum(avg_policy);
                NDarray avg_policy_normalized = avg_policy / avg_policy_sum;
                foreach (int action in legal_actions) {
                    action_probs[action] = (double)avg_policy_normalized[action];
                }
            }
            else {
                // return uniform policy
                foreach (int action in legal_actions) {
                    action_probs[action] = 1.0 / legal_actions.Count;
                }
            }
            return action_probs;
        }
        // MCCFR - Outcome Sampling
        public void iteration() {
            // Single iteration of MCCFR - Outcome Sampling
            KuhnPoker game = new KuhnPoker();
            for (int player = 0; player < 2; player++) {
                episode(game, player, 1.0, 1.0, 1.0);
                game.reset();
            }
        }
        // episode
        public double episode(KuhnPoker game, int update_player, double my_reach, double opp_reach, double sample_reach){
            if (game.isTerminal()) {
                return game.getUtility(update_player);
            }
            int cur_player = game.currentPlayer;
            string info_state = game.getInfoState(cur_player);
            List<int> legal_actions = game.legalActions();
            int num_legal_actions = legal_actions.Count;
            List<NDarray> lookup_info_state = info_state_lookup(info_state, num_legal_actions);
            NDarray policy = regret_matching(lookup_info_state[Index.Regret], num_legal_actions);

            var shape = new Numpy.Models.Shape(num_legal_actions);
            NDarray sample_policy = null;
            if (cur_player == update_player) {
                // sample policy will be a combination of uniform policy and current policy
                for (int i = 0; i < num_legal_actions; i++) {
                    var uniform_policy = (np.ones(shape, np.float64) / num_legal_actions);
                    sample_policy = (exploration_constant * uniform_policy) + ((1 - exploration_constant) * policy);
                }
            }
            else {
                // sample policy will be the current policy
                sample_policy = policy;
            }
            if (sample_policy == null) {
                throw new Exception("sample_policy is null");
            }
            // sample action
            var a = np.arange(num_legal_actions);
            var choice = np.random.choice(a, p: sample_policy);
            int sample_action_idx = (int)choice;
            // apply action
            game.applyAction(legal_actions[sample_action_idx]);

            // update reach probs
            double new_my_reach, new_opp_reach;
            if (cur_player == update_player) {
                new_my_reach = my_reach * policy.GetData<double>()[sample_action_idx];
                new_opp_reach = opp_reach;
            }
            else {
                new_my_reach = my_reach;
                new_opp_reach = opp_reach * policy.GetData<double>()[sample_action_idx];
            }
            double new_sample_reach = sample_reach * sample_policy.GetData<double>()[sample_action_idx];
            // recurse
            double child_value = episode(game, update_player, new_my_reach, new_opp_reach, new_sample_reach);

            // Child value estimates
            List<double> child_values = new List<double>();
            for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                if (action_idx == sample_action_idx) {
                    child_values.Add(child_value / sample_policy.GetData<double>()[action_idx]);
                }
                else {
                    child_values.Add(0.0);
                }
            }
            double value_estimate = 0.0;
            for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                value_estimate += child_values[action_idx] * policy.GetData<double>()[action_idx];;
            }

            // Regret and avg policy updates
            if (cur_player == update_player) {
                NDarray regret_matching_policy = regret_matching(lookup_info_state[Index.Regret], num_legal_actions);
                double cf_value = value_estimate * opp_reach / sample_reach;
                // Update regrets
                for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                    double cf_action_value = child_values[action_idx] * opp_reach / sample_reach;
                    add_regret(info_state, action_idx, cf_action_value - cf_value);
                }
                // Update avg policy 
                for (int action_idx = 0; action_idx < num_legal_actions; action_idx++) {
                    double pol_increment = my_reach * regret_matching_policy.GetData<double>()[action_idx] / sample_reach;
                    add_avg_policy(info_state, action_idx, pol_increment);    
                }
            }
            return value_estimate;
        }
    } // end of class MCCFROutcomeSampling
} // end namespace MCCFRBase


