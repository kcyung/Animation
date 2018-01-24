using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDIDrawer;

namespace Animation
{
    internal interface IRender
    {
        // render instance to the supplied drawer
        void Render(CDrawer canvas);
    }

    internal interface IAnimate
    {
        // Cause per-tick state change to instance (movement, animation, etc)
        void Tick();
    }

    internal abstract class Shape:IRender
    {
        public PointF _position { get; protected set; }     // Current location of shape object
        protected Color _color;                             // Color of shape object
        protected Shape _parent { get; set; }               // The shapes parent object

        // Constructor
        public Shape(PointF position, Color color, Shape parent = null)
        {
            _position = position;
            _color = color;
            _parent = parent;
        }

        // Draws a line to connect child shape to their parent (if one exists)
        public virtual void Render(CDrawer canvas)
        {
            if (_parent != null)
                canvas.AddLine((int)_position.X, (int)_position.Y, (int)_parent._position.X, (int)_parent._position.Y, Color.White);
        }
    }

    // A fixed square class with no animation
    internal class FixedSquare : Shape
    {
        // Constructor
        public FixedSquare(PointF position, Color color, Shape parent = null) 
            : base(position, color, parent)
        { }

        // Render a fixed square with a size of 20*20
        public override void Render(CDrawer canvas)
        {
            canvas.AddCenteredRectangle((int)_position.X, (int)_position.Y, 20, 20, _color);
            base.Render(canvas);
        }
    }

    // Abstract class for all animated objects 
    internal abstract class AniShape : Shape, IAnimate
    {
        protected double _seqValue;       // Current value of animation sequence
        protected double _seqDelta;       // Incremental change in next animation 

        public AniShape(PointF position, Color color, Shape parent, double seqValue, double seqDelta) :
            base(position, color, parent)
        {
            _seqValue = seqValue;
            _seqDelta = seqDelta;
        }

        // Default incremental delta change to sequential value
        public virtual void Tick()
        {
            _seqValue += _seqDelta;
        }
    }

    // Class for rotating polygon objects
    internal class AniPoly : AniShape
    {
        private int _sides;         // Number of sides of a polygon
        
        // Constructor
        public AniPoly(PointF position, Color color, int sides, Shape parent, double seqDelta, double seqValue = -1) :
            base(position, color, parent, seqValue, seqDelta)
        {
            if (sides < 3)
                throw new ArgumentException("The number of sides to a polygon must be a minimum of three");
            _sides = sides;
        }

        // Display polygon on canvas and tick for next frame
        public override void Render(CDrawer canvas)
        {
            canvas.AddPolygon((int)_position.X, (int)_position.Y, 25, _sides, _seqValue, _color);
            base.Render(canvas);
            Tick();
        }
    }

    internal abstract class AniChild : AniShape
    {
        protected double _distanceToParent;

        public AniChild(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1) :
            base(new PointF(-1, -1), color, parent, seqValue, seqDelta)
        {
            if (parent == null)
                throw new ArgumentException("AniChild requires to be derived from a parent");

            _position = parent._position;
            _distanceToParent = distance;
        }
    }

    internal abstract class AniHighlight : AniChild
    {
        protected double deltaAngle;
        protected long count;
        protected int slowCount;

        public AniHighlight(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1) :
            base(color, distance, parent, seqValue, seqDelta)
        {
            deltaAngle = seqDelta > 0 ? -1 * Math.PI / 180 : Math.PI / 180;
        }
    }

    // Highlight with Star Pattern around the parent
    internal class AniStarHighlight : AniHighlight
    {
        // outer and inner radius of circle for backbone of star pattern
        private float outerRadius = 75;
        private float innerRadius = 30;

        // growth (or shirnk) factor for the star
        private float expander = 1.0f;

        // flag to indicate if it's expanding/shrinking
        private bool isExpanding = true;

        // rotational amount for the star
        private double angle = 0.0;

        // Original coordinates of star vertices centered at 0,0
        private List<PointF> star = new List<PointF>();

