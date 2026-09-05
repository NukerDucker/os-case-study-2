using System;
using System.Threading;

namespace OS_Problem_02
{
    class Thread_safe_buffer
    {
        static int[] TSBuffer = new int[10];
        static object _lock = new object();
        static int Front = 0;
        static int Back = 0;
        static int Count = 0;
        static bool producersFinished = false;


        static void EnQueue(int eq, object t)
        {
            while (Count == 10)
            {
                Console.WriteLine("[Thread {0}] Queue full, waiting...", t);
                Monitor.Wait(_lock);
            }
            TSBuffer[Back] = eq;
            Back++;
            Back %= 10;
            Count += 1;
            Monitor.Pulse(_lock);
        }

        static int DeQueue()
        {
        int x = 0;

            while (Count == 0 && !producersFinished)
            {
                Monitor.Wait(_lock);
            }
            if (Count == 0 && producersFinished)
            {
                return -1;
            }
            x = TSBuffer[Front];
            Front++;
            Front %= 10;
            Count -= 1;
            Monitor.Pulse(_lock);
        
            return x;
        }

        static void th01(object t)
        {
            int i;

            for (i = 1; i < 51; i++)
            {
                lock(_lock)
                {
                    EnQueue(i, t);
                }
                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th011(object t)
        {
            int i;

            for (i = 100; i < 151; i++)
            {
                lock(_lock)
                {
                    EnQueue(i, t);
                }
                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }


        static void th02(object t)
        {
            int i;
            int j;

            for (i=0; i< 60; i++)
            {
                lock(_lock)
                {
                    j = DeQueue();
                }
                if (j == -1)
                {
                    break;
                }
                Console.WriteLine("j={0}, thread:{1}", j, t);
                Thread.Sleep(16); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }
        static void Main(string[] args)
        {
            Thread t1 = new Thread(th01);
            Thread t11 = new Thread(th011);
            Thread t2 = new Thread(th02);
            Thread t21 = new Thread(th02);
            Thread t22 = new Thread(th02);

            while(true)
            {
                Console.WriteLine("Press any key to start the program...");
                Console.ReadKey();
                Console.WriteLine("\n");
                break;
            }

            t1.Start(100);
            t11.Start(200);
            t2.Start(1);
            t21.Start(2);
            t22.Start(3);

            t1.Join();
            t11.Join();
            lock(_lock)
            {
                producersFinished = true;
                Monitor.PulseAll(_lock);
            }
            t2.Join();
            t21.Join();
            t22.Join();
            Console.WriteLine("All threads have completed execution.");
        }
    }
}
