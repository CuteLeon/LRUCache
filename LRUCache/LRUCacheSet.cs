using System;
using System.Collections.Generic;
using System.Text;

namespace LRUCache
{
    public class LRUCacheSet<TCacheValue>
    {
        internal sealed class LinkedNode
        {
            public TCacheValue Value { get; }

            public LinkedNode(TCacheValue value)
            {
                this.Value = value;
            }

            internal LinkedNode Previous { get; set; }
            internal LinkedNode Next { get; set; }
        }

        Dictionary<TCacheValue, LinkedNode> valueMap = new Dictionary<TCacheValue, LinkedNode>();
        LinkedNode head = null;
        LinkedNode end = null;

        public LRUCacheSet()
        {
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// 如果链表头和尾为空，此时需要建立头和尾指向新建的节点
        /// 如果存在链表头和尾，将新节点追加到链表尾部
        /// </remarks>
        public void Add(TCacheValue value)
        {
            if (valueMap.ContainsKey(value)) return;

            var currentNode = new LinkedNode(value);
            valueMap.Add(value, currentNode);

            if (head == null)
            {
                head = currentNode;
                end = head;
            }
            else
            {
                end.Next = currentNode;
                end.Next.Previous = end;
                end = end.Next;
            }
        }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// 如果当前节点是头结点，则直接移除头结点（RemoveHead() 方法会处理只有一个节点的情况），
        /// 如果当前节点是尾结点，则直接移除尾结点
        /// 否则，则移除当前节点，并修复前后节点的引用
        /// </remarks>
        public void Remove(TCacheValue value)
        {
            if (!valueMap.ContainsKey(value)) return;

            var currentNode = valueMap[value];
            valueMap.Remove(value);

            if (head == currentNode)
            {
                RemoveHead();
            }
            else if (end == currentNode)
            {
                end = end.Previous;
                end.Next.Previous = null;
                end.Next = null;
            }
            else
            {
                currentNode.Next.Previous = currentNode.Previous;
                currentNode.Previous.Next = currentNode.Next;
                currentNode.Previous = null;
                currentNode.Next = null;
            }
        }

        /// <summary>
        /// 移除头结点
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 如果只有这一个节点，则直接置空首尾结点
        /// 否则移除头部结点，并将头指针后移
        /// </remarks>
        public TCacheValue RemoveHead()
        {
            if (head == null) return default;

            var headValue = head.Value;
            this.valueMap.Remove(headValue);
            if (head == end)
            {
                head = null;
                end = null;
            }
            else
            {
                head = head.Next;
                head.Previous.Next = null;
                head.Next = null;
            }

            return headValue;
        }

        /// <summary>
        /// 最近使用了某个结点，将此节点移动到链表尾部
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// 如果当前节点是尾结点，不用移动
        /// 如果当前节点是头结点，将头指针后移，并将当前节点移动到链表尾部
        /// 否则，修复当前节点前后节点的引用，并将当前节点移动到链表尾部
        /// </remarks>
        public void Use(TCacheValue value)
        {
            if (!this.valueMap.ContainsKey(value)) return;

            var currentNode = this.valueMap[value];

            if (end == currentNode) return;
            if (currentNode == head)
            {
                head = head.Next;
                head.Previous = null;
            }
            else
            {
                currentNode.Previous.Next = currentNode.Next;
                currentNode.Next.Previous = currentNode.Previous;
            }

            end.Next = currentNode;
            end.Next.Previous = end;
            end = end.Next;
            end.Next = null;
        }
    }
}
