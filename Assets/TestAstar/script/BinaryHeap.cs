using System;
using System.Collections.Generic;
using UnityEngine;
namespace BinaryHeap
{
    /// <summary>  
    /// The heap's order  
    /// </summary>  
    public enum Order
    {
        ASC = 0,
        DESC = 1
    }

    /// <summary>  
    /// The BinaryHeap  
    /// </summary>  
    /// <typeparam name="T">The T represent the type of the heap's value</typeparam>  
    public class BinaryHeap<T> where T : IComparable<T>
    {
        /// <summary>  
        /// The size of the heap  
        /// </summary>  
        public int Size { get; set; }

        private int length;

        /// <summary>
        /// 默认值
        /// </summary>
        private T defaultValue;
        /// <summary>  
        /// The length of the heap  
        /// </summary>  
        public int Length
        {
            get
            {
                return length;
            }
            private set { length = value; }
        }

        private T[] Items { get; set; }
        private Order Order = Order.ASC;

        /// <summary>  
        /// The Cons of the heap  
        /// </summary>  
        /// <param name="size">The default size of the heap</param>  
        /// <param name="order">The order of the heap</param>  
        public BinaryHeap(int size, Order order)
        {
            if (size < 1)
            {
                throw new Exception("The size should be greater or equal than 1.");
            }

            this.Size = size;
            this.Order = order;
            // We don't need the Items[0], so the actually size is (this.Size + 1),  
            // and we just use the the value of Items[1], Items[2], Items[3]... and so on  
            this.Size++;
            Items = new T[this.Size];

            // Set to 0 represent the heap's length is empty  
            this.length = 0;
        }

        /// <summary>  
        /// Add new item to the heap  
        /// </summary>  
        /// <param name="item"></param>  
        public void Push(T item)
        {
            if (this.length == 0)
            {
                Items[1] = item;
            }
            else
            {
                int len = this.length;
                if (len + 1 >= this.Size)
                {
                    throw new Exception("The heap is fulfilled, can't add item anymore." + len);
                }

                // Set the new item at the end of this heap  
                int endPos = len + 1;
                try
                {
                    Items[endPos] = item;
                }
                catch
                {
                    int j = 0;
                }
                // Calculate the new item's parent position  
                int parentPos = endPos / 2;
                bool isContinue = true;
                while (parentPos != 0 && isContinue)
                {
                    // Compare the new added item and its parent, swap each other if needed  
                    if (Order == BinaryHeap.Order.ASC)
                    {
                        if (Items[endPos].CompareTo(Items[parentPos]) < 0)
                        {
                            Swap(ref Items[endPos], ref Items[parentPos]);

                            endPos = parentPos;
                            parentPos = endPos / 2;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else
                    {
                        if (Items[endPos].CompareTo(Items[parentPos]) > 0)
                        {
                            Swap(ref Items[endPos], ref Items[parentPos]);

                            endPos = parentPos;
                            parentPos = endPos / 2;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                }
            }

            // After the new item added, set the heap's length added by one  
            this.length++;
        }

        /// <summary>  
        /// Remove the top item from the heap  
        /// </summary>  
        /// <returns>if the order is ASC, return the smallest one, if DESC, return the largest one.</returns>  
        public T Pop()
        {
            if (this.length == 0)
            {
                throw new Exception("The heap is empty");
            }

            // Remove the first item and move the last item to the first  
            T removedItem = Items[1];
            int len = this.length;
            if (len == 1)
            {
                len = 0;
            }
            Items[1] = Items[len];
            // After the top item removed, set the heap's length reduced by one  
            this.length--;
            // Get the removing item's childrens's position  
            int currentPos = 1;
            int leftChildPos = currentPos * 2;
            int rightChildPos = currentPos * 2 + 1;

            // Set the while loop continue or not flag  
            bool isContinue = true;

            while ((leftChildPos <= len || rightChildPos <= len) && isContinue)
            {
                // Compare the removing item to its childrens, swap each other if needed  
                if (Order == BinaryHeap.Order.ASC)
                {
                    #region Order == BinaryHeap.Order.ASC  
                    if (leftChildPos <= len && rightChildPos <= len)
                    {
                        if (Items[leftChildPos].CompareTo(Items[rightChildPos]) < 0 && Items[currentPos].CompareTo(Items[leftChildPos]) >= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[leftChildPos]);
                            currentPos = leftChildPos;
                        }
                        else if (Items[leftChildPos].CompareTo(Items[rightChildPos]) >= 0 && Items[currentPos].CompareTo(Items[rightChildPos]) >= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[rightChildPos]);
                            currentPos = rightChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else if (leftChildPos <= len)
                    {
                        if (Items[currentPos].CompareTo(Items[leftChildPos]) >= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[leftChildPos]);
                            currentPos = leftChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else if (rightChildPos <= len)
                    {
                        if (Items[currentPos].CompareTo(Items[rightChildPos]) >= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[rightChildPos]);
                            currentPos = rightChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else
                    {
                        isContinue = false;
                    }
                    #endregion

                    leftChildPos = currentPos * 2;
                    rightChildPos = currentPos * 2 + 1;
                }
                else
                {
                    #region Order == BinaryHeap.Order.DESC  
                    if (leftChildPos <= len && rightChildPos <= len)
                    {
                        if (Items[leftChildPos].CompareTo(Items[rightChildPos]) > 0 && Items[currentPos].CompareTo(Items[leftChildPos]) <= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[leftChildPos]);
                            currentPos = leftChildPos;
                        }
                        else if (Items[leftChildPos].CompareTo(Items[rightChildPos]) <= 0 && Items[currentPos].CompareTo(Items[rightChildPos]) <= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[rightChildPos]);
                            currentPos = rightChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else if (leftChildPos <= len)
                    {
                        if (Items[currentPos].CompareTo(Items[leftChildPos]) <= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[leftChildPos]);
                            currentPos = leftChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else if (rightChildPos <= len)
                    {
                        if (Items[currentPos].CompareTo(Items[rightChildPos]) <= 0)
                        {
                            Swap(ref Items[currentPos], ref Items[rightChildPos]);
                            currentPos = rightChildPos;
                        }
                        else
                        {
                            isContinue = false;
                        }
                    }
                    else
                    {
                        isContinue = false;
                    }
                    #endregion

                    leftChildPos = currentPos * 2;
                    rightChildPos = currentPos * 2 + 1;
                }
            }

            return removedItem;
        }

        /// <summary>  
        /// Sort the heap  
        /// </summary>  
        /// <returns>Return the sorted heap array</returns>  
        public IEnumerable<T> Sort()
        {
            if (this.length == 0)
            {
                throw new Exception("The heap is empty");
            }

            while (this.length > 0)
            {
                yield return Pop();
            }
        }

        /// <summary>
        /// 判断对象是否在二叉堆中
        /// </summary>
        /// <param name="t">判断对象</param>
        /// <returns>对象</returns>
        public T Exist(T t)
        {
            T tmpItem;
            for (int i = 0; i < Items.Length; i++)
            {
                tmpItem = Items[i];
                if (tmpItem != null && tmpItem.CompareTo(t) == 0)
                {
                    Debug.Log("True");
                    return Items[i];
                }
            }
            Debug.Log("False");
            return defaultValue;
        }

        #region Private method  
        /// <summary>  
        /// Swap each other  
        /// </summary>  
        /// <param name="t1">The first one</param>  
        /// <param name="t2">The second one</param>  
        private void Swap(ref T t1, ref T t2)
        {
            T temp = t1;
            t1 = t2;
            t2 = temp;
        }
        #endregion
    }
}