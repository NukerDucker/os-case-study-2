// Original unsynchronized baseline, kept for reference/contrast only.
// Excluded from build — see CaseStudy02.csproj <Compile Remove> if/when a
// .csproj is added, or just don't include this file when running.
using System;
using System.Threading;

namespace OS_Problem_02_Baseline
{
    class Thread_safe_buffer
    {
        static int[] TSBuffer = new int[10];
        static int Front = 0;
        static int Back = 0;
        static int Count = 0;

        static void EnQueue(int eq)
        {
            TSBuffer[Back] = eq;
            Back++;
            Back %= 10;
            Count += 1;
        }

        static int DeQueue()
        {
            int x = 0;
            x = TSBuffer[Front];
            Front++;
            Front %= 10;
            Count -= 1;
            return x;
        }

        static void th01(object t)
        {
            int i;

            for (i = 1; i < 51; i++)
            {
                EnQueue(i);
                Thread.Sleep(5); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }

        static void th011(object t)
        {
            int i;

            for (i = 100; i < 151; i++)
            {
                EnQueue(i);
                Thread.Sleep(7); //ห้ามแก้ไขหรือเปลี่ยนแปลงบรรทัดนี้/Editing or Modification of this line is forbidden
            }
        }


        static void th02(object t)
        {
            int i;
            int j;

            for (i=0; i< 60; i++)
            {
                j = DeQueue();
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

            t1.Start(100);
            t11.Start(200);
            t2.Start(1);
            t21.Start(2);
            t22.Start(3);
        }
    }
}
