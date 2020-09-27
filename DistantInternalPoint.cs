namespace DistantInternalPoint
{
    using System;
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;
    public class DistantInternalPoint
    {
        private Cell GetCentroidCell(List<Point> polygon)
        {
            double area = 0;
            double x = 0;
            double y = 0;
            List<Point> points = polygon;

            for (int i = 0, len = points.Count, j = len - 1; i < len; j = i++)
            {
                Point a = points[i];
                Point b = points[j];
                double f = a.X * b.Y - b.X * a.Y;

                x += (a.X + b.X) * f;
                y += (a.Y + b.Y) * f;
                area += f * 3;
            }
            if (area == 0)
                return new Cell(points[0].X, points[0].Y, 0, polygon);

            return new Cell(x / area, y / area, 0, polygon);
        }

        public Point GetPolyLabel(List<Point> polygon, double precision = 1.0f)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            for (int i = 0; i < polygon.Count; i++)
            {
                Point p = polygon[i];
                if (i == 0 || p.X < minX) minX = p.X;
                if (i == 0 || p.Y < minY) minY = p.Y;
                if (i == 0 || p.X > maxX) maxX = p.X;
                if (i == 0 || p.Y > maxY) maxY = p.Y;
            }

            double width = maxX - minX;
            double height = maxY - minY;
            double cellSize = Math.Min(width, height);
            double h = cellSize / 2;

            Queue<Cell> cellQueue = new Queue<Cell>();

            if (cellSize == 0)
                return new Point(minX, minY);

            for (var x = minX; x < maxX; x += cellSize)
            {
                for (var y = minY; y < maxY; y += cellSize)
                {
                    cellQueue.Enqueue(new Cell(x + h, y + h, h, polygon));
                }
            }

            Cell bestCell = GetCentroidCell(polygon);

            Cell bBoxCell = new Cell(minX + width / 2, minY + height / 2, 0, polygon);
            if (bBoxCell.d > bestCell.d)
                bestCell = bBoxCell;

            int numProbes = cellQueue.Count;

            while (cellQueue.Count != 0)
            {
                // pick the most promising cell from the queue
                var cell = cellQueue.Dequeue();

                // update the best cell if we found a better one
                if (cell.d > bestCell.d)
                {
                    bestCell = cell;
#if DEBUG
                    Console.WriteLine("found best {0} after {1} probes", Math.Round(1e4 * cell.d) / 1e4, numProbes);
#endif
                }

                // do not drill down further if there's no chance of a better solution
                if (cell.max - bestCell.d <= precision)
                    continue;

                // split the cell into four cells
                h = cell.h / 2;
                cellQueue.Enqueue(new Cell(cell.x - h, cell.y - h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.x + h, cell.y - h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.x - h, cell.y + h, h, polygon));
                cellQueue.Enqueue(new Cell(cell.x + h, cell.y + h, h, polygon));
                numProbes += 4;
            }
#if DEBUG
            Console.WriteLine("num probes: " + numProbes);
            Console.WriteLine("best distance: " + bestCell.d);
#endif
            return new Point(bestCell.x, bestCell.y);
        }

        private class Cell
        {
            public double x, y, h, d, max;
            public Cell(double x, double y, double h, List<Point> polygon)
            {
                this.x = x;
                this.y = y;
                this.h = h;
                this.d = PointToPolygonDist(x, y, polygon);
                this.max = Convert.ToSingle(this.d + this.h * Math.Sqrt(2));
            }

            double PointToPolygonDist(double x, double y, List<Point> polygon)
            {
                bool inside = false;
                double minDistSq = double.PositiveInfinity;

                for (int i = 0, len = polygon.Count, j = len - 1; i < len; j = i++)
                {
                    Point a = polygon[i];
                    Point b = polygon[j];

                    if ((a.Y > y != b.Y > y) && (x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X))
                        inside = !inside;

                    minDistSq = Math.Min(minDistSq, GetSeqDistSq(x, y, a, b));
                }

                return Convert.ToSingle((inside ? 1 : -1) * Math.Sqrt(minDistSq));
            }

            double GetSeqDistSq(double px, double py, Point a, Point b)
            {
                double x = a.X;
                double y = a.Y;
                double dx = b.X - x;
                double dy = b.Y - y;

                if (dx != 0 || dy != 0)
                {

                    var t = ((px - x) * dx + (py - y) * dy) / (dx * dx + dy * dy);

                    if (t > 1)
                    {
                        x = b.X;
                        y = b.Y;

                    }
                    else if (t > 0)
                    {
                        x += dx * t;
                        y += dy * t;
                    }
                }

                dx = px - x;
                dy = py - y;

                return dx * dx + dy * dy;
            }
        }
    }
}
