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
    //static float currentY, lastY;
    static float boundaryLow, boundaryHigh, boundaryLeft, boundaryRight;
    static Cell[] allCells;
    static int count = 0;
    static List<VoronoiVertexPoint> voronoiVertex = new List<VoronoiVertexPoint>();

    /* Vector2[] test = new Vector2[5];
                test[0] = new Vector2(0.0f, 0.2f);
                test[1] = new Vector2(-0.3f, 0.5f);
                test[2] = new Vector2(0.3f, 0.5f);
                test[3] = new Vector2(0.6f, -0.7f);
                test[3] = new Vector2(-0.5f, -0.8f);
                
                Vector2[][] pieces = voronoi.GenerateVoronoi(test);*/

    public Vector2[][] GenerateVoronoi(Vector2[] points)
    {
        boundaryLow = -1;
        boundaryHigh = 1;
        boundaryLeft = -1;
        boundaryRight = 1;
        int numberOfCell = points.Length;

        // Read file.

        allCells = new Cell[numberOfCell];

        for (int i = 0; i < numberOfCell; i++)
        {
            float x = points[i].x;
            float y = points[i].y;
            Site newSite = new Site(0, i, x, y);
            eq.Enqueue(newSite);
            allCells[i] = new Cell(2, i, x, y);
        }

        float lastY = float.MaxValue;
        while (!eq.IsEmpty())
        {
            Site eventP = eq.Dequeue();
            VoronoiVertexPoint voronoiEventP;
            float voronoiRealY = boundaryLow - 1;
            if (eventP.GetType() == typeof(VoronoiVertexPoint))
            {
                voronoiEventP = (VoronoiVertexPoint) eventP;
                voronoiRealY = voronoiEventP.GetY();
            }

            float currentY = eventP.y;
            if (currentY <= boundaryLow && voronoiRealY < boundaryLow)
            {
                //Debug.Log("Reach lower boundary. " + currentY);
                break;
            }
            if (currentY > lastY && !Mathf.Approximately(currentY, lastY)) // approximately in case 0.2 > 0.2.
            {
                Debug.Log("Earlier event."+ currentY+" > "+ lastY);
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

        Vector2[][] OutputPoints = new Vector2[numberOfCell][];

        for (uint i = 0; i < numberOfCell; ++i)
        {
            Cell cell = allCells[i];
            cell.HandleEdges(boundaryHigh, boundaryLow, boundaryLeft, boundaryRight);
            cell.Sort();

            OutputPoints[i] = new Vector2[cell.relatedVoronoiVertex.Count];

            Debug.Log("#" + i + " : " + cell.relatedVoronoiVertex.Count);

            for (int j = 0; j < cell.relatedVoronoiVertex.Count; ++j)
            {
                OutputPoints[i][j] = new Vector2(cell.relatedVoronoiVertex[j].x, cell.relatedVoronoiVertex[j].y + cell.relatedVoronoiVertex[j].radius);
                Debug.Log("\t" + OutputPoints[i][j].ToString("F10"));
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
                float findRightDist = float.MaxValue;
                Node target = head;
                Node current = head;
                if (s.index == 3)
                    findRight = false;
                // Iterate until the node is on the new Site's right.
                while (current != null)
                {
                    parabola = GetParabolaIntersect(current.s.x, current.s.y, newNode.s.y, newNode.s.x);
                    // No intersection yet.
                    if (float.IsInfinity(parabola) && ymin == float.MaxValue)
                        target = current;
                    if (parabola <= ymin)
                    {
                        ymin = parabola;
                        target = current;
                    }

                    if (current.s.x > newNode.s.x)
                        findRight = true;
                    else
                        findRight = false;
                    float dist = current.s.x - newNode.s.x;
                    // The last node is the closest one.
                    if (findRight && dist > findRightDist)
                        break;
                    if (findRight)
                        findRightDist = dist;

                    current = current.Right;
                }
                if (ymin > boundaryHigh && s.x > target.s.x)
                    ADDRight(target, newNode);
                else if (ymin > boundaryHigh && s.x < target.s.x)
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
                if (tmpLeft.Right != null && tmpRight != null)
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

            if (float.IsInfinity(circleInfo[1]))   // y 
                return null;
            else
            { }
            VoronoiVertexPoint lowestP = new VoronoiVertexPoint(1, count++, circleInfo[0],
                circleInfo[1] - circleInfo[2], circleInfo[2], pi, pj, pk);
            return lowestP;
        }

        public bool Test269(int a, int b, int c, Site pi, Site pj, Site pk)
        {
            if (pi.index == a)
                if ((pj.index == b && pk.index == c) || (pj.index == c && pk.index == b))
                    return true;
            if (pi.index == b)
                if ((pj.index == a && pk.index == c) || (pj.index == c && pk.index == a))
                    return true;
            if (pi.index == c)
                if ((pj.index == a && pk.index == b) || (pj.index == b && pk.index == a))
                    return true;
            return false;
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
        public float[] GetCircle(Site pi, Site pj, Site pk)
        {
            // The order of three sites will affect the result.
            Site[] newSites = RearrangeSites(pi, pj, pk);
            Site p1 = newSites[0];
            Site p2 = newSites[1];
            Site p3 = newSites[2];

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
            // Round(x,12)
            result[0] = (float)System.Math.Round((double)x, 12);
            result[1] = (float)System.Math.Round((double)y, 12);
            result[2] = (float)System.Math.Round((double)r, 12);

            return result;
        }

        /// <summary>
        /// Rearrange the order if three sites by y.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public Site[] RearrangeSites(Site p1, Site p2, Site p3)
        {
            Site[] result = new Site[3];
            if (p1.GetY() >= p2.GetY() && p1.GetY() >= p3.GetY())
            {
                result[0] = p1;
                result[1] = (p2.GetY() > p3.GetY()) ? p2 : p3;
                result[2] = (p2.GetY() > p3.GetY()) ? p3 : p2;
            }
            else if (p2.GetY() >= p1.GetY() && p2.GetY() >= p3.GetY())
            {
                result[0] = p2;
                result[1] = (p1.GetY() > p3.GetY()) ? p1 : p3;
                result[2] = (p1.GetY() > p3.GetY()) ? p3 : p1;
            }
            else if (p3.GetY() >= p1.GetY() && p3.GetY() >= p2.GetY())
            {
                result[0] = p3;
                result[1] = (p1.GetY() > p2.GetY()) ? p1 : p2;
                result[2] = (p1.GetY() > p2.GetY()) ? p2 : p1;
            }

            return result;
        }

        /*
         * Bisector (pi, pj) and (pj, pk) have met, a single bisector (pi, pk) remains.
         */
        public void HandleVVEvent(VoronoiVertexPoint p)
        {
            var sameItem = voronoiVertex.SingleOrDefault(r => (p.OnSamePoint(r)));
            if (sameItem == null)
            {
                voronoiVertex.Add(p);
                // Take every site in the same circle and add them to voronoi vertex's related sites.
                foreach (Site s in allCells)
                {
                    if (p.OnSameCircle(s))
                        p.AddRelatedSite(s);
                }
                p.Sort();
                // Assign edge points to those cells one by one.
                for (int i = 0; i < p.relatedSites.Count; i++)
                {
                    int cellIndex1, cellIndex2;
                    cellIndex1 = p.relatedSites[i].index;
                    if (i != p.relatedSites.Count - 1)
                        cellIndex2 = p.relatedSites[i + 1].index;
                    else
                        cellIndex2 = p.relatedSites[0].index;

                    allCells[cellIndex1].AddEdgePoint(p, allCells[cellIndex2]);
                    allCells[cellIndex2].AddEdgePoint(p, allCells[cellIndex1]);
                }
                eq.RemoveAll(p);

                // Find where pj is.
                Node pj = head.Right;
                while (pj.Left != null && pj != null && pj.Right != null
                    && (!pj.Left.s.Equals(p.pi)
                    || !pj.s.Equals(p.pj)
                    || !pj.Right.s.Equals(p.pk)))
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
                    if (tmp != null && !tmp.OnSamePoint(p))
                        CheckCircleEvent(pj.Left.Left.s, pj.Left.s, pj.Right.s, true);
                }
                if (pj.Right.Right != null)
                {
                    CheckCircleEvent(pj.s, pj.Right.s, pj.Right.Right.s, false);
                    tmp = GetVoronoiVertexEvent(pj.Left.s, pj.Right.s, pj.Right.Right.s);
                    if (tmp != null && !tmp.OnSamePoint(p))
                        CheckCircleEvent(pj.Left.s, pj.Right.s, pj.Right.Right.s, true);
                }

                // Remove pj.
                pj.Left.Right = pj.Right;
                pj.Right.Left = pj.Left;

                if (p.relatedSites.Count > 3)
                    SpecialCase(p);
            }
        }

        /// <summary>
        /// This method removes extra sites from the beach line.
        /// </summary>
        /// <param name="p"></param>
        public void SpecialCase(VoronoiVertexPoint p)
        {
            Node current = head;
            while (!p.OnSameCircle(current.s))
                current = current.Right;
            Node start = current;

            // removes the upper half of the sites
            while (p.OnCircle(current.s) && p.OnCircle(current.Right.s))
            {
                if ((current.Right.s.x < current.s.x)
                    || current.Right.s.GetY() > p.GetY())
                {
                    current.SkipNextNode();
                }
                else
                    current = current.Right;
            }
            CheckCircleEvent(current.Left.s, current.s, current.Right.s, true);
        }

        public void Print()
        {
            Node current = head;
            while (current != null)
            {
                Console.Write(current.s.x + ", " + current.s.y + " -> ");
                current = current.Right;
            }
            Console.WriteLine();
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
    // Type: 0=site, 1=voronoi point, 2=cell point, 3=boundary intersect point, 4 for testing
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
        return OnSamePoint(p);
    }

    public bool OnSamePoint(Site p)
    {
        if ((this.x == p.x || Math.Abs(this.x - p.x) < 0.0000000000000001) &&
            (this.GetY() == p.GetY() || Math.Abs(this.GetY() - p.GetY()) < 0.0000000000000001))
            return true;
        return false;
    }

    /// <summary>
    /// This method should sort the list of surrounding sites in a clockwise order.
    /// The center is this site.
    /// </summary>


    public override string ToString()
    {
        return this.x + " " + this.y;
    }
}

public class VoronoiVertexPoint : Site
{
    public float radius;
    public List<Site> relatedSites;
    public Site pi, pj, pk;
    public VoronoiVertexPoint() { }

    public VoronoiVertexPoint(int type, int index, float x, float y, float r, Site pi, Site pj, Site pk)
    {
        this.radius = r;
        this.relatedSites = new List<Site>();
        this.pi = pi;
        this.pj = pj;
        this.pk = pk;
        AddRelatedSite(pi);
        AddRelatedSite(pj);
        AddRelatedSite(pk);
        SetPoint(type, index, x, y);
    }

    public float GetY()
    {
        return this.y + this.radius;
    }

    public void AddRelatedSite(Site s)
    {
        this.relatedSites.Add(s);
    }

    public bool Equals(VoronoiVertexPoint vvp)
    {
        if (this.pi.index == vvp.pi.index && this.pj.index == vvp.pj.index
            && this.pk.index == vvp.pk.index)
        {
            return true;
        }
        return false;
    }

    public bool Equals(Site p)
    {
        if (p.GetType() == typeof(VoronoiVertexPoint))
        {
            VoronoiVertexPoint vvp = (VoronoiVertexPoint)p;
            bool result = Equals(vvp);
            return result;
        }
        return base.Equals(p);
    }

    public bool OnSamePoint(VoronoiVertexPoint p)
    {
        if ((this.x == p.x || Math.Abs(this.x - p.x) < 0.0000000000000001) &&
            (this.GetY() == p.GetY() || Math.Abs(this.GetY() - p.GetY()) < 0.0000000000000001))
            return true;
        return false;
    }

    /// <summary>
    /// See if the target site is on this voronoi vertex's circle.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool OnSameCircle(Site s)
    {
        if (s.Equals(this.pi) || s.Equals(this.pj) || s.Equals(this.pk))
            return false;
        return OnCircle(s);
    }

    public bool OnCircle(Site s)
    {
        float dist = (s.x - this.x) * (s.x - this.x) + (s.GetY() - this.GetY()) * (s.GetY() - this.GetY());
        //dist = Mathf.Round(Mathf.Sqrt(dist), 12);
        dist = (float)System.Math.Round(Mathf.Sqrt(dist), 12);
        if (dist == this.radius)
            return true;
        return false;
    }

    /// <summary>
    /// This method should sort the related sites into a clockwise order.
    /// </summary>
    public void Sort()
    {
        int size = this.relatedSites.Count;

        // Compute and store the angle of every line.
        float[] theta = new float[size];
        for (int i = 0; i < size; i++)
        {
            theta[i] = Mathf.Atan2(this.relatedSites[i].GetY() - this.GetY(), this.relatedSites[i].x - this.x);
            theta[i] = theta[i] * 180f / Mathf.PI;
        }

        // Sort by the angle.
        List<Site> sortedPoints = new List<Site>();
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
            Site nextPoint = this.relatedSites[nextIndex];
            sortedPoints.Add(nextPoint);
            theta[nextIndex] = float.MinValue;
        }

        this.relatedSites = sortedPoints;
    }

    public override string ToString()
    {
        return this.x + " " + (this.y + this.radius);
    }
}

public class Cell : Site
{
    int count;
    public List<VoronoiVertexPoint> relatedVoronoiVertex;
    public List<Edge> edges;

    public Cell(int type, int index, float x, float y)
    {
        this.count = 0;
        this.relatedVoronoiVertex = new List<VoronoiVertexPoint>();
        this.edges = new List<Edge>();
        SetPoint(type, index, x, y);
    }

    public void AddEdgePoint(VoronoiVertexPoint vvp, Site neighbor)
    {
        // Do not add duplicate vvp in list.
        var itemToRemove = this.relatedVoronoiVertex.SingleOrDefault(r => (vvp.OnSamePoint(r)));
        if (itemToRemove == null)
            this.relatedVoronoiVertex.Add(vvp);

        Edge newEdge = new Edge(this, neighbor, vvp);
        bool edgePointExist = false;
        foreach (Edge e in this.edges)
        {
            // Start of the edge already exist.
            if (e.Equals(newEdge))
            {
                e.SetEnd(vvp);
                edgePointExist = true;
                break;
            }
        }
        if (!edgePointExist)
            this.edges.Add(newEdge);
    }

    /// <summary>
    /// See if any edge of this cell is open ended, if so, bound it.
    /// </summary>
    public void HandleEdges(float boundaryHigh, float boundaryLow, float boundaryLeft, float boundaryRight)
    {
        bool[] boundaryIntersect = new bool[4];
        Vector2 boundaryPoint;

        foreach (Edge e in this.edges)
        {
            if (e.IsOpen())
            {
                boundaryPoint = e.GetBoundaryIntersection(boundaryHigh, boundaryLow, boundaryLeft, boundaryRight);
                SetEdgeEnd(e, boundaryPoint);
                if (boundaryPoint.y == boundaryHigh)
                    boundaryIntersect[0] = true;
                else if (boundaryPoint.y == boundaryLow)
                    boundaryIntersect[1] = true;
                else if (boundaryPoint.x == boundaryLeft)
                    boundaryIntersect[2] = true;
                else if (boundaryPoint.x == boundaryRight)
                    boundaryIntersect[3] = true;
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

    public void SetEdgeEnd(Edge e, Vector2 boundaryIntersection)
    {
        // Both start and end point of an edge are VVP.
        VoronoiVertexPoint boundP = new VoronoiVertexPoint(3, count++, boundaryIntersection.x,
            boundaryIntersection.y, 0, e.face1, e.face2, null);
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
            theta[i] = Mathf.Atan2(this.relatedVoronoiVertex[i].GetY() - this.GetY(), this.relatedVoronoiVertex[i].x - this.x);
            theta[i] = theta[i] * 180 / Mathf.PI;
        }

        // Sort by the angle.
        List<VoronoiVertexPoint> sortedPoints = new List<VoronoiVertexPoint>();
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
            sortedPoints.Add(nextPoint);
            theta[nextIndex] = float.MinValue;
        }

        this.relatedVoronoiVertex = sortedPoints;
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

    public Site GetThirdFace() 
    {
        if ((this.face1.Equals(start.pi) && this.face2.Equals(start.pj))
            || (this.face1.Equals(start.pj) && this.face2.Equals(start.pi))) 
            return start.pk;
        if ((this.face1.Equals(start.pj) && this.face2.Equals(start.pk))
            || (this.face1.Equals(start.pk) && this.face2.Equals(start.pj)))
            return start.pi;
        return start.pj;
    }

    public bool IsOpen()
    {
        return (this.end == null);
    }

    /// <summary>
    /// See if the point is on the right side of the two faces.
    /// </summary>
    /// <returns>True if the point is on the right side of the two faces.</returns>
    public bool OrientFace(Site s) 
    {
        float[] linepq = new float[2];
        float[] linepr = new float[2];
        linepq[0] = this.face2.x - this.face1.x;
        linepq[1] = this.face2.GetY() - this.face1.GetY();
        linepr[0] = s.x - this.face1.x;
        linepr[1] = s.GetY() - this.face1.GetY();

        float crossProduct = linepq[0] * linepr[1] - linepq[1] * linepr[0];
        if (crossProduct <= 0)
            return true;
        return false;
    }

    /// <summary>
    /// This method will return the interection point of this edge and boundaries.
    /// </summary>
    /// <param name="high"></param>
    /// <param name="low"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public Vector2 GetBoundaryIntersection(float high, float low, float left, float right) 
    {
        Vector2 faceLine1 = new Vector2(this.face2.x - this.face1.x, this.face2.GetY() - this.face1.GetY());             
        Vector2 perpendicular1 = Vector2.Perpendicular(faceLine1);
        Vector2 faceLine2 = new Vector2(this.face1.x - this.face2.x, this.face1.GetY() - this.face2.GetY());             
        Vector2 perpendicular2 = Vector2.Perpendicular(faceLine2);

        // If the third face is on the opposite direction of this edge, use perpendicular line 1.
        Site thirdFace = GetThirdFace();
        if (OrientFace(thirdFace))
            return GetBoundaryIntersectionHelper(high, low, left, right, perpendicular1);
        else
            return GetBoundaryIntersectionHelper(high, low, left, right, perpendicular2);
    }

    public Vector2 GetBoundaryIntersectionHelper(float high, float low, float left, float right, Vector2 vector)
    {
        Vector2 result;

        if (vector.y == 0)  // Horizontal vector.
        {
            result.y = this.start.GetY();
            if (vector.x > 0) 
                result.x = right;
            else 
                result.x = left;
            return result;
        } 
        else if (vector.x == 0) // Vertical vector.
        {
            result.x = this.start.x;
            if (vector.y > 0)
                result.y = high;
            else
                result.y = low;
            return result;
        }

        // y = a*x + b.
        Vector2 highIntersect, lowIntersect, leftIntersect, rightIntersect;
        float a = vector.y / vector.x;
        float b = this.start.GetY() - a * this.start.x;
        highIntersect = new Vector2((high - b) / a, high);
        lowIntersect = new Vector2((low - b) / a, low);
        leftIntersect = new Vector2(left, a * left + b);
        rightIntersect = new Vector2(right, a * right + b);

        if (vector.x > 0 && vector.y > 0) 
        {
            if (highIntersect.x > right)
                return rightIntersect;
            return highIntersect;
        }
        else if(vector.x > 0 && vector.y < 0) 
        {
            if(lowIntersect.x > right)
                return rightIntersect;
            return lowIntersect;
        }
        else if(vector.x < 0 && vector.y < 0) 
        {
            if(lowIntersect.x < left)
                return leftIntersect;
            return lowIntersect;
        }
        else if(vector.x < 0 && vector.y > 0) 
        {
            if(highIntersect.x < left)
                return leftIntersect;
            return highIntersect;
        }
        
        // Should never reach this line.
        return new Vector2(0, 0);
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

    public void Remove(VoronoiVertexPoint vvp)
    {
        if (vvp != null)
        {
            var itemToRemove = list.SingleOrDefault(r => (vvp.Equals(r)));
            if (itemToRemove != null)
                list.Remove(itemToRemove);
        }
    }

    /// <summary>
    /// After handling circle event, all other circle events at the same point should be removed. 
    /// </summary>
    /// <param name="vvp"></param>
    public void RemoveAll(VoronoiVertexPoint vvp)
    {
        list.RemoveAll(r => (vvp.OnSamePoint(r)));
    }

    /// <summary>
    /// See if the target voronoi vertex already exist in the list.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsDuplicate(VoronoiVertexPoint vvp)
    {
        var itemToRemove = list.SingleOrDefault(r => (vvp.Equals(r)));
        if (itemToRemove == null || itemToRemove.type == 0)
            return false;
        return true;
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

    public void SkipNextNode()
    {
        if (this.Right != null && this.Right.Right != null)
        {
            Node newNext = this.Right.Right;
            this.Right = newNext;
            newNext.Left = this;
        }
    }
}
