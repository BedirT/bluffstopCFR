using System;
using System.Collections.Generic;

namespace bluffstopCFR
{
    // Blueprint for a player
    public class Player
    {
        public Dictionary<int, float> strategy;
        //constructor
        public Player(Dictionary<int, float> strategy = null)
        {
            // initialize the strategy
            this.strategy = strategy;
        }
        
        public virtual int getAction(KuhnPoker game, List<int> actions)
        {
            // get the action
            return 0;
        }

        public virtual Player clone()
        {
            // clone the player
            return null;
        }
    }
} // end namespace bluffstopCFR