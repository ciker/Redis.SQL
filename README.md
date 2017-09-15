# Redis Data Structures

## Strings

Strings are the most basic kind of Redis value. A String value can be at max 512 Megabytes in length.

## Hashsets

Redis Hashes are maps between string fields and string values

## Lists

A list is a collection of strings sorted by insertion order. It is possible to add elements to a Redis List pushing new elements on the head (on the left) or on the tail (on the right) of the list. Accessing elements is very fast near the extremes of the list but is slow if you try accessing the middle of a very big list, as it is an O(N) operation.

## Sets

Redis Sets are an unordered collection of Strings. It is possible to add, remove, and test for existence of members in O(1) time. Redis Sets have the desirable property of not allowing repeated members. Adding the same element multiple times will result in a set having a single copy of this element. Practically speaking this means that adding a member does not require a check if exists then add operation.

## Sorted Sets

Redis Sorted Sets are, similarly to Redis Sets, non repeating collections of Strings. The difference is that every member of a Sorted Set is associated with score, that is used in order to take the sorted set ordered, from the smallest to the greatest score. While members are unique, scores may be repeated.
With sorted sets you can add, remove, or update elements in a very fast way (in a time proportional to the logarithm of the number of elements). Since elements are taken in order and not ordered afterwards, you can also get ranges by score or by rank (position) in a very fast way. Accessing the middle of a sorted set is also very fast, so you can use Sorted Sets as a smart list of non repeating elements where you can quickly access everything you need: elements in order, fast existence test, fast access to elements in the middle!

# Mutexes & Semaphores:

## Mutex:

A Mutex is a key to a room. One person can have the key to occupy the room at the time. When finished, the person gives (frees) the key to the next person in the queue.

Mutexes are typically used to serialise access to a section of re-entrant code that cannot be executed concurrently by more than one thread. A mutex object only allows one thread into a controlled section, forcing other threads which attempt to gain access to that section to wait until the first thread has exited from that section.

## Semaphore:

A semaphore is the number of free identical room keys. For example, say that we have four rooms with identical locks and keys. The semaphore count - the count of keys - is set to 4 at beginning (all four rooms are free), then the count value is decremented as people are coming into the room. If all rooms are full, ie. there are no free keys left, the semaphore count is 0. Now, when one person leaves a room, the semaphore value is increased to 1 (one free key), and given to the next person in the queue.

A semaphore restricts the number of simultaneous users of a shared resource up to a maximum number. Threads can request access to the resource (decrementing the semaphore), and can signal that they have finished using the resource (incrementing the semaphore).

One key difference between a semaphore and a mutex is that a mutex can be unlocked only by the entity that has locked it, a semaphore doesn't have this restriction.

# Lock

In several instances you will face a situation where you have to lock access to blocks of code. For example, assume that you have a block of code which reads data from a data source, appends a value to this data and writes it back. Assume that thread A has read a data field, modified it and just when it was about to store the new field, the context has switched to thread B which read the same data field, modified it and stored the new value, therefore when thread A runs again, it will overwrite the value stored by thread B and thread B's changes will be lost. This is a very valid scenario when coding Web APIs where multiple requests activate multiple threads and you can have race conditions for specific resources.

C# solves this problem by providing the lock keyword where you can lock a block of code using a static object to restrict access to the code. A lock will automatically get freed once the block of code finishes execution.

# Lock and Await

Notice that awaiting a Task inside a locked code block will yield a compilation error since the CSC doesn't allow the await keyword inside lock statements. This restriction is due to the fact that awaiting Tasks inside a locked code block could lead to deadlocks (for example: throwing an exception that hasn't been caught). Therefore, the key to solving this problem is guaranteeing that the lock will get freed, here is where Mutexes can be used directly by developers instead of the lock keyword to block access to blocks of code that is placed within a 'try' block, and in the 'finally' block you must free the mutex. This assures that the lock will be freed even if an exception is thrown, an example for this is shown in '/Redis.SQL.Client/Semaphores.cs'