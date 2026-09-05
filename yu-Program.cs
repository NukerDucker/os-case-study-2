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
        static object bufferLock = new object();
        // Store the order in which threads terminate
        static List<int> terminationOrder = new List<int>();


        static void EnQueue(int eq, object t)
        {
            lock (bufferLock)
            {
                while (Count == TSBuffer.Length)
                {
                    Console.WriteLine(".........[Thread-{0}]: Queue full, waiting.........", t);
                    Monitor.Wait(bufferLock);
                }

                TSBuffer[Back] = eq;
                Back++;
                Back %= TSBuffer.Length;

                Count++;
                Monitor.PulseAll(bufferLock);
            }
        }


        static int DeQueue()
        {
            lock (bufferLock)
            {
                while (Count == 0)
                {
                    Monitor.Wait(bufferLock);
                }

                int x = TSBuffer[Front];

                Front++;
                Front %= TSBuffer.Length;
                Count--;

                Monitor.PulseAll(bufferLock);
                return x;
            }
        }


        // Record when a thread finishes
        static void ThreadExit(object t)
        {
            lock (bufferLock)
            {
                terminationOrder.Add((int)t);
            }
        }


        // Producer 1 produces 1 - 50
        static void th01(object t)
        {
            for (int i = 1; i < 51; i++)
            {
                EnQueue(i, t);

                Console.WriteLine(
                    "Enqueue: Buffer: {0}",
                    string.Join(", ", TSBuffer));

                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }


        // Producer 2 produces 100 - 150
        static void th011(object t)
        {
            for (int i = 100; i < 151; i++)
            {
                EnQueue(i, t);

                Console.WriteLine(
                    "Enqueue: Buffer: {0}",
                    string.Join(", ", TSBuffer));

                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }


        // Consumer threads
        // Thread 1 = 34 items
        // Thread 2 = 34 items
        // Thread 3 = 33 items
        // Total = 101 items
        static void th02(object t)
        {
            int numberOfItems = 34;

            // have 101 items in total.
            // thread 3 consumes 33 items.
            if ((int)t == 3)
            {
                numberOfItems = 33;
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                int j = DeQueue();

                Console.WriteLine(
                    "j={0}, thread:{1}", j, t);

                Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }

            ThreadExit(t);
        }


        static void Main(string[] args)
        {
            Thread t1 = new Thread(th01);
            Thread t11 = new Thread(th011);

            Thread t2 = new Thread(th02);
            Thread t21 = new Thread(th02);
            Thread t22 = new Thread(th02);

            t1.Start(100);
            t11.Start(200);

            t2.Start(1);
            t21.Start(2);
            t22.Start(3);

            t1.Join();
            t11.Join();
            t2.Join();
            t21.Join();
            t22.Join();

            Console.WriteLine("Press any key to exit...");

            Console.ReadKey(true);

            for (int i = 0; i < terminationOrder.Count; i++)
            {
                Console.WriteLine(
                    "Thread-{0} exit", terminationOrder[i]);
            }
        }
    }
}
