# LRUCache
LRU (Least Recently Used) 最近最少使用算法

## 并发安全

​	使用自旋锁 SpinLock 在频繁小任务场景下保证效率和并发安全；