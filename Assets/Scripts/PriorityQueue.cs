using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    private List<(TElement Element, TPriority Priority)> heap;
    private readonly IComparer<TPriority> comparer;

    public PriorityQueue()
    {
        heap = new List<(TElement, TPriority)>();
        comparer = Comparer<TPriority>.Default;
    }

    public PriorityQueue(IComparer<TPriority> comparer)
    {
        heap = new List<(TElement, TPriority)>();
        this.comparer = comparer ?? Comparer<TPriority>.Default;
    }

    public int Count => heap.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        heap.Add((element, priority));

        HeapifyUp(heap.Count - 1);
    }

    public TElement Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("큐가 비어있습니다.");
        }

        TElement result = heap[0].Element;

        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
        {
            HeapifyDown(0);
        }

        return result;
    }

    public TElement Peek()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("큐가 비어있습니다.");
        }

        return heap[0].Element;
    }

    public void Clear()
    {
        heap.Clear();
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            // 부모 인덱스 = (i - 1) / 2
            int parentIndex = (index - 1) / 2;

            if (comparer.Compare(heap[index].Priority, heap[parentIndex].Priority) >= 0)
            {
                break;
            }

            Swap(index, parentIndex);

            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = heap.Count - 1;

        while (true)
        {
            // 좌/우 자식 인덱스 = 2i + 1, 2i + 2
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int smallestIndex = index;

            if (leftChildIndex <= lastIndex &&
                comparer.Compare(heap[leftChildIndex].Priority, heap[smallestIndex].Priority) < 0)
            {
                smallestIndex = leftChildIndex;
            }

            if (rightChildIndex <= lastIndex &&
                comparer.Compare(heap[rightChildIndex].Priority, heap[smallestIndex].Priority) < 0)
            {
                smallestIndex = rightChildIndex;
            }

            if (smallestIndex == index)
            {
                break;
            }

            Swap(index, smallestIndex);

            index = smallestIndex;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}