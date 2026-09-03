# OS Case Study 2 — Thread-Safe Bounded Queue

**Subject:** Operating Systems (KMITL Year 3)  
**Points:** 15% (group work)  
**Due:** September 11, 2026

---

## Problem

`Program.cs` ships with an **unsynchronized** 10-slot ring buffer shared across 5 threads:

| Thread | Role | Count | Sleep |
|--------|------|-------|-------|
| `th01` | Producer — enqueues 1..50 | 1 | 5 ms |
| `th011` | Producer — enqueues 100..150 | 1 | 7 ms |
| `th02` | Consumer — dequeues 60× | 3 (t2, t21, t22) | 16 ms |

**Race hazards in the baseline:**
- Two producers write `Back`/`Count` unsynchronized → lost/overwritten slots
- Three consumers read `Front`/`Count` unsynchronized → duplicate/skipped reads
- No full/empty blocking → consumers return stale data; producers overwrite unread slots

## Solution

Single `lock` object + `Monitor.Wait`/`PulseAll` (Mesa semantics, .NET).

```csharp
static readonly object lockObj = new object();

static void EnQueue(int eq, object t)
{
    lock (lockObj)
    {
        while (Count == TSBuffer.Length)
        {
            Console.WriteLine("...........[Thread-{0}]:Queue full, waiting...........", t);
            Monitor.Wait(lockObj);
        }
        TSBuffer[Back] = eq;
        Back = (Back + 1) % TSBuffer.Length;
        Count += 1;
        Monitor.PulseAll(lockObj);
    }
}

static void DeQueueAndPrint(object t)
{
    lock (lockObj)
    {
        while (Count == 0)
            Monitor.Wait(lockObj);

        int x = TSBuffer[Front];
        Front = (Front + 1) % TSBuffer.Length;
        Count -= 1;
        Console.WriteLine("j={0}, thread:{1}", x, t);
        Monitor.PulseAll(lockObj);
    }
}
```

### Why `while`, not `if`

.NET `Monitor` uses **Mesa semantics**: `PulseAll` moves waiters to the ready queue, but the waiter re-acquires the lock in an unspecified order. By the time it runs, another thread may have already consumed the slot. Rechecking the predicate (`while`) is mandatory.

### Why `PulseAll`, not `Pulse`

C# `lock`/`Monitor` exposes **one wait queue per lock object**, shared by both producers and consumers. `Pulse` wakes one arbitrary waiter — if it wakes the wrong role (e.g. another producer when the queue is still full), that wakeup is consumed and the thread that could actually proceed stays parked. That is a livelock/deadlock risk.

`Pulse` is safe only when there are separate condition variables per predicate (Java's `ReentrantLock` + two `Condition`s, or `SemaphoreSlim` — both banned by the assignment). Under these constraints, `PulseAll` is the idiomatic answer.

### Shutdown

Producers finish at 101 items; consumers loop 60×3 = 180 times. Without a shutdown protocol the 79 extra `DeQueue` calls park forever.

Fix: consumers are **background threads**. After producers `Join`, Main drains the remaining queue (≤10 items), then `Console.ReadKey()` exits the process, killing the parked consumers.

```csharp
t2.IsBackground = t21.IsBackground = t22.IsBackground = true;
// ... Start all threads ...
t1.Join(); t11.Join();
lock (lockObj) { while (Count > 0) Monitor.Wait(lockObj); }
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
```

---

## Expected Output

```
...........[Thread-100]:Queue full, waiting...........
...........[Thread-200]:Queue full, waiting...........
j=1, thread:3
j=100, thread:1
j=2, thread:2
...
j=50, thread:2
j=149, thread:1
j=150, thread:3
Press any key to exit...
```

101 lines of `j=<value>, thread:<n>`. `Thread-100` = th01, `Thread-200` = th011, thread 1/2/3 = t2/t21/t22.

---

## Build & Run

Requires [.NET SDK](https://dotnet.microsoft.com/download) (tested on .NET 10).

```bash
dotnet run
```

Or compile manually:

```bash
csc Program.cs -out:Case_02.exe
mono Case_02.exe          # macOS/Linux
.\Case_02.exe             # Windows
```

---

## Files

| File | Description |
|------|-------------|
| `Program.cs` | Fixed solution (thread-safe) |
| `Program.Baseline.cs` | Original unsynchronized baseline (reference only) |
| `CaseStudy02.pdf` | Assignment spec (Thai + English) |

---

## Group

| Student ID | Name |
|------------|------|
| 67011081 | Aphichaphon Phatthanakun |
| 67011178 | Napaul Intharasing |
| 67011214 | Nuttawee Wachiratienchai |
| 67011717 | Phyo Arkar Win |
| 67011736 | Yu Yu Khaing |