        internal AniStarHighlight(Color clr, double distance, Shape parent, double seqDelta, double seqVal = 0)
            : base(clr, distance, parent, seqDelta, seqVal)
        {
            // Cooridnates each vertex of a star with center at 0,0
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 90), outerRadius * (float)Math.Sin(Math.PI / 180 * 90)));
            star.Add(new PointF(innerRadius * (float)Math.Cos(Math.PI / 180 * 53), innerRadius * (float)Math.Sin(Math.PI / 180 * 53)));
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 18), outerRadius * (float)Math.Sin(Math.PI / 180 * 18)));
            star.Add(new PointF(innerRadius * (float)Math.Cos(Math.PI / 180 * 342), innerRadius * (float)Math.Sin(Math.PI / 180 * 342)));
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 306), outerRadius * (float)Math.Sin(Math.PI / 180 * 306)));
            star.Add(new PointF(innerRadius * (float)Math.Cos(Math.PI / 180 * 270), innerRadius * (float)Math.Sin(Math.PI / 180 * 270)));
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 234), outerRadius * (float)Math.Sin(Math.PI / 180 * 234)));
            star.Add(new PointF(innerRadius * (float)Math.Cos(Math.PI / 180 * 198), innerRadius * (float)Math.Sin(Math.PI / 180 * 198)));
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 162), outerRadius * (float)Math.Sin(Math.PI / 180 * 162)));
            star.Add(new PointF(innerRadius * (float)Math.Cos(Math.PI / 180 * 126), innerRadius * (float)Math.Sin(Math.PI / 180 * 126)));
            star.Add(new PointF(outerRadius * (float)Math.Cos(Math.PI / 180 * 90), outerRadius * (float)Math.Sin(Math.PI / 180 * 90)));
        }

        public override void Render(CDrawer canvas)
        {
            // temporary lists used for rendering each tick for rotating and expanding
            List<PointF> rotateList = new List<PointF>();
            List<PointF> expandList = new List<PointF>();

            // Rotation
            for (int i = 0; i < star.Count(); ++i)
            {
                // Rotation about origin formula:
                // new X = x*cos(angle) - y *sin(angle)
                // new Y = y*cos(angle) + x*sin(angle)
                float newX;
                float newY;
                // Rotate each tick event
                newX = star[i].X * (float)Math.Cos(angle) - star[i].Y * (float)Math.Sin(angle);
                newY = star[i].Y * (float)Math.Cos(angle) + star[i].X * (float)Math.Sin(angle);

                // Transform back to correct position
                rotateList.Add(new PointF(newX, newY));
            }

            // Expand/Shrink each point and translate back to proper position relative to parent
            for (int i = 0; i < rotateList.Count(); ++i)
                expandList.Add(new PointF(rotateList[i].X * expander + _parent._position.X, rotateList[i].Y * expander + _parent._position.Y));

            // Draw each line of the star
            for (int i = 0; i < expandList.Count() - 1; ++i)
                canvas.AddLine((int)expandList[i].X, (int)expandList[i].Y, (int)expandList[i + 1].X, (int)expandList[i + 1].Y, _color, (int)(5* Math.Pow(expander, 3.5)));

            //  Rotating Circle for 5 end points { % 5 to get 5 points, * 2 to get even ones}
            canvas.AddCenteredEllipse((int)expandList[slowCount % 5 * 2].X, (int)expandList[slowCount % 5 * 2].Y, 10, 10, Color.Aqua);

            Tick();
        }

        public override void Tick()
        {
            // increase the phase angle by 5 degree/tick event
            angle += 5 * deltaAngle;

            count++;

            // slow count for the rotating point around 5 end points
            if (count % 10 == 0)
                slowCount++;

            // Check for expansion/shrinkage and increment, flip direction at limits
            if (isExpanding)
            {
                expander += 0.01f;
                if (expander >= 1.2f)
                    isExpanding = false;
            }
            else
            {
                expander -= 0.01f;
                if (expander <= 0.8f)
                    isExpanding = true;
            }
        }
    }

    // Highlight with 12 obrital balls around the parent
    internal class AniOrbitalHighlight : AniHighlight
    {
        private Color[] colors =
            new Color[] { Color.Orchid, Color.Gold, Color.Lime, Color.HotPink, Color.Silver, Color.LightSalmon };
        private double angle = 0.0;

        public AniOrbitalHighlight(Color clr, double distance, Shape parent, double seqDelta, double seqVal = -1)
            : base(clr, distance, parent, seqDelta, seqVal)
        {
        }

        public override void Render(CDrawer canvas)
        {
            // Draw 12 orbiting circles around the parent -> distance of parent grows/shrinks with time
            for (int i = 0; i < 12; ++i)
            {
                // X Position = parent position + 
                //              Cos(sequence value + one of 12 slots on unit circle) 
                //              * distance from child to parent
                //              * (1 + sinusodial wave between 0 to 1)                                                
                canvas.AddCenteredEllipse(
                    (int)(_parent._position.X +
                         (float)(Math.Cos(_seqValue + 2 * Math.PI * i / 12)
                         * _distanceToParent
                         * (1 + Math.Abs((float)Math.Sin(angle))))),
                    (int)(_parent._position.Y +
                         (float)(Math.Sin(_seqValue + 2 * Math.PI * i / 12)
                         * _distanceToParent
                         * (1 + Math.Abs((float)Math.Sin(angle))))),
                    10, 10, (i % 2 == 0) ? _color : colors[i / 2]);
            }

            Tick();

            // Set new position 
            _position = new PointF(_position.X + (float)(Math.Cos(_seqValue * _distanceToParent)), _position.Y + (float)(Math.Sin(_seqValue) * _distanceToParent));
        }

        public override void Tick()
        {
            // increment the pSequenceValue by delta
            base.Tick();

            // increase the phase angle by 1 degree/tick event
            angle += deltaAngle;
        }
    }

    internal abstract class AniBall : AniChild
    {
        public AniBall(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1)
            : base(color, distance, parent, seqValue, seqDelta)
        {
        }

        public override void Render(CDrawer canvas)
        {
            Tick();
            canvas.AddCenteredEllipse((int)_position.X, (int)_position.Y, 20, 20, _color);
            base.Render(canvas);
        }
    }

    internal class HWobbleBall: AniBall
    {
        public HWobbleBall(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1) :
            base(color, distance, parent, seqValue, seqDelta)
        {

        }

        public override void Tick()
        {
            base.Tick();
            _position = new PointF(_parent._position.X + (float)(Math.Cos(_seqValue) * _distanceToParent),
                                   _parent._position.Y);
        }
    }

    internal class VWobbleBall : AniBall
    {
        public VWobbleBall(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1) :
            base(color, distance, parent, seqValue, seqDelta)
        {

        }

        public override void Tick()
        {
            base.Tick();
            _position = new PointF(_parent._position.X, 
                            _parent._position.Y + (float)(Math.Cos(_seqValue) * _distanceToParent));
        }
    }

    internal class OrbitBall : AniBall
    {
        public OrbitBall(Color color, double distance, Shape parent, double seqDelta, double seqValue = -1) :
            base(color, distance, parent, seqValue, seqDelta)
        {

        }

        public override void Tick()
        {
            base.Tick();
            _position = new PointF(_parent._position.X + (float)(Math.Cos(_seqValue) * _distanceToParent),
                                   _parent._position.Y + (float)(Math.Sin(_seqValue) * _distanceToParent));
        }
    }
}
