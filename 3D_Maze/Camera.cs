using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Maze
{
    class Camera
    {
        #region Fields
        private Vector3 position = Vector3.Zero;
        //Position of the camera in the 3D world
        private float rotation;
        //The angle the camera is facing
        private Vector3 facing;
        //The point the camera is facing towards
        private Vector3 baseCameraReference = new Vector3(0, 0, 1);
        //The direction the camera is pointing towards when the camera is not rotated at all
        private bool needViewResync = true;
        //determines when to rebuild the View Matrix
        private Matrix cachedViewMatrix;
        //By combining the camera position and the point the camera is facing towards,
        //the View Matrix helps XNA to interpret how we wish our camera to view the 3D world
        #endregion

        #region Properties
        public Matrix Projection { get; private set; }
        //While Matrix transforms points in 3D space,
        //Projection matrix describes to the GPU how to translate (or "project") 3D objects onto 2D viewing area
        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateFacing(); }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; UpdateFacing(); }
        }

        public Matrix View
        {
            get
            {
                if (needViewResync)
                {
                    cachedViewMatrix = Matrix.CreateLookAt(Position, facing, Vector3.Up);
                }
                return cachedViewMatrix;
            }
        }
        //recalculates the View Matrix
        #endregion

        #region Constructor
        public Camera(Vector3 position, float rotation, float aspectRatio, float nearClip, float farClip) {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearClip, farClip);
            //The field of view, or viewing angle of the camera is MathHelper.PiOver4, which translates to 45 degree angle
            //Aspect ratio determines the shape of the viewing area i.e. 4:3 or 16:9 aspect ratio

            //Viewing area is not the same as a viewport, but generally,
            //the aspect ratio value should match the aspect ratio of the viewport

            //nearClip and farClip defines the clipping planes (point, past which objects in 3D world will no longer be displayed)

            MoveTo(position, rotation);
            //specifies the camera position and rotation
        }
        #endregion

        #region Helper Methods
        private void UpdateFacing()
        {
            //determines "facing", the point in 3D space that the camera will look towards
            Matrix rotationMatrix = Matrix.CreateRotationY(rotation);
            Vector3 facingOffset = Vector3.Transform(baseCameraReference, rotationMatrix);
            facing = position + facingOffset;
            needViewResync = true;
        }

        public Vector3 Rotate(float scale)
        {
            //The rotation of the camera takes place here
            Matrix rotate = Matrix.CreateRotationY(rotation);
            Vector3 towards = new Vector3(0, 0, scale);
            towards = Vector3.Transform(towards, rotate);
            return (position + towards);
        }

        public void MoveTowards(float scale)
        {
            //gets the vector to move the camera towards
            MoveTo(Rotate(scale), rotation);
        }
        #endregion

        public void MoveTo(Vector3 position, float rotation)
        {
            //Actually moves the camera/player by
            //setting the related fields for both position and rotation property
            //and updating the "facing" point
            this.position = position;
            this.rotation = rotation;
            UpdateFacing();
        }
    }
}
