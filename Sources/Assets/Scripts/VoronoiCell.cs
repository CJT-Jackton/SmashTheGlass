using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.IO;

public class VoronoiCell : MonoBehaviour
{
    static EventQueue eq = new EventQueue();
    static BeachLine BL = new BeachLine();
    static float currentY, lastY;
    static float boundaryLow, boundaryHigh, boundaryLeft, boundaryRight;
    static Cell[] allCells;
    static int count = 0;
    static List<VoronoiVertexPoint> voronoiVertex = new List<VoronoiVertexPoint>();

    // Use this for initialization
    //void Start () {

    //    boundaryLow = -1;
    //    boundaryHigh = 1;
    //    boundaryLeft = -1;
    //    boundaryRight = 1;
    //    int numberOfCell = 0;

    //    // Read file.
    //    try
    //    {
    //        using (StreamReader sr = new StreamReader("Assets/Output.txt"))
    //        {
    //            String line = sr.ReadLine();
    //            numberOfCell = Int32.Parse(line);
    //            allCells = new Cell[numberOfCell];

    //            for (int i = 0; i < numberOfCell; i++)
    //            {
    //                line = sr.ReadLine();
    //                String[] split = line.Split(' ');
    //                float x = float.Parse(split[0]);
    //                float y = float.Parse(split[1]);
    //                Site newSite = new Site(0, i, x, y);
    //                eq.Enqueue(newSite);
    //                allCells[i] = new Cell(2, i, x, y);
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("The file could not be read:");
    //        Debug.Log(e.Message);
    //    }

    //    lastY = float.MaxValue;
    //    while (!eq.IsEmpty())
    //    {
    //        Site eventP = eq.Dequeue();
    //        currentY = eventP.GetY();
    //        if (currentY <= boundaryLow)
    //        {
    //            Debug.Log("Reach lower boundary. " + currentY);
    //            break;
    //        }
    //        if (currentY > lastY)
    //        {
    //            //Debug.Log("Earlier event."+ currentY+" > "+ lastY);
    //            continue;
    //        }

    //        if (eventP.type == 0)
    //        {
    //            BL.Insert(eventP);
    //        }
    //        else if (eventP.type == 1)
    //            BL.HandleVVEvent((VoronoiVertexPoint)eventP);

    //        lastY = currentY;
    //    }

    //    // Output.
    //    try
    //    {
    //        using (StreamWriter sw = new StreamWriter("Assets/outputVoronoiCell.txt"))
    //        {
    //            sw.WriteLine(numberOfCell);

    //            foreach (Cell cell in allCells)
    //            {
    //                cell.HandleEdges(boundaryHigh, boundaryLow, boundaryLeft, boundaryRight);
    //                cell.Sort();
    //                sw.WriteLine(cell.relatedVoronoiVertex.Count);
    //                for (int i = 0; i < cell.relatedVoronoiVertex.Count; i++)
    //                {
    //                    sw.WriteLine(cell.relatedVoronoiVertex[i].x + " " +
    //                        (cell.relatedVoronoiVertex[i].y + cell.relatedVoronoiVertex[i].radius));
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("Fail to create output file!");
    //        Debug.Log(e.Message);
    //    }
    //}

