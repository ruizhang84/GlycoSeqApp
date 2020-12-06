using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlycoSeqClassLibrary.algorithm
{
    public class PriorityQueue<T>
    {
        class Node
        {
            public double Priority { get; set; }
            public T Object { get; set; }
        }

        //object array
        List<Node> queue = new List<Node>();
        int heapSize = -1;
        bool _isMinPriorityQueue = true;
        public int Count { get { return queue.Count; } }

        /// <summary>
        /// If min queue or max queue
        /// </summary>
        /// <param name="isMinPriorityQueue"></param>
        public PriorityQueue(bool isMinPriorityQueue = true)
        {
            _isMinPriorityQueue = isMinPriorityQueue;
        }


        public void Enqueue(double priority, T obj)
        {
            Node node = new Node() { Priority = priority, Object = obj };
            queue.Add(node);
            heapSize++;
            //Maintaining heap
            if (_isMinPriorityQueue)
                BuildHeapMin(heapSize);
            else
                BuildHeapMax(heapSize);
        }
        public T Dequeue()
        {
            if (heapSize > -1)
            {
                var returnVal = queue[0].Object;
                queue[0] = queue[heapSize];
                queue.RemoveAt(heapSize);
                heapSize--;
                //Maintaining lowest or highest at root based on min or max queue
                if (_isMinPriorityQueue)
                    MinHeapify(0);
                else
                    MaxHeapify(0);
                return returnVal;
            }
            else
                throw new Exception("Queue is empty");
        }
        public void UpdatePriority(T obj, double priority)
        {
            int i = 0;
            for (; i <= heapSize; i++)
            {
                Node node = queue[i];
                if (object.ReferenceEquals(node.Object, obj))
                {
                    node.Priority = priority;
                    if (_isMinPriorityQueue)
                    {
                        BuildHeapMin(i);
                        MinHeapify(i);
                    }
                    else
                    {
                        BuildHeapMax(i);
                        MaxHeapify(i);
                    }
                }
            }
        }
        public bool IsInQueue(T obj)
        {
            foreach (Node node in queue)
                if (object.ReferenceEquals(node.Object, obj))
                    return true;
            return false;
        }

        private void BuildHeapMax(int i)
        {
            while (i >= 0 && queue[(i - 1) / 2].Priority < queue[i].Priority)
            {
                Swap(i, (i - 1) / 2);
                i = (i - 1) / 2;
            }
        }
        private void BuildHeapMin(int i)
        {
            while (i >= 0 && queue[(i - 1) / 2].Priority > queue[i].Priority)
            {
                Swap(i, (i - 1) / 2);
                i = (i - 1) / 2;
            }
        }
        private void MaxHeapify(int i)
        {
            int left = ChildL(i);
            int right = ChildR(i);

            int heighst = i;

            if (left <= heapSize && queue[heighst].Priority < queue[left].Priority)
                heighst = left;
            if (right <= heapSize && queue[heighst].Priority < queue[right].Priority)
                heighst = right;

            if (heighst != i)
            {
                Swap(heighst, i);
                MaxHeapify(heighst);
            }
        }
        private void MinHeapify(int i)
        {
            int left = ChildL(i);
            int right = ChildR(i);

            int lowest = i;

            if (left <= heapSize && queue[lowest].Priority > queue[left].Priority)
                lowest = left;
            if (right <= heapSize && queue[lowest].Priority > queue[right].Priority)
                lowest = right;

            if (lowest != i)
            {
                Swap(lowest, i);
                MinHeapify(lowest);
            }
        }

        private void Swap(int i, int j)
        {
            var temp = queue[i];
            queue[i] = queue[j];
            queue[j] = temp;
        }
        private int ChildL(int i)
        {
            return i * 2 + 1;
        }
        private int ChildR(int i)
        {
            return i * 2 + 2;
        }
    }
}
