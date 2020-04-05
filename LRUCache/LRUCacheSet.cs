using System;
using System.Collections.Generic;
using System.Threading;

namespace LRUCache
{
    public class LRUCacheSet<TCacheKey, TCacheValue>
    {
        internal sealed class LinkedNode
        {
            public TCacheKey Key { get; }
            public TCacheValue Value { get; set; }

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
        private readonly int capacity;
        public event EventHandler<string> Log;
        private SpinLock spinLock = new SpinLock();

        /// <summary>
        /// LRU 缓存容器
        /// </summary>
        /// <param name="capacity">缓存容量</param>
        public LRUCacheSet(int capacity = 100)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(nameof(capacity));
            }

            Log?.Invoke(this, $"创建 LRU 缓存容器[{this.GetHashCode():X}]：Capacity={capacity}");
            this.capacity = capacity;
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
            var lockSeed = false;
            if (!this.spinLock.IsHeldByCurrentThread)
            {
                this.spinLock.Enter(ref lockSeed);
            }

            LinkedNode currentNode = null;
            if (this.valueMap.ContainsKey(key))
            {
                Log?.Invoke(this, $"覆盖已有的键：{key}={value}");
                currentNode = this.valueMap[key];
                currentNode.Value = value;
                if (this.head == this.end)
                {
                    this.head = null;
                    this.end = null;
                }
                else if (this.head == currentNode)
                {
                    this.head = this.head.Next;
                    this.head.Previous.Next = null;
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
            else
            {
                Log?.Invoke(this, $"新增缓存：{key}={value}");
                currentNode = new LinkedNode(key, value);
                this.valueMap.Add(key, currentNode);
            }

            TCacheValue headValue = default;
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

                if (this.valueMap.Count > this.capacity)
                {
                    Log?.Invoke(this, $"缓存数量超过阈值...");
                    headValue = this.RemoveHead();
                }
            }
            if (lockSeed)
            {
                this.spinLock.Exit();
            }

            return headValue;
        }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="key"></param>
        /// <remarks>
        /// 如果当前节点是头结点，则直接移除头结点
        /// 如果当前节点是尾结点，则直接移除尾结点
        /// 否则，则移除当前节点，并修复前后节点的引用
        /// </remarks>
        public void Remove(TCacheKey key)
        {
            if (!this.valueMap.ContainsKey(key))
            {
                Log?.Invoke(this, $"无法删除不存在的Key：{key}");
                return;
            }

            var lockSeed = false;
            if (!this.spinLock.IsHeldByCurrentThread)
            {
                this.spinLock.Enter(ref lockSeed);
            }

            var currentNode = this.valueMap[key];
            this.valueMap.Remove(key);
            Log?.Invoke(this, $"删除缓存：{currentNode.Key}={currentNode.Value}");

            if (this.head == this.end)
            {
                this.head = null;
                this.end = null;
            }
            else if (this.head == currentNode)
            {
                this.head = this.head.Next;
                this.head.Previous.Next = null;
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
            if (lockSeed)
            {
                this.spinLock.Exit();
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
                Log?.Invoke(this, $"头结点为空，无需删除");
                return default;
            }

            var lockSeed = false;
            if (!this.spinLock.IsHeldByCurrentThread)
            {
                this.spinLock.Enter(ref lockSeed);
            }

            var headKey = this.head.Key;
            var headValue = this.valueMap[headKey].Value;
            this.valueMap.Remove(headKey);
            Log?.Invoke(this, $"删除头结点：{headKey}={headValue}");

            if (this.head == this.end)
            {
                this.head = null;
                this.end = null;
            }
            else
            {
                this.head = this.head.Next;
                this.head.Previous.Next = null;
            }
            if (lockSeed)
            {
                this.spinLock.Exit();
            }

            return headValue;
        }

        /// <summary>
        /// 最近使用了某个结点，将此节点移动到链表尾部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>
        /// 如果当前节点是尾结点，不用移动
        /// 如果当前节点是头结点，将头指针后移，并将当前节点移动到链表尾部
        /// 否则，修复当前节点前后节点的引用，并将当前节点移动到链表尾部
        /// </remarks>
        public TCacheValue Use(TCacheKey key)
        {
            if (!this.valueMap.ContainsKey(key))
            {
                Log?.Invoke(this, $"使用Key不存在的缓存：{key}");
                return default;
            }

            var lockSeed = false;
            if (!this.spinLock.IsHeldByCurrentThread)
            {
                this.spinLock.Enter(ref lockSeed);
            }

            var currentNode = this.valueMap[key];
            Log?.Invoke(this, $"使用缓存：{currentNode.Key}={currentNode.Value}");

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
            if (lockSeed)
            {
                this.spinLock.Exit();
            }

            return currentNode.Value;
        }

        /// <summary>
        /// 获取Key集合
        /// </summary>
        /// <returns></returns>
        public List<TCacheKey> GetKeyList()
        {
            var lockSeed = false;
            if (!this.spinLock.IsHeldByCurrentThread)
            {
                this.spinLock.Enter(ref lockSeed);
            }

            var currentNode = this.head;
            var result = new List<TCacheKey>();
            while (currentNode != null)
            {
                result.Add(currentNode.Key);
                currentNode = currentNode.Next;
            }
            if (lockSeed)
            {
                this.spinLock.Exit();
            }

            // Log?.Invoke(this, $"获取MapKey列表：\n\t{string.Join("\n\t", this.valueMap.Keys)}");
            Log?.Invoke(this, $"获取链表Key列表：\n\t{string.Join("\n\t", result)}");

            return result;
        }
    }
}