    public Vector2[][] GenerateVoronoi(Vector2[] points)
    {
        Vector2[][] OutputPoints;

        boundaryLow = -1;
        boundaryHigh = 1;
        boundaryLeft = -1;
        boundaryRight = 1;
        int numberOfCell = 0;

        // Read points.
        numberOfCell = points.Length;
        allCells = new Cell[numberOfCell];

        for (int i = 0; i < numberOfCell; i++)
        {

            float x = points[i].x;
            float y = points[i].y;
            Site newSite = new Site(0, i, x, y);
            eq.Enqueue(newSite);
            allCells[i] = new Cell(2, i, x, y);
        }

        lastY = float.MaxValue;
        while (!eq.IsEmpty())
        {
            Site eventP = eq.Dequeue();
            currentY = eventP.GetY();
            if (currentY <= boundaryLow)
            {
                Debug.Log("Reach lower boundary. " + currentY);
                break;
            }
            if (currentY > lastY)
            {
                //Debug.Log("Earlier event."+ currentY+" > "+ lastY);
                continue;
            }

            if (eventP.type == 0)
            {
                BL.Insert(eventP);
            }
            else if (eventP.type == 1)
                BL.HandleVVEvent((VoronoiVertexPoint)eventP);

            lastY = currentY;
        }

        // Output.
        OutputPoints = new Vector2[allCells.Length][];

        for (int i = 0; i < allCells.Length; ++i)
        {
            Cell cell = allCells[i];

            cell.HandleEdges(boundaryHigh, boundaryLow, boundaryLeft, boundaryRight);
            cell.Sort();

            OutputPoints[i] = new Vector2[cell.relatedVoronoiVertex.Count];

            for (int j = 0; j < cell.relatedVoronoiVertex.Count; ++j)
            {
                OutputPoints[i][j] = new Vector2(cell.relatedVoronoiVertex[j].x, cell.relatedVoronoiVertex[j].y + cell.relatedVoronoiVertex[j].radius);
            }
        }

        return OutputPoints;
    }

    public class BeachLine
    {
        public Node head;

        public BeachLine()
        {
            head = null;
        }

        /// <summary>
        /// Find where to put the new site in the beach line and insert it.
        /// </summary>
        /// <param name="s"></param>
        public void Insert(Site s)
        {
            Node newNode = new Node(s);

            // Tree is empty
            if (IsEmpty())
                head = newNode;
            else
            {
                bool findRight = false;
                float ymin = float.MaxValue;
                float parabola = 0;
                Node target = head;
                Node current = head;

                // Iterate until the Site on the new Site's right.
                while (!findRight && current != null)
                {
                    if (current.s.x > newNode.s.x)
                        findRight = true;
                    parabola = GetParabolaIntersect(current.s.x, current.s.y, newNode.s.y, newNode.s.x);

                    // No intersection yet.
                    if (float.IsInfinity(parabola) && ymin == float.MaxValue)
                        target = current;
                    if (parabola <= ymin)
                    {
                        ymin = parabola;
                        target = current;
                    }
                    current = current.Right;
                }
                if (parabola > boundaryHigh && s.x > target.s.x)
                    ADDRight(target, newNode);
                else if (parabola > boundaryHigh && s.x < target.s.x)
                    ADDLeft(target, newNode);
                else
                    SplitADD(target, newNode);
            }
        }

        /// <summary>
        /// Add the new Node on the right side of the target node.
        /// </summary>
        /// <param name="targetNode"></param>
        /// <param name="newNode"></param>
        public void ADDRight(Node targetNode, Node newNode)
        {
            Node tmpRight = targetNode.Right;   // Might be null.
            Node tmpLeft = targetNode.Left;   // Might be null.
            if (tmpRight == null)   // Bottom.
            {
                newNode.Left = targetNode;
                targetNode.Right = newNode;
                if (tmpLeft != null)
                    CheckCircleEvent(tmpLeft.s, targetNode.s, newNode.s, true);
            }
            else
            {
                // Remove the affected circle event. 
                if (tmpLeft != null)
                    CheckCircleEvent(tmpLeft.s, targetNode.s, tmpRight.s, false);
                if (tmpRight.Right != null)
                    CheckCircleEvent(targetNode.s, tmpRight.s, tmpRight.Right.s, false);

                targetNode.Right = newNode;
                newNode.Left = targetNode;
                newNode.Right = tmpRight;
                tmpRight.Left = newNode;

                // Check all possible circle event.
                CheckCircleEvent(targetNode.s, newNode.s, tmpRight.s, true);
                if (tmpLeft != null)
                    CheckCircleEvent(tmpLeft.s, targetNode.s, newNode.s, true);
                if (tmpRight.Right != null)
                    CheckCircleEvent(newNode.s, tmpRight.s, tmpRight.Right.s, true);
            }
        }

        /// <summary>
        /// Add the new Node on the left side of the target node.
        /// </summary>
        /// <param name="targetNode"></param>
        /// <param name="newNode"></param>
        public void ADDLeft(Node targetNode, Node newNode)
        {
            Node tmpRight = targetNode.Right;   // Might be null.
            Node tmpLeft = targetNode.Left; // Might be null.
            if (tmpLeft == null)    // Target is head.
            {
                newNode.Right = targetNode;
                targetNode.Left = newNode;
                this.head = newNode;
                if (tmpRight != null)
                    CheckCircleEvent(newNode.s, targetNode.s, tmpRight.s, true);
            }
            else
            {
                // Remove the affected circle event. 
                if (tmpLeft.Left != null)
                    CheckCircleEvent(tmpLeft.Left.s, tmpLeft.s, targetNode.s, false);
                if (tmpLeft.Right != null)
                    CheckCircleEvent(tmpLeft.s, targetNode.s, tmpRight.s, false);

                tmpLeft.Right = newNode;
                newNode.Left = tmpLeft;
                newNode.Right = targetNode;
                targetNode.Left = newNode;

                // Check all possible circle event.
                CheckCircleEvent(tmpLeft.s, newNode.s, targetNode.s, true);
                if (tmpLeft.Left != null)
                    CheckCircleEvent(tmpLeft.Left.s, tmpLeft.s, newNode.s, true);
                if (tmpRight != null)
                    CheckCircleEvent(newNode.s, targetNode.s, tmpRight.s, true);
            }
        }

        /// <summary>
        /// Split the target node and insert the new node in between.
        /// </summary>
        /// <param name="targetNode"></param>
        /// <param name="newNode"></param>
        public void SplitADD(Node targetNode, Node newNode)
        {
            Node split = new Node(targetNode.s);
            Node tmpRight = targetNode.Right;   // Might be null.
            Node tmpLeft = targetNode.Left;   // Might be null.

            // Remove the affected circle event. 
            if (tmpLeft != null && tmpRight != null)
                CheckCircleEvent(tmpLeft.s, targetNode.s, tmpRight.s, false);

            targetNode.Right = newNode;
            newNode.Left = targetNode;
            newNode.Right = split;
            split.Left = newNode;
            if (tmpRight != null)
            {
                split.Right = tmpRight;
                tmpRight.Left = split;
            }

            // Check all possible circle event.
            if (tmpLeft != null)
                CheckCircleEvent(tmpLeft.s, targetNode.s, newNode.s, true);
            if (tmpRight != null)
                CheckCircleEvent(newNode.s, targetNode.s, tmpRight.s, true);
        }

        /// <summary>
        /// Add or remove the voronoi vertex event by the given three points.
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="pj"></param>
        /// <param name="pk"></param>
        /// <param name="isAdd"></param>
        public void CheckCircleEvent(Site pi, Site pj, Site pk, bool isAdd)
        {
            VoronoiVertexPoint lowestP = GetVoronoiVertexEvent(pi, pj, pk);
            if (isAdd && lowestP != null && !eq.IsDuplicate(lowestP))
                eq.Enqueue(lowestP);
            else if (!isAdd)
                eq.Remove(lowestP);
        }

        /// <summary>
        /// Get the bottom Site of the circle formed by given three points.
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="pj"></param>
        /// <param name="pk"></param>
        /// <returns></returns>
        public VoronoiVertexPoint GetVoronoiVertexEvent(Site pi, Site pj, Site pk)
        {
            if (pi.Equals(pk))
                return null;
            float[] circleInfo = new float[3];
            circleInfo = GetCircle(pi, pj, pk);
            if (float.IsInfinity(circleInfo[1]))
                return null;
            else
            { }
            VoronoiVertexPoint lowestP = new VoronoiVertexPoint(1, count++, circleInfo[0],
                circleInfo[1] - circleInfo[2], circleInfo[2], pi, pj, pk);
            return lowestP;
        }

        /// <summary>
        /// Get the intersection of new site(x, ynow) and parabola of Site p. 
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="ynow"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public float GetParabolaIntersect(float px, float py, float ynow, float x)
        {
            return (x * x - 2 * px * x + px * px + py * py - ynow * ynow) / (2 * (py - ynow));
        }

        /// <summary>
        /// Get the circle center and radius by the given three points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public float[] GetCircle(Site p1, Site p2, Site p3)
        {
            float[] result = new float[3];
            float A, B, C, D, x, y, r;
            A = p1.x * (p2.y - p3.y) - p1.y * (p2.x - p3.x) + p2.x * p3.y - p3.x * p2.y;
            B = (p1.x * p1.x + p1.y * p1.y) * (p3.y - p2.y) + (p2.x * p2.x + p2.y * p2.y) * (p1.y - p3.y)
                + (p3.x * p3.x + p3.y * p3.y) * (p2.y - p1.y);
            C = (p1.x * p1.x + p1.y * p1.y) * (p2.x - p3.x) + (p2.x * p2.x + p2.y * p2.y) * (p3.x - p1.x)
                + (p3.x * p3.x + p3.y * p3.y) * (p1.x - p2.x);
            D = (p1.x * p1.x + p1.y * p1.y) * (p3.x * p2.y - p2.x * p3.y) +
                (p2.x * p2.x + p2.y * p2.y) * (p1.x * p3.y - p3.x * p1.y) +
                (p3.x * p3.x + p3.y * p3.y) * (p2.x * p1.y - p1.x * p2.y);

            x = -1 * B / (2 * A);
            y = -1 * C / (2 * A);
            r = Mathf.Sqrt((B * B + C * C - 4 * A * D) / (4 * A * A));
            result[0] = x;
            result[1] = y;
            result[2] = r;
            return result;
        }

        /*
         * Bisector (pi, pj) and (pj, pk) have met, a single bisector (pi, pk) remains.
         */
        public void HandleVVEvent(VoronoiVertexPoint p)
        {
            voronoiVertex.Add(p);
            foreach (Site s in p.relatedSites)
            {
                allCells[s.index].AddVertex(p);
            }

            // Find where pj is.
            Node pj = head.Right;
            while (pj.Left != null && pj != null && pj.Right != null
                && (!pj.Left.s.Equals(p.relatedSites[0])
                || !pj.s.Equals(p.relatedSites[1])
                || !pj.Right.s.Equals(p.relatedSites[2])))
            {
                pj = pj.Right;
            }

            // Affected bisectors need to be cleaned.
            // Remove: (pi-1, pi, pj), (pj, pk, pk+1) 
            // Add: (pi-1, pi, pk), (pi, pk, pk+1) if not duplicate.
            VoronoiVertexPoint tmp;
            if (pj.Left.Left != null)
            {
                CheckCircleEvent(pj.Left.Left.s, pj.Left.s, pj.s, false);
                tmp = GetVoronoiVertexEvent(pj.Left.Left.s, pj.Left.s, pj.Right.s);
                if (tmp != null && !tmp.Equals(p))
                    CheckCircleEvent(pj.Left.Left.s, pj.Left.s, pj.Right.s, true);
            }
            if (pj.Right.Right != null)
            {
                CheckCircleEvent(pj.s, pj.Right.s, pj.Right.Right.s, false);
                tmp = GetVoronoiVertexEvent(pj.Left.s, pj.Right.s, pj.Right.Right.s);
                if (tmp != null && !tmp.Equals(p))
                    CheckCircleEvent(pj.Left.s, pj.Right.s, pj.Right.Right.s, true);
            }

            // Remove pj.
            pj.Left.Right = pj.Right;
            pj.Right.Left = pj.Left;
        }

        public void Print()
        {
            Node current = head;
            while (current != null)
            {
                Debug.Log(current.s.x + ", " + current.s.y + " -> ");
                current = current.Right;
            }
        }

        public bool IsEmpty()
        {
            if (head == null)
                return true;
            return false;
        }
    }
}

public class Site
{
    // Type: 0=site, 1=voronoi point, 2=cell point, 3=boundary intersect point
    public int type, index;
    public float x, y;
    public Site() { }

    public Site(int type, int index, float x, float y)
    {
        SetPoint(type, index, x, y);
    }

    public void SetPoint(int type, int index, float x, float y)
    {
        this.type = type;
        this.index = index;
        this.x = x;
        this.y = y;
    }

    public float GetY()
    {
        return this.y;
    }

    public bool Equals(Site p)
    {
        if ((this.x == p.x || Math.Abs(this.x - p.x) < 0.000000001) &&
            (this.y == p.y || Math.Abs(this.y - p.y) < 0.000000001))
            return true;
        return false;
    }

    public override string ToString()
    {
        return this.x + " " + this.y;
    }
}

public class VoronoiVertexPoint : Site
{
    public float radius;
    public List<Site> relatedSites;
    public VoronoiVertexPoint() { }

    public VoronoiVertexPoint(int type, int index, float x, float y, float r, Site pi, Site pj, Site pk)
    {
        this.radius = r;
        this.relatedSites = new List<Site>();
        relatedSites.Add(pi);
        relatedSites.Add(pj);
        relatedSites.Add(pk);
        SetPoint(type, index, x, y);
    }

    public float GetY()
    {
        return this.y + this.radius;
    }

    /// <summary>
    /// See if the target voronoi vertex has different related sites.
    /// If so, add that site to its related sites and return true.
    /// Else, return false.
    /// </summary>
    /// <param name="target">Target voronoi vertex.</param>
    /// <returns></returns>
    public bool OnSameCircle(VoronoiVertexPoint target)
    {
        // They are not the same point.
        if (!this.Equals(target))
            return false;

        foreach (Site s2 in target.relatedSites)
        {
            bool appeared = false;
            foreach (Site s in this.relatedSites)
                if (s.Equals(s2))
                    appeared = true;
            if (!appeared)
            {
                this.relatedSites.Add(s2);
            }
        }
        return true;
    }

    public override string ToString()
    {
        return this.x + " " + (this.y + this.radius);
    }
}

public class Cell : Site
{
    int count;
    public Dictionary<Site, int> neighbors;
    public List<VoronoiVertexPoint> relatedVoronoiVertex;
    public List<Edge> edges;

    public Cell(int type, int index, float x, float y)
    {
        this.count = 0;
        this.neighbors = new Dictionary<Site, int>();
        this.relatedVoronoiVertex = new List<VoronoiVertexPoint>();
        this.edges = new List<Edge>();
        SetPoint(type, index, x, y);
    }

    public void AddVertex(VoronoiVertexPoint vvp)
    {
        count++;
        this.relatedVoronoiVertex.Add(vvp);
        foreach (Site s in vvp.relatedSites)
        {
            Edge newEdge = new Edge(this, s, vvp);
            if (!this.neighbors.ContainsKey(s) && !s.Equals(this))
            {
                neighbors.Add(s, 1);
                this.edges.Add(newEdge);
            }
            // Neighbor already exist
            else if (this.neighbors.ContainsKey(s))
            {
                neighbors[s] = neighbors[s] + 1;
                foreach (Edge e in this.edges)
                    if (e.Equals(newEdge))
                    {
                        e.SetEnd(vvp);
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// See if any edge of this cell is open ended, if so, bound it.
    /// </summary>
    public void HandleEdges(float boundaryHigh, float boundaryLow, float boundaryLeft, float boundaryRight)
    {
        float[] vector = new float[2];
        float[] intersection = new float[2];
        bool[] boundaryIntersect = new bool[4];
        foreach (Edge e in this.edges)
        {
            if (e.IsOpen())
            {
                vector = e.GetEdgeVector();
                if (vector[1] > 0)
                {
                    intersection = e.GetYaxisIntersection(boundaryHigh);
                    if (intersection[0] > boundaryLeft && intersection[0] < boundaryRight)
                    {
                        SetEdgeEnd(e, intersection);
                        boundaryIntersect[0] = true;
                    }
                }
                else if (vector[1] < 0)
                {
                    intersection = e.GetYaxisIntersection(boundaryLow);
                    if (intersection[0] > boundaryLeft && intersection[0] < boundaryRight)
                    {
                        SetEdgeEnd(e, intersection);
                        boundaryIntersect[1] = true;
                    }
                }
                if (vector[0] < 0)
                {
                    intersection = e.GetXaxisIntersection(boundaryLeft);
                    if (intersection[1] > boundaryLow && intersection[1] < boundaryHigh)
                    {
                        SetEdgeEnd(e, intersection);
                        boundaryIntersect[2] = true;
                    }
                }
                else if (vector[0] > 0)
                {
                    intersection = e.GetXaxisIntersection(boundaryRight);
                    if (intersection[1] > boundaryLow && intersection[1] < boundaryHigh)
                    {
                        SetEdgeEnd(e, intersection);
                        boundaryIntersect[3] = true;
                    }
                }
            }
        }

        // Assign the corner point to this cell.
        if (boundaryIntersect[0] && boundaryIntersect[2])
        {
            VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryLeft,
                boundaryHigh, 0, this, null, null);
            this.relatedVoronoiVertex.Add(boundP);
        }
        else if (boundaryIntersect[0] && boundaryIntersect[3])
        {
            VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryRight,
                boundaryHigh, 0, this, null, null);
            this.relatedVoronoiVertex.Add(boundP);
        }
        else if (boundaryIntersect[1] && boundaryIntersect[2])
        {
            VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryLeft,
                boundaryLow, 0, this, null, null);
            this.relatedVoronoiVertex.Add(boundP);
        }
        else if (boundaryIntersect[1] && boundaryIntersect[3])
        {
            VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryRight,
                boundaryLow, 0, this, null, null);
            this.relatedVoronoiVertex.Add(boundP);
        }
    }

    public void SetEdgeEnd(Edge e, float[] boundaryIntersection)
    {
        // Both start and end point of an edge are VVP.
        VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryIntersection[0],
            boundaryIntersection[1], 0, e.face1, e.face2, null);
        e.SetEnd(boundP);
        this.relatedVoronoiVertex.Add(boundP);
    }

    /// <summary>
    /// This method should sort the related voronoi vertex this cell has,
    /// into a clockwise order.
    /// </summary>
    public void Sort()
    {
        int size = this.relatedVoronoiVertex.Count;

        // Compute and store the angle of every line.
        float[] theta = new float[size];
        for (int i = 0; i < size; i++)
        {
            theta[i] = Mathf.Atan2((this.relatedVoronoiVertex[i].y + this.relatedVoronoiVertex[i].radius) - this.y,
                this.relatedVoronoiVertex[i].x - this.x);
            theta[i] = theta[i] * 180 / Mathf.PI;
        }

        // Sort by the angle.
        List<VoronoiVertexPoint> sortedVVP = new List<VoronoiVertexPoint>();
        for (int i = 0; i < size; i++)
        {
            int nextIndex = i;
            float max = float.MinValue;
            for (int j = 0; j < size; j++)
            {
                if (theta[j] > max)
                {
                    nextIndex = j;
                    max = theta[j];
                }
            }
            VoronoiVertexPoint nextPoint = this.relatedVoronoiVertex[nextIndex];
            sortedVVP.Add(nextPoint);
            theta[nextIndex] = float.MinValue;
        }

        this.relatedVoronoiVertex = sortedVVP;
    }

    public override string ToString()
    {
        String result = "Cell " + this.index + " " + this.relatedVoronoiVertex.Count + "\n";
        foreach (Site s in this.relatedVoronoiVertex)
            result += s + "\n";
        return result;
    }
}

public class Edge
{
    public Site face1;
    public Site face2;
    VoronoiVertexPoint start;
    VoronoiVertexPoint end;

    public Edge(Site a, Site b, VoronoiVertexPoint vvp)
    {
        this.face1 = a;
        this.face2 = b;
        this.start = vvp;
        this.end = null;
    }

    public bool Equals(Edge newEdge)
    {
        if ((this.face1.Equals(newEdge.face1) && this.face2.Equals(newEdge.face2))
            || (this.face1.Equals(newEdge.face2) && this.face2.Equals(newEdge.face1)))
            return true;
        return false;
    }

    public void SetEnd(VoronoiVertexPoint vvp)
    {
        this.end = vvp;
    }

    public float[] GetEdgeVector()
    {
        float[] result = new float[2];
        float[] mid = GetMidPoint();
        result[0] = mid[0] - this.start.x;
        result[1] = mid[1] - (this.start.y + this.start.radius);
        return result;
    }

    public float[] GetMidPoint()
    {
        float[] result = new float[2];
        result[0] = (this.face1.x + this.face2.x) / 2;
        result[1] = (this.face1.y + this.face2.y) / 2;
        return result;
    }

    public bool IsOpen()
    {
        return (this.end == null);
    }

    /// <summary>
    /// See if the point is on the right side of the line.
    /// </summary>
    /// <returns>True if the point is on the right side of the line.</returns>
    public bool Orient(Site s)
    {
        float[] linepq = new float[2];
        float[] linepr = new float[2];
        float[] q = GetMidPoint();
        linepq[0] = q[0] - this.start.x;
        linepq[1] = q[1] - (this.start.y + this.start.radius);
        linepr[0] = s.x - this.start.x;
        linepr[1] = s.y - (this.start.y + this.start.radius);

        float crossProduct = linepq[0] * linepr[1] - linepq[1] * linepr[0];
        if (crossProduct <= 0)
            return true;
        return false;
    }

    public float[] GetYaxisIntersection(float y)
    {
        float[] result = new float[2];
        float[] midpoint = GetMidPoint();
        float a, b;    // y = ax+b.
        if (midpoint[0] == this.start.x)
        {
            a = 1;
            result[0] = this.start.x;
        }
        else
        {
            a = (midpoint[1] - (this.start.y + this.start.radius)) / (midpoint[0] - this.start.x);
            b = midpoint[1] - a * midpoint[0];
            result[0] = (y - b) / a;
        }

        result[1] = y;
        return result;
    }

    public float[] GetXaxisIntersection(float x)
    {
        float[] result = new float[2];
        float[] midpoint = GetMidPoint();
        float a, b;    // y = ax+b.
        a = (midpoint[1] - (this.start.y + this.start.radius)) / (midpoint[0] - this.start.x);
        b = midpoint[1] - a * midpoint[0];

        result[0] = x;
        result[1] = a * x + b;
        return result;
    }

    public override string ToString()
    {
        String result = "E(" + this.face1 + "-" + this.face2 + ")";
        return result;
    }
}

public class EventQueue
{
    private List<Site> list;
    public int Count { get { return list.Count; } }

    public EventQueue()
    {
        list = new List<Site>();
    }

    public EventQueue(int count)
    {
        list = new List<Site>(count);
    }

    public void Enqueue(Site p)
    {
        list.Add(p);
        list = list.OrderByDescending(points => points.y).ToList();
    }

    public Site Dequeue()
    {
        Site maxP = Peek();
        list.RemoveAt(0);
        return maxP;
    }

    public void Remove(Site p)
    {
        if (p != null)
        {
            var itemToRemove = list.SingleOrDefault(r => (r.x == p.x && r.y == p.y));
            if (itemToRemove != null)
                list.Remove(itemToRemove);
        }
    }

    /// <summary>
    /// See if the target voronoi vertex already exist in the list.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsDuplicate(VoronoiVertexPoint p)
    {
        var itemToRemove = list.SingleOrDefault(r => (r.x == p.x && r.y == p.y));
        if (itemToRemove == null || itemToRemove.type == 0)
            return false;
        VoronoiVertexPoint item = (VoronoiVertexPoint)itemToRemove;

        // Same voronoi vertex.
        if (item != null && item.OnSameCircle(p))
            return true;
        return false;
    }

    public Site Peek()
    {
        if (Count == 0) throw new InvalidOperationException("Queue is empty.");
        return list.First();
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }
}

public class Node
{
    public Node Left;
    public Node Right;
    public Site s;

    public Node()
    {
        this.Left = null;
        this.Right = null;
        this.s = null;
    }

    public Node(Site s)
    {
        this.Left = null;
        this.Right = null;
        Set(s);
    }

    public void Set(Site s)
    {
        this.s = s;
    }
}

