// MCCFR implementation in C# for the game 'Cheat'

using System;
using System.Collections.Generic;

namespace MCCFR
{
    public class MCCFRBase {
        public int REGRET_INDEX = 0;
        public int AVG_POLICY_INDEX = 1;
        public Dictionary<string, List<List<double>>> infostates = new Dictionary<string, List<List<double>>>();
        // info_state_key: [regrets, avg_policy]
        public List<List<double>> infostate_oracle(string key, int numLegalActions) {
            if (infostates.ContainsKey(key)) {
                return infostates[key];
            } else {
                List<List<double>> oracle = new List<List<double>>();
                // start with a small regret: 1/1e6 for all actions
                for (int i = 0; i < 2; i++) {
                    List<double> regrets = new List<double>();
                    for (int j = 0; j < numLegalActions; j++) {
                        regrets.Add(1.0 / 1e6);
                    }
                    oracle.Add(regrets);
                }
                infostates.Add(key, oracle);
                return oracle;
            }
        }
        public Dictionary<int, double> average_policy(State state) {
            List<int> legalActions = state.legal_actions;
            string infoStateKey = state.info_state_key;
            Dictionary<int, double> avgPolicy = new Dictionary<int, double>();
            if (!infostates.ContainsKey(infoStateKey)) {
                // uniform policy
                foreach (int action in legalActions) {
                    avgPolicy.Add(action, 1.0 / legalActions.Count);
                }
                return avgPolicy;
            }
            double policy_sum = 0.00;
            List<List<double>> theInfoState = infostates[infoStateKey];
            foreach (int action in legalActions) {
                policy_sum += theInfoState[AVG_POLICY_INDEX][action];
            }
            foreach (int action in legalActions) {
                avgPolicy.Add(action, theInfoState[AVG_POLICY_INDEX][action] / policy_sum);
            }
            return avgPolicy;
        }
        public List<double> regret_matching(List<double> regrets, int numLegalActions){
            List<double> positive_regrets = new List<double>();
            double sum_positives = 0.0;
            for (int i = 0; i < numLegalActions; i++) {
                positive_regrets.Add(Math.Max(regrets[i], 0));
                sum_positives += positive_regrets[i];
            }
            if (sum_positives <= 0.0) {
                // uniform strategy
                List<double> uniform_strategy = new List<double>();
                for (int i = 0; i < numLegalActions; i++) {
                    uniform_strategy.Add(1.0 / numLegalActions);
                }
                return uniform_strategy;
            }
            for (int i = 0; i < numLegalActions; i++) {
                positive_regrets[i] /= sum_positives;
            }
            return positive_regrets;
        }
    }
    public class MCCFROS : MCCFRBase {
        // Outcome Sampling Monte Carlo CFR
        double exploration_constant = 0.6;
        public void run(int numIterations) {
            for (int i = 0; i < numIterations; i++) {
                Console.WriteLine("Iteration " + i);
                run_iteration();
            }
        }
        public void run_iteration() {
            // initialize the regrets
            foreach (string key in infostates.Keys) {
                List<List<double>> theInfoState = infostates[key];
    }
}
