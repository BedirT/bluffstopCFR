namespace bluffstopCFR {
    class RandomPlayer : Player {
        public RandomPlayer(){}
        
        public override int getAction(KuhnPoker game, List<int> actions) {
            Random rnd = new Random();
            int action = rnd.Next(actions.Count);
            return action;
        }
        
        public override Player clone() {
            return new RandomPlayer();
        }
    }
} // end namespace bluffstopCFR