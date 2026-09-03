using System;
using System.Threading;

namespace OS_Problem_02
{
    class Thread_safe_buffer
    {
        static int[] TSBuffer = new int[10];
        static int Front = 0;
        static int Back = 0;
        static int Count = 0;

        // single lock + Mesa-semantics condition variable (one wait queue shared by
        // producers and consumers -> PulseAll required; while required per Mesa recheck rule)
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

                Monitor.PulseAll(lockObj); // must PulseAll: one wait queue, two predicates
            }
        }

        // DeQueue + print inside same critical section so printed order == FIFO dequeue order
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

        static void th01(object t)
        {
            int i;
            for (i = 1; i < 51; i++)
            {
                EnQueue(i, t);
                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th011(object t)
        {
            int i;
            for (i = 100; i < 151; i++)
            {
                EnQueue(i, t);
                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th02(object t)
        {
            int i;
            for (i = 0; i < 60; i++)
            {
                DeQueueAndPrint(t);
                Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void Main(string[] args)
        {
            Thread t1  = new Thread(th01);
            Thread t11 = new Thread(th011);
            Thread t2  = new Thread(th02);
            Thread t21 = new Thread(th02);
            Thread t22 = new Thread(th02);

            // consumers are background: killed automatically when producers finish + queue drains
            t2.IsBackground = t21.IsBackground = t22.IsBackground = true;

            t1.Start(100); t11.Start(200);
            t2.Start(1);   t21.Start(2);   t22.Start(3);

            t1.Join(); t11.Join(); // wait for all items produced

            // drain: wait until last queued item is consumed and printed
            lock (lockObj) { while (Count > 0) Monitor.Wait(lockObj); }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
