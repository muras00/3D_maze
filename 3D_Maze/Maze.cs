using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    class Maze
    {
        VertexBuffer wallBuffer;
        //VertexBuffer holds a list of 3D vertices to send to the GPU
        //wallBuffer stores all the triangles necessary to generate the wall

        Vector3[] wallPoints = new Vector3[8];

        //Color[] wallColors = new Color[4] { Color.Green, Color.DarkGreen, Color.ForestGreen, Color.DarkGreen };
        Color[] wallColors = new Color[4] { Color.Green, Color.Green, Color.Green, Color.Green };
        //colors of the walls

        #region Fields
        public const int mazeWidth = 10;
        public const int mazeHeight = 10;
        //both sets the total size of the maze

        GraphicsDevice device;
        //The GPU

        VertexBuffer floorBuffer;
        //stores all the triangles necessary to generate the floor

        Color[] floorColors = new Color[2] { Color.DarkOliveGreen, Color.Gray };
        //color of the floor

        private Random rnd = new Random();
        public Cell[,] Cells = new Cell[mazeWidth, mazeHeight];
        #endregion

        #region Constructor
        public Maze(GraphicsDevice device)
        {
            this.device = device;

            GenerateFloor();

            for (int x = 0; x < mazeWidth; x++)
                for (int z = 0; z < mazeHeight; z++)
                {
                    Cells[x, z] = new Cell();
                }
            //generates enough number of cells to cover the entire map

            GenerateMaze();

            wallPoints[0] = new Vector3(0, 1, 0);
            wallPoints[1] = new Vector3(0, 1, 1);
            wallPoints[2] = new Vector3(0, 0, 0);
            wallPoints[3] = new Vector3(0, 0, 1);
            wallPoints[4] = new Vector3(1, 1, 0);
            wallPoints[5] = new Vector3(1, 1, 1);
            wallPoints[6] = new Vector3(1, 0, 0);
            wallPoints[7] = new Vector3(1, 0, 1);

            //generates blocks in the 3D world
            //blocks = cells (walls of the blocks = lines of the cells)

            BuildWallBuffer();
        }
        #endregion

        #region The Floor
        private void GenerateFloor()
        {
            //generates points that make up the triangles for the floor
            List<VertexPositionColor> vertexList = new List<VertexPositionColor>();

            int counter = 0;

            for (int x = 0; x < mazeWidth; x++)
            {
                counter++;
                for (int z = 0; z < mazeHeight; z++)
                {
                    counter++;
                    foreach (VertexPositionColor vertex in FloorTile(x, z, floorColors[counter % 2]))
                    {
                        vertexList.Add(vertex);
                    }
                }
            }

            floorBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, vertexList.Count, BufferUsage.WriteOnly);

            floorBuffer.SetData<VertexPositionColor>(vertexList.ToArray());
        }

        private List<VertexPositionColor> FloorTile(int xOffset, int zOffset, Color tileColor)
        {
            //builds vertices for each square
            //two triangles makes a square -> six VertexPositionColors are returned

            List<VertexPositionColor> vList = new List<VertexPositionColor>();

            vList.Add(new VertexPositionColor(new Vector3(0 + xOffset, 0, 0 + zOffset), tileColor));
            vList.Add(new VertexPositionColor(new Vector3(1 + xOffset, 0, 0 + zOffset), tileColor));
            vList.Add(new VertexPositionColor(new Vector3(0 + xOffset, 0, 1 + zOffset), tileColor));

            vList.Add(new VertexPositionColor(new Vector3(1 + xOffset, 0, 0 + zOffset), tileColor));
            vList.Add(new VertexPositionColor(new Vector3(1 + xOffset, 0, 1 + zOffset), tileColor));
            vList.Add(new VertexPositionColor(new Vector3(0 + xOffset, 0, 1 + zOffset), tileColor));

            return vList;
        }
        #endregion

        #region Draw
        public void Draw(Camera camera, BasicEffect effect)
        {
            //Draws the maze
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;

            effect.World = Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(floorBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, floorBuffer.VertexCount / 3);
                //causes GPU to interpret the vertex buffer and output the triangles contained to itself (the GPU)
                device.SetVertexBuffer(wallBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, wallBuffer.VertexCount / 3);
            }
        }
        #endregion

        #region Maze Generation
        public void GenerateMaze()
        {
            //generates a new maze whenever the method is called
            for (int x = 0; x < mazeWidth; x++)
                for (int z = 0; z < mazeHeight; z++)
                {
                    Cells[x, z].Lines[0] = true;
                    Cells[x, z].Lines[1] = true;
                    Cells[x, z].Lines[2] = true;
                    Cells[x, z].Lines[3] = true;
                    Cells[x, z].Open = false;
                    //all cells have all four lines (walls) and are closed by default
                }

            Cells[0, 0].Open = true;
            //The first cell of the maze (0, 0) is opened by default
            //the EvaluateCell() method randomly generates a maze around this first cell
            CheckNeighboringCells(new Vector2(0, 0));
        }
        #endregion

        private void CheckNeighboringCells(Vector2 cell)
        {
            List<int> neighborCells = new List<int>();
            neighborCells.Add(0);
            neighborCells.Add(1);
            neighborCells.Add(2);
            neighborCells.Add(3);
            //0 = neighboring cell located above, 1 = right
            //2 = below, and 3 = left of the cell

            while (neighborCells.Count > 0)
            {
                int pick = rnd.Next(0, neighborCells.Count);
                //generates random numbers to determine which of the cells will have openings
                //hence, determines where a wall stands or doesn't stand (if an opening exists)
                int selectedNeighbor = neighborCells[pick];
                neighborCells.RemoveAt(pick);
                //removes a line from a cell (removes one of the four walls in 3D world)
                //when a line is removed from a cell, the corresponding line from a neighboring cell needs to be also removed

                Vector2 neighbor = cell;

                switch (selectedNeighbor)
                {
                    case 0:
                        neighbor += new Vector2(0, -1);
                        break;
                    case 1:
                        neighbor += new Vector2(1, 0);
                        break;
                    case 2:
                        neighbor += new Vector2(0, 1);
                        break;
                    case 3:
                        neighbor += new Vector2(-1, 0);
                        break;
                }

                if ((neighbor.X >= 0) && (neighbor.X < mazeWidth) && (neighbor.Y >= 0) && (neighbor.Y < mazeHeight))
                {
                    if (!Cells[(int)neighbor.X, (int)neighbor.Y].Open)
                    {
                        Cells[(int)neighbor.X, (int)neighbor.Y].Open = true;
                        Cells[(int)cell.X, (int)cell.Y].Lines[selectedNeighbor] = false;
                        Cells[(int)neighbor.X, (int)neighbor.Y].Lines[(selectedNeighbor + 2) % 4] = false;
                        CheckNeighboringCells(neighbor);
                    }
                }
            }
        }
        #region Walls
        private void BuildWallBuffer()
        {
            //loops through each cells and accumulate a list of triangles needed for the wall
            List<VertexPositionColor> wallVertexList = new List<VertexPositionColor>();

            for (int x = 0; x < mazeWidth; x++)
            {
                for (int z = 0; z < mazeHeight; z++)
                {
                    foreach (VertexPositionColor vertex in BuildMazeWall(x, z))
                    {
                        wallVertexList.Add(vertex);
                    }
                }
            }

            wallBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, wallVertexList.Count, BufferUsage.WriteOnly);

            wallBuffer.SetData<VertexPositionColor>(wallVertexList.ToArray());
        }

        private List<VertexPositionColor> BuildMazeWall(int x, int z)
        {
            //generates points that make up the triangles for the wall
            List<VertexPositionColor> triangles = new
                List<VertexPositionColor>();

            if (Cells[x, z].Lines[0])
            {
                triangles.Add(CalcPoint(0, x, z, wallColors[0]));
                triangles.Add(CalcPoint(4, x, z, wallColors[0]));
                triangles.Add(CalcPoint(2, x, z, wallColors[0]));
                triangles.Add(CalcPoint(4, x, z, wallColors[0]));
                triangles.Add(CalcPoint(6, x, z, wallColors[0]));
                triangles.Add(CalcPoint(2, x, z, wallColors[0]));
            }

            if (Cells[x, z].Lines[1])
            {
                triangles.Add(CalcPoint(4, x, z, wallColors[1]));
                triangles.Add(CalcPoint(5, x, z, wallColors[1]));
                triangles.Add(CalcPoint(6, x, z, wallColors[1]));
                triangles.Add(CalcPoint(5, x, z, wallColors[1]));
                triangles.Add(CalcPoint(7, x, z, wallColors[1]));
                triangles.Add(CalcPoint(6, x, z, wallColors[1]));
            }

            if (Cells[x, z].Lines[2])
            {
                triangles.Add(CalcPoint(5, x, z, wallColors[2]));
                triangles.Add(CalcPoint(1, x, z, wallColors[2]));
                triangles.Add(CalcPoint(7, x, z, wallColors[2]));
                triangles.Add(CalcPoint(1, x, z, wallColors[2]));
                triangles.Add(CalcPoint(3, x, z, wallColors[2]));
                triangles.Add(CalcPoint(7, x, z, wallColors[2]));
            }

            if (Cells[x, z].Lines[3])
            {
                triangles.Add(CalcPoint(1, x, z, wallColors[3]));
                triangles.Add(CalcPoint(0, x, z, wallColors[3]));
                triangles.Add(CalcPoint(3, x, z, wallColors[3]));
                triangles.Add(CalcPoint(0, x, z, wallColors[3]));
                triangles.Add(CalcPoint(2, x, z, wallColors[3]));
                triangles.Add(CalcPoint(3, x, z, wallColors[3]));
            }

            return triangles;
        }

        private VertexPositionColor CalcPoint(int wallPoint, int xOffset, int zOffset, Color color)
        {
            //looks up points that comprises each wall segment
            //and build the actual VertexPositionColor
            return new VertexPositionColor(wallPoints[wallPoint] + new Vector3(xOffset, 0, zOffset), color);
        }

        private BoundingBox BuildCollider(int x, int z, int point1, int point2)
        {
            //builds colliders for the walls, so that the player doesn't walk through the walls
            BoundingBox thisBox = new BoundingBox(wallPoints[point1], wallPoints[point2]);
            thisBox.Min.X += x;
            thisBox.Min.Z += z;
            thisBox.Max.X += x;
            thisBox.Max.Z += z;

            thisBox.Min.X -= 0.1f;
            thisBox.Min.Z -= 0.1f;
            thisBox.Max.X += 0.1f;
            thisBox.Max.Z += 0.1f;

            return thisBox;
        }

        public List<BoundingBox> DetectWallCollision(int x, int z)
        {
            //uses BuildBoundingBox() up to four times, depending on the existing number of walls
            List<BoundingBox> boxes = new List<BoundingBox>();

            if (Cells[x, z].Lines[0])
                boxes.Add(BuildCollider(x, z, 2, 4));

            if (Cells[x, z].Lines[1])
                boxes.Add(BuildCollider(x, z, 6, 5));

            if (Cells[x, z].Lines[2])
                boxes.Add(BuildCollider(x, z, 3, 5));

            if (Cells[x, z].Lines[3])
                boxes.Add(BuildCollider(x, z, 2, 1));

            return boxes;
        }
        #endregion
    }
}