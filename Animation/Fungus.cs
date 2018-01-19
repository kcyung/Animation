using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using GDIDrawer;

namespace Animation
{
    public class Fungus
    {
        // Color Enumerator 
        public enum Colors { Red, Green, Blue, Yellow };
        
        // Random number generator
        public static Random _rnd = new Random();

        // Constants for Window dimensions and ARGB values
        private const int MAX_ROWS = 1000;
        private const int MAX_COLS = 1000;
        private const int BASE_ARG = 32;
        private const int INCREMENTAL_ARG = 16;
        private const int MAX_ARG = 255;

        // Dictionary to hold each cell's color intensity
        private Dictionary<Point, int> _CellFillColor = new Dictionary<Point, int>();

        private Point _current = new Point(0, 0);   // Current location of fungus
        private CDrawer _canvas = null;     // Canvas for drawer to paint on
        private Colors _color;              // Color of fungus     
        
        // Constructor
        public Fungus(Point StartingPoint, CDrawer canvas, Colors color)
        {
            // Initialize Fungus values
            _current = StartingPoint;
            _canvas = canvas;
            _color = color;
            
            // Start a new thread for fungus operation
            Thread thread  = new Thread(StartFungus);
            thread.IsBackground = true;
            thread.Start();
        }

        // Thread Body
        private void StartFungus()
        {
            List<Point> AdjacentPoints;         // List of points of neighbouring cells that are inbounds
            List<KeyValuePair<Point, int>> sortedLowestIntensity;    // Sorted list of points by lowest color intensity value

            while (true)
            {
                // Get a shuffled list of neighbouring cells that are in bounds
                AdjacentPoints = ShufflePointList(AvailableMoves());

                // Sort the list of neighbours by lowest color intensity value
                sortedLowestIntensity = AdjacentPoints.ToDictionary(
                    o => o, o => _CellFillColor.ContainsKey(o) ? _CellFillColor[o] : 0).ToList();
                sortedLowestIntensity = sortedLowestIntensity.OrderBy((kvp) => kvp.Value).ToList();

                // Select cell with lowest intensity value, increase intensity and display it on the canvas
                _current = sortedLowestIntensity.First().Key;

                if (!_CellFillColor.ContainsKey(_current))
                     _CellFillColor.Add(_current, BASE_ARG);
                else
                    _CellFillColor[_current] =
                        _CellFillColor[_current] + INCREMENTAL_ARG > 255 ? 255 : _CellFillColor[_current] + INCREMENTAL_ARG;

                switch (_color)
                {
                    case Colors.Red:
                        _canvas.SetBBPixel(_current.X, _current.Y, Color.FromArgb(_CellFillColor[_current], 0 , 0));
                        break;
                    case Colors.Green:
                        _canvas.SetBBPixel(_current.X, _current.Y, Color.FromArgb(0, _CellFillColor[_current], 0));
                        break;
                    case Colors.Blue:
                        _canvas.SetBBPixel(_current.X, _current.Y, Color.FromArgb(0, 0, _CellFillColor[_current]));
                        break;
                    case Colors.Yellow:
                        _canvas.SetBBPixel(_current.X, _current.Y, Color.FromArgb(_CellFillColor[_current], _CellFillColor[_current], 0));
                        break;
                }

                Thread.Sleep(1);
            }
        }

        // Provides a valid list of next available moves within bounds
        private List<Point> AvailableMoves()
        {
            List<Point> nextMove = new List<Point>();

            for (int x = -1; x <= 1; ++x)
                for (int y = -1; y <= 1; ++y)
                    if (!(x == 0 && y== 0))
                        nextMove.Add(new Point(_current.X + x, _current.Y + y));

            nextMove.RemoveAll((o) => 
                                        o.X < 0 || 
                                        o.X > MAX_COLS - 1 || 
                                        o.Y < 0 || 
                                        o.Y > MAX_ROWS - 1 );
            return nextMove;
        }
        
        // Fisher-Yates Shuffle algorithm to mix a list of points
        private List<Point> ShufflePointList(List<Point> points)
        {
            int j;      // Index of element to swap into
            Point temp; // Place holder of element swapping

            for (int i = points.Count - 1; i > 0; --i)
            {
                //Lock rng to ensure unique random values in multi-threaded app
                lock (_rnd)
                    j = _rnd.Next(0, i + 1);

                // Swap elements
                temp = points[i];
                points[i] = points[j];
                points[j] = temp;
            }
            return points;
        }
    }
}