using System.Collections.Generic;

namespace LRUCache
{
    public class LRUCacheSet<TCacheKey, TCacheValue>
    {
        internal sealed class LinkedNode
        {
            public TCacheKey Key { get; }
            public TCacheValue Value { get; }

            public LinkedNode(TCacheKey key, TCacheValue value)
            {
                this.Key = key;
                this.Value = value;
            }

            internal LinkedNode Previous { get; set; }
            internal LinkedNode Next { get; set; }
        }

        private readonly Dictionary<TCacheKey, LinkedNode> valueMap = new Dictionary<TCacheKey, LinkedNode>();
        private LinkedNode head = null;
        private LinkedNode end = null;
        private readonly int capccity;

        /// <summary>
        /// LRU 缓存容器
        /// </summary>
        /// <param name="capccity">缓存容量</param>
        public LRUCacheSet(int capccity = 100)
        {
            this.capccity = capccity;
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>可能删除的头结点</returns>
        /// <remarks>
        /// 如果链表头和尾为空，此时需要建立头和尾指向新建的节点
        /// 如果存在链表头和尾，将新节点追加到链表尾部
        /// </remarks>
        public TCacheValue Add(TCacheKey key, TCacheValue value)
        {
            if (this.valueMap.ContainsKey(key))
            {
                return default;
            }

            TCacheValue headValue = default;
            if (this.valueMap.Count == this.capccity)
            {
                headValue = this.RemoveHead();
            }

            var currentNode = new LinkedNode(key, value);
            this.valueMap.Add(key, currentNode);

            if (this.head == null)
            {
                this.head = currentNode;
                this.end = this.head;
            }
            else
            {
                this.end.Next = currentNode;
                this.end.Next.Previous = this.end;
                this.end = this.end.Next;
            }

            return headValue;
        }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="key"></param>
        /// <remarks>
        /// 如果当前节点是头结点，则直接移除头结点（RemoveHead() 方法会处理只有一个节点的情况），
        /// 如果当前节点是尾结点，则直接移除尾结点
        /// 否则，则移除当前节点，并修复前后节点的引用
        /// </remarks>
        public void Remove(TCacheKey key)
        {
            if (!this.valueMap.ContainsKey(key))
            {
                return;
            }

            var currentNode = this.valueMap[key];
            this.valueMap.Remove(key);

            if (this.head == currentNode)
            {
                this.RemoveHead();
            }
            else if (this.end == currentNode)
            {
                this.end = this.end.Previous;
                this.end.Next.Previous = null;
                this.end.Next = null;
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
            if (this.head == null)
            {
                return default;
            }

            var headKey = this.head.Key;
            var headValue = this.valueMap[headKey].Value;
            this.valueMap.Remove(headKey);
            if (this.head == this.end)
            {
                this.head = null;
                this.end = null;
            }
            else
            {
                this.head = this.head.Next;
                this.head.Previous.Next = null;
                this.head.Next = null;
            }

            return headValue;
        }

        /// <summary>
        /// 最近使用了某个结点，将此节点移动到链表尾部
        /// </summary>
        /// <param name="key"></param>
        /// <remarks>
        /// 如果当前节点是尾结点，不用移动
        /// 如果当前节点是头结点，将头指针后移，并将当前节点移动到链表尾部
        /// 否则，修复当前节点前后节点的引用，并将当前节点移动到链表尾部
        /// </remarks>
        public TCacheValue Use(TCacheKey key)
        {
            if (!this.valueMap.ContainsKey(key))
            {
                return default;
            }

            var currentNode = this.valueMap[key];

            if (this.end == currentNode)
            {
                return currentNode.Value;
            }

            if (currentNode == this.head)
            {
                this.head = this.head.Next;
                this.head.Previous = null;
            }
            else
            {
                currentNode.Previous.Next = currentNode.Next;
                currentNode.Next.Previous = currentNode.Previous;
            }

            this.end.Next = currentNode;
            this.end.Next.Previous = this.end;
            this.end = this.end.Next;
            this.end.Next = null;
            return currentNode.Value;
        }

        /// <summary>
        /// 获取链表的Key集合
        /// </summary>
        /// <returns></returns>
        public List<TCacheKey> GetLinkedList()
        {
            var currentNode = head;
            var result = new List<TCacheKey>();
            while (currentNode != null)
            {
                result.Add(currentNode.Key);
                currentNode = currentNode.Next;
            }
            return result;
        }
    }
}
