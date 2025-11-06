using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SlimeGame.GameAsset
{
    public class Camera
    {
        // Core Camera Properties
        public Vector3 Position { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Up { get; set; }

        public float AspectRatio { get; private set; }
        public float FieldOfView { get; set; }
        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }

        // Derived Matrices (The most important parts!)
        public Matrix ViewMatrix { get; private set; }
        public Matrix FieldOfViewMatrix { get; private set; }
        public BoundingFrustum Frustum { get; private set; }

        public Camera(float aspectRatio, Vector3 position, Vector3 forward, Vector3 upVector, float viewDistance, float FOV = MathHelper.PiOver4)
        {
            AspectRatio = aspectRatio;
            Position = position;
            Forward = forward;
            Up = upVector; // Standard up direction

            FieldOfView = FOV; // 45 degrees
            NearClipPlane = 0.01f;
            FarClipPlane = viewDistance;

            // Calculate the initial matrices
            UpdateViewMatrix();
            UpdateProjectionMatrix();
        }
        public void UpdateViewMatrix()
        {
            // The LookAt target is wherever the camera is pointing plus its forward vector
            ViewMatrix = Matrix.CreateLookAt(Position, Position + Forward, Up);
            // Rebuild the frustum whenever the view changes
            RecreateFrustum();
        }

        public void UpdateProjectionMatrix()
        {
            FieldOfViewMatrix = Matrix.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearClipPlane, FarClipPlane);
            // Rebuild the frustum whenever the projection changes
            RecreateFrustum();
        }

        private void RecreateFrustum()
        {
            // The frustum is built from the combined view and projection matrices
            Matrix viewProjection = ViewMatrix * FieldOfViewMatrix;
            Frustum = new BoundingFrustum(viewProjection);
        }
        public void Move(Vector3 translation)
        {
            // Transform the movement vector to be relative to the camera's orientation
            Position += Forward * translation.Z;
            Position += Vector3.Cross(Up, Forward) * translation.X; // Right movement
            Position += Up * translation.Y;
            UpdateViewMatrix();
        }
        public void SetPosition(Vector3 position)
        {
            Position = position;
            UpdateViewMatrix();
        }
        public void SetAspectRatio(float aspectRatio)
        {
            AspectRatio = aspectRatio;
            UpdateProjectionMatrix();
        }
        // Rotates the camera (using a simple Yaw/Pitch method. Can be prone to gimbal lock if looking straight up/down)
        public void Rotate(float yaw, float pitch)
        {
            // Create rotation matrices
            Matrix yawRotation = Matrix.CreateFromAxisAngle(Up, yaw);
            Matrix pitchRotation = Matrix.CreateFromAxisAngle(Vector3.Cross(Up, Forward), pitch); // Right axis

            // Apply rotations to the forward vector
            Forward = Vector3.Transform(Forward, yawRotation * pitchRotation);
            Forward.Normalize();

            UpdateViewMatrix();
        }
        public void SetRotation(float yaw, float pitch)
        {
            // create a new forward from rotation
            Forward = new Vector3(
            (float)(Math.Cos(pitch) * Math.Sin(yaw)),
            (float)Math.Sin(pitch),
            (float)(Math.Cos(pitch) * Math.Cos(yaw))
            );
            //Forward.Normalize();

            UpdateViewMatrix();
        }
    }
}
