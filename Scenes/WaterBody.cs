using Godot;

namespace Game
{
    [Tool]
    public partial class WaterBody : ColorRect
    {
        private const uint PointCount = 20;
        [Export] public float WaveTargetHeight = 20.0f;
        [Export] public float WaveStiffness = 0.01f;
        [Export] public float WaveDampening = 0.005f;
        [Export] public int WaveSpreadPasses = 8;
        [Export] public float WaveSpreadFactor = 0.5f;
        [Export] public float WaveImpactPower = 50; // This gets multiplied by the velocity of an object falling into the water body
        [Export] public bool ContinousImpact = false;
        [Export] public float ContinousImpactPower = 100;

        private ShaderMaterial shaderMaterial;
        private Area2D area;
        private CollisionShape2D collisionShape;
        private readonly Vector2[] points = new Vector2[PointCount];
        private readonly float[] pointsYVelocities = new float[PointCount];

        private void uploadPoints()
        {
            shaderMaterial.SetShaderParameter("points", points);
        }

        private async void interactWithBody(PhysicsBody2D body)
        {
            int lowestDistanceIndex = -1;
            for (int i = 0; i < PointCount; i++)
            {
                if (lowestDistanceIndex < 0)
                    lowestDistanceIndex = i;
                else
                {
                    float lowestDistance = points[lowestDistanceIndex].DistanceTo(body.GlobalPosition);
                    float distance = points[i].DistanceTo(body.GlobalPosition);
                    if (distance < lowestDistance)
                        lowestDistanceIndex = i;
                }
            }
            if (lowestDistanceIndex >= 0)
            {
                Vector2 prevPos = body.GlobalPosition;
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Vector2 velocity = body.GlobalPosition - prevPos;
                pointsYVelocities[lowestDistanceIndex] += velocity.Y * WaveImpactPower;
            }
        }

        private void onResized()
        {
            RectangleShape2D shape = collisionShape.Shape as RectangleShape2D;
            area.Position = new Vector2(Size.X / 2, WaveTargetHeight + (Size.Y - WaveTargetHeight) / 2.0f);
            shape.Size = Size - new Vector2(0, WaveTargetHeight);

            for (uint i = 0; i < PointCount; i++)
            {
                points[i] = new(
                    GlobalPosition.X + (Size.X / (points.Length - 1) * i),
                    GlobalPosition.Y + WaveTargetHeight
                );
            }

            uploadPoints();
        }

        public override async void _Ready()
        {
            shaderMaterial = Material as ShaderMaterial;
            area = GetNode<Area2D>("Area2D");
            collisionShape = area.GetNode<CollisionShape2D>("CollisionShape2D");
            Timer timer = GetNode<Timer>("Timer");

            for (uint i = 0; i < PointCount; i++)
            {
                pointsYVelocities[i] = 0;
            }

            onResized();
            Resized += onResized;

            timer.Timeout += () =>
            {
                if (ContinousImpact)
                {
                    pointsYVelocities[1] += ContinousImpactPower;
                    pointsYVelocities[PointCount / 2] += ContinousImpactPower;
                    pointsYVelocities[PointCount - 2] += ContinousImpactPower;
                }
            };
        }

        public override void _Process(double delta)
        {
            foreach (Node2D bodyNode in area.GetOverlappingBodies())
            {
                if (bodyNode is PhysicsBody2D body)
                {
                    interactWithBody(body);
                }
            }

            for (uint i = 0; i < PointCount; i++)
            {
                if (i > 0 && i < PointCount - 1) // skip first and last points
                {
                    float extension = points[i].Y - (GlobalPosition.Y + WaveTargetHeight);
                    float loss = -WaveDampening * pointsYVelocities[i];
                    pointsYVelocities[i] += -WaveStiffness * extension + loss;
                    points[i] = points[i] with
                    {
                        Y = points[i].Y + pointsYVelocities[i] * (float)delta
                    };
                }
            }

            for (uint i = 0; i < WaveSpreadPasses; i++)
            {
                for (uint j = 0; j < PointCount; j++)
                {
                    if (j > 0)
                    {
                        pointsYVelocities[j - 1] += WaveSpreadFactor * (points[j].Y - points[j - 1].Y);
                    }
                    if (j < PointCount - 1)
                    {
                        pointsYVelocities[j + 1] += WaveSpreadFactor * (points[j].Y - points[j + 1].Y);
                    }
                }
            }

            uploadPoints();
        }
    }
}
