using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    class GoalObject
    {
        #region Fields
        private GraphicsDevice graphicsDevice;
        private Texture2D texture;

        private Vector3 location;

        private VertexBuffer objectVertexBuffer;
        private List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

        private float rotation = 0f;
        private float zrotation = 0f;

        private Random rnd = new Random();

        private const float collisionRadius = 0.25f;
        #endregion

        #region Properties
        public BoundingSphere Bounds
        {
            get
            {
                return new BoundingSphere(location, collisionRadius);
            }
        }
        #endregion

        #region Constructor
        public GoalObject(GraphicsDevice graphicsDevice, Vector3 playerLocation, float minDistance, Texture2D texture)
        {
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            PositionObject(playerLocation, minDistance);

            // Create the object(cube)'s vertical faces
            BuildFace(new Vector3(0, 0, 0), new Vector3(0, 1, 1));
            BuildFace(new Vector3(0, 0, 1), new Vector3(1, 1, 1));
            BuildFace(new Vector3(1, 0, 1), new Vector3(1, 1, 0));
            BuildFace(new Vector3(1, 0, 0), new Vector3(0, 1, 0));

            // Create the object(cube)'s horizontal faces
            BuildFaceHorizontal(new Vector3(0, 1, 0), new Vector3(1, 1, 1));
            BuildFaceHorizontal(new Vector3(0, 0, 1), new Vector3(1, 0, 0));

            objectVertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);

            objectVertexBuffer.SetData<VertexPositionTexture>(vertices.ToArray());
        }
        #endregion

        #region Update 
        public void Update(GameTime gameTime)
        {
            rotation = MathHelper.WrapAngle(rotation + 0.05f);
            zrotation = MathHelper.WrapAngle(zrotation + 0.025f);
        }
        #endregion

        #region Helper Methods
        private void BuildFace(Vector3 p1, Vector3 p2)
        {
            vertices.Add(BuildVertex(p1.X, p1.Y, p1.Z, 1, 0));
            vertices.Add(BuildVertex(p1.X, p2.Y, p1.Z, 1, 1));
            vertices.Add(BuildVertex(p2.X, p2.Y, p2.Z, 0, 1));
            vertices.Add(BuildVertex(p2.X, p2.Y, p2.Z, 0, 1));
            vertices.Add(BuildVertex(p2.X, p1.Y, p2.Z, 0, 0));
            vertices.Add(BuildVertex(p1.X, p1.Y, p1.Z, 1, 0));
        }

        private void BuildFaceHorizontal(Vector3 p1, Vector3 p2)
        {
            vertices.Add(BuildVertex(p1.X, p1.Y, p1.Z, 0, 1));
            vertices.Add(BuildVertex(p2.X, p1.Y, p1.Z, 1, 1));
            vertices.Add(BuildVertex(p2.X, p2.Y, p2.Z, 1, 0));
            vertices.Add(BuildVertex(p1.X, p1.Y, p1.Z, 0, 1));
            vertices.Add(BuildVertex(p2.X, p2.Y, p2.Z, 1, 0));
            vertices.Add(BuildVertex(p1.X, p1.Y, p2.Z, 0, 0));
        }

        private VertexPositionTexture BuildVertex(float x, float y, float z, float u, float v)
        {
            return new VertexPositionTexture(
            new Vector3(x, y, z),
            new Vector2(u, v));
        }

        public void PositionObject(Vector3 playerLocation, float minDistance)
        {
            Vector3 newLocation;

            do
            {
                newLocation = new Vector3(rnd.Next(0, Maze.mazeWidth) + 0.5f, 0.5f, rnd.Next(0, Maze.mazeHeight) + 0.5f);
            }
            while (Vector3.Distance(playerLocation, newLocation) < minDistance);

            location = newLocation;
        }
        #endregion

        #region Draw
        public void Draw(Camera camera, BasicEffect effect)
        {
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Matrix center = Matrix.CreateTranslation(new Vector3(-0.5f, -0.5f, -0.5f));
            Matrix scale = Matrix.CreateScale(0.5f);
            Matrix translate = Matrix.CreateTranslation(location);
            Matrix rot = Matrix.CreateRotationY(rotation);
            Matrix zrot = Matrix.CreateRotationZ(zrotation);

            effect.World = center * rot * zrot * scale * translate;
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.SetVertexBuffer(objectVertexBuffer);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, objectVertexBuffer.VertexCount / 3);
            }
        }
        #endregion
    }
}
