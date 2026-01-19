using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SlimeGame.GameAsset;
using SlimeGame.Models;

namespace SlimeGame
{
    internal struct ModelHolder : GenericModel
    {
        Model _model;
        Matrix _transformation = Matrix.Identity;
        Vector3 _position;
        Vector3 _dimensions;
        Color _color;

        public ModelHolder(Model model)
        {
            this._model = model;
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // calculate vertices
                    VertexPositionColorNormalTexture[] ImportVertices = new VertexPositionColorNormalTexture[part.NumVertices];
                    part.VertexBuffer.GetData<VertexPositionColorNormalTexture>(ImportVertices);

                    // set color
                    _color = ImportVertices[0].Color;

                    // calculate bounding box
                    foreach (var vertex in ImportVertices)
                    {
                        min = Vector3.Min(min, vertex.Position);
                        max = Vector3.Max(max, vertex.Position);
                    }
                }
            }
            _position = (min + max) / 2;
            //_position.Y = max.Y;
            _dimensions = max - min;
        }

        public Vector3 Position => _position;
        public Vector3 Dimensions => _dimensions;
        public Vector3 Min => _position - _dimensions / 2;
        public Vector3 Max => _position + _dimensions / 2;
        public BoundingBox BoundingBox => new BoundingBox(Min, Max);
        BoundingBox GenericModel.BoundingBox => BoundingBox;
        VertexPositionColorNormal[] GenericModel.Vertices
        {
            get => throw new NotImplementedException();
        }
        int[] GenericModel.Indeces
        {
            get => throw new NotImplementedException();
        }
        Color GenericModel.Color
        {
            get => _color;
            set => _color = value;
        }
        Vector3 GenericModel.Position
        {
            get => _position;
            set
            {
                _transformation.Decompose(out var scale, out var rotation, out var translation);
                rotation.Deconstruct(out float rotX, out float rotY, out float rotZ, out float W);
                Move(rotX, rotY, rotZ, scale.X, translation + value);
            }
        }
        public void Move(float yaw, float pitch, float roll, float scale, Vector3 translation)
        {
            Matrix world =
                    //Matrix.Identity;
                    Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) *
                    Matrix.CreateScale(scale) *
                    Matrix.CreateTranslation(translation)
                    ;
            _transformation = world;
            _position = Vector3.Transform(_position, world);
            _dimensions *= scale;
        }
        public void Draw(WorldDraw draw, GraphicsDevice GraphicsDevice)
        {
            draw.DrawModel(GraphicsDevice, _model, _transformation);
        }

    }
}
