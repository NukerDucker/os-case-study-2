using System;
using System.Collections.Generic;
using System.Threading;

namespace OS_Problem_02
{
    class Thread_safe_buffer
    {
        static int[] TSBuffer = new int[10];
        static int Front = 0;
        static int Back = 0;
        static int Count = 0;

        static readonly object lockObj = new object();

        // set to true after both producers finish so consumers know to exit
        static bool producersFinished = false;

        // track which consumer thread exits first (for the presentation)
        static readonly List<int> terminationOrder = new List<int>();

        static void EnQueue(int eq, object t)
        {
            lock (lockObj)
            {
                // while (not if) — Mesa semantics: another thread may grab the slot
                // before this one re-acquires the lock after being woken
                while (Count == TSBuffer.Length)
                {
                    Console.WriteLine("...........[Thread-{0}]:Queue full, waiting...........", t);
                    Monitor.Wait(lockObj);
                }

                TSBuffer[Back] = eq;
                Back = (Back + 1) % TSBuffer.Length;
                Count++;

                // PulseAll because producers and consumers share one wait queue —
                // Pulse would risk waking the wrong role
                Monitor.PulseAll(lockObj);
            }
        }

        // dequeue + print in the same critical section so printed order == FIFO
        static bool DeQueueAndPrint(object t)
        {
            lock (lockObj)
            {
                while (Count == 0 && !producersFinished)
                    Monitor.Wait(lockObj);

                if (Count == 0)
                    return false; // producers done, nothing left

                int x = TSBuffer[Front];
                Front = (Front + 1) % TSBuffer.Length;
                Count--;

                Console.WriteLine("j={0}, thread:{1}", x, t);

                Monitor.PulseAll(lockObj);
                return true;
            }
        }

        static void th01(object t)
        {
            for (int i = 1; i < 51; i++)
            {
                EnQueue(i, t);
                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th011(object t)
        {
            for (int i = 100; i < 151; i++)
            {
                EnQueue(i, t);
                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th02(object t)
        {
            for (int i = 0; i < 60; i++)
            {
                if (!DeQueueAndPrint(t)) break;
                Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }

            lock (lockObj) { terminationOrder.Add((int)t); }
        }

        static void Main(string[] args)
        {
            Thread t1  = new Thread(th01);
            Thread t11 = new Thread(th011);
            Thread t2  = new Thread(th02);
            Thread t21 = new Thread(th02);
            Thread t22 = new Thread(th02);

            t1.Start(100); t11.Start(200);
            t2.Start(1);   t21.Start(2);   t22.Start(3);

            t1.Join(); t11.Join();

            // signal consumers: nothing more will be enqueued
            lock (lockObj) { producersFinished = true; Monitor.PulseAll(lockObj); }

            t2.Join(); t21.Join(); t22.Join();

            Console.WriteLine("Press any key to exit...");
            if (!Console.IsInputRedirected) Console.ReadKey(true);

            foreach (int id in terminationOrder)
                Console.WriteLine("Thread-{0} exited", id);
        }
    }
}
