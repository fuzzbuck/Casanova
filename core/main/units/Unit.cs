using Casanova.core.types;
using Casanova.core.types.bodies;
using Casanova.core.types.effects;
using Godot;
using Godot.Collections;
using static Casanova.core.Vars.Enums;
using Particles = Casanova.core.types.effects.Particles;

namespace Casanova.core.main.units
{
    public static class Utils
    {
        public static Body CreateBody(UnitType type)
        {
            var body = (Body) ResourceLoader.Load<PackedScene>(Vars.path_types + $"/bodies/{type.Body}.tscn").Instance();
            body.Init(type);

            return body;
        }

        public static Node CreateTypeEffect(string name)
        {
            return ResourceLoader.Load<PackedScene>(Vars.path_types + $"/effects/{name}.tscn").Instance();
        }
    }

    public class Unit : Node2D
    {
        public int netId;
        public Body Body;
        public Node2D Content;
        private readonly Array<Particles> Particles = new Array<Particles>();
        private readonly Array<SkidMark> SkidMarks = new Array<SkidMark>();
        public UnitType Type;

        public override void _Ready()
        {
            Content = GetNode<Node2D>("Content");
            Body = Utils.CreateBody(Type);

            if (Type.ParticleEffects != null)
                foreach (var kvp in Type.ParticleEffects)
                {
                    var pos = kvp.Key;
                    var particlesInfo = kvp.Value;
                    var particles = (Particles) Utils.CreateTypeEffect("Particles");

                    particles.Info = particlesInfo;
                    particles.Pos = pos;

                    particles.InitialVelocity = particlesInfo.Velocity;
                    particles.Direction = particlesInfo.Direction;
                    particles.Amount = particlesInfo.Amount;

                    Particles.Add(particles);
                    Content.AddChild(particles);
                }

            if (Type.SkidMarks != null)
                foreach (var kvp in Type.SkidMarks)
                {
                    var pos = kvp.Key;
                    var skid = kvp.Value;
                    var skidMark = (SkidMark) Utils.CreateTypeEffect("SkidMark");

                    var grad = new Gradient();

                    var fade = skid.Color;
                    fade.a = 0;

                    grad.SetColor(1, skid.Color);
                    grad.SetColor(0, fade);


                    skidMark.Width = skid.Width;
                    skidMark.Gradient = grad;
                    skidMark.Pos = pos;
                    skidMark.Info = skid;
                    skidMark.Modulate = new Color(1, 1, 1, skid.Opacity / 100f);

                    if (skid.Curve != null)
                        skidMark.WidthCurve = skid.Curve;

                    SkidMarks.Add(skidMark);
                    Content.AddChild(skidMark);
                }

            AddChild(Body);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (SkidMarks != null)
                foreach (var skid in SkidMarks)
                {
                    skid.AddPoint(Body.InWorldPosition + skid.Pos.Rotated(Body.Rotation));
                    if (skid.Points.Length > skid.Info.Length)
                        skid.RemovePoint(0);
                }

            if (Particles != null)
                foreach (var particles in Particles)
                {
                    particles.GlobalPosition = Body.InWorldPosition + particles.Pos.Rotated(Body.Rotation);
                    particles.GlobalRotation = Body.Rotation;
                }
        }
    }
}