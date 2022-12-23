using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    class Cell
    {
        public bool[] Lines = new bool[4] { true, true, true, true };
        //true represents a line, and false represents an opening in a cell
        //newly generated cell will have lines on all four coordinates
        //and these four lines make up a square-shaped cell
        public bool Open = false;
        //open will be used later on to remove lines from a cell
        //meaning: by setting a bool value in Lines to false,
        //a line in the cell will be deleted, therefore,
        //making an opening in the cell
    }
}