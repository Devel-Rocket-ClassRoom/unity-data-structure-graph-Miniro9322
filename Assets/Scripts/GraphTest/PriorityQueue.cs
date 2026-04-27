using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    public int Count => queue.Count;

    private List<(TElement Element, TPriority Priority)> queue;
    private readonly IComparer<TPriority> comparer;

    public PriorityQueue()
    {
        queue = new();
        comparer = Comparer<TPriority>.Default;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        queue.Add((element, priority));

        HeapifyUp(queue.Count - 1);
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            var parentIndex = (index - 1) / 2;

            if (comparer.Compare(queue[index].Priority, queue[parentIndex].Priority) < 0)
            {
                var temp = queue[index];
                queue[index] = queue[parentIndex];
                queue[parentIndex] = temp;
            }

            index = parentIndex;
        }
    }


    public TElement Dequeue()
    {
        if (queue.Count == 0)
        {
            throw new ArgumentNullException();
        }

        var value = queue[0].Element;

        queue[0] = queue[Count - 1];
        queue.RemoveAt(Count - 1);

        HeapifyDown(0);
        return value;
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            var leftChildIndex = 2 * index + 1;
            var rightChildIndex = 2 * index + 2;
            var smallNodeIndex = index;

            if (leftChildIndex < Count && comparer.Compare(queue[smallNodeIndex].Priority, queue[leftChildIndex].Priority) > 0)
            {
                smallNodeIndex = leftChildIndex;
            }
            if (rightChildIndex < Count && comparer.Compare(queue[smallNodeIndex].Priority, queue[rightChildIndex].Priority) > 0)
            {
                smallNodeIndex = rightChildIndex;
            }
            if (smallNodeIndex == index)
            {
                break;
            }

            var temp = queue[index];
            queue[index] = queue[smallNodeIndex];
            queue[smallNodeIndex] = temp;

            index = smallNodeIndex;
        }
    }

    public TElement Peek()
    {
        return queue[0].Element;
    }

    public void Clear()
    {
        queue.Clear();
    }
}
