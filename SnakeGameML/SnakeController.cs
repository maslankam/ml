using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGameML
{
    public enum Steering
    {
        stay = 0,
        right = 1,
        left = 2
    }


    public class SnakeController
    {
        private int counter;

        public Steering MakeMove()
        {
            if(counter % 2 == 0)
            {
                counter = 0;
                return Steering.right;
            }
            else
            {
                counter++;
                return Steering.stay;
            }
            
        }
    }
}
