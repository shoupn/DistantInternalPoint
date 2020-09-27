namespace polylabel
{
    using System;
    using NetTopologySuite.Geometries;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            var points = new List<Point>() {new Point(-70.0, 2.0), new Point(0.0, 2.0), new Point(-1.0, -10.0), new Point(2.0, 0.0), new Point(1.2, 0.0) };
            var poly = new DistantInternalPoint.DistantInternalPoint();
            var p = poly.GetPolyLabel(points);
        }
    }
}
