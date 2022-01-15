using System;
using System.Collections.Generic;

namespace bluffstopCFR
{
    // Blueprint for a player
    public class Player
    {
        public Dictionary<string, Node> strategy;
        //constructor
        public Player(Dictionary<string, Node> strategy = null)
        {
            // initialize the strategy
            this.strategy = strategy;
        }
        
        public virtual int getAction(BluffStop game, List<string> moves)
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