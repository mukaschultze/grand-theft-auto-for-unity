using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Shared {

    public class ThreadPool {

        private const int DEFAULT_QUEUE_SIZE = 4096;

        private Thread[] threads;
        private Queue<Action> jobs;

        public bool Started { get; private set; }
        public int ThreadCount { get { return threads.Length; } }

        public ThreadPool() {
            jobs = new Queue<Action>(DEFAULT_QUEUE_SIZE);
            threads = new Thread[Settings.Instance.numberOfThreads];

            for(var i = 0; i < threads.Length; i++)
                threads[i] = new Thread(ThreadBody);
        }

        public void PushJob(Action job) {
            if(Started)
                throw new InvalidOperationException("Cannot add jobs to an already started thread pool");

            jobs.Enqueue(job);
        }

        public void Start() {
            if(Started)
                throw new InvalidOperationException("Cannot start an already started thread pool");

            for(var i = 0; i < threads.Length; i++)
                threads[i].Start();

            Started = true;
        }

        public void WaitUntilFinished() {
            for(var i = 0; i < threads.Length; i++) {
                threads[i].Join();
                threads[i].Abort();
                Log.Message(threads[i].IsAlive);
            }
        }

        private void ThreadBody() {
            Action job = null;

            while(true) {
                lock(jobs) {
                    if(jobs.Count == 0)
                        break;

                    job = jobs.Dequeue();
                }

                try {
                    job.Invoke();
                    // Debug.Log("Processing job");
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        public static ThreadPool ForLoop(int length, Action<int> job) {

            var pool = new ThreadPool();
            var chunkSize = Mathf.Max(length / pool.ThreadCount, 1);

            for(int i = 0; i < pool.ThreadCount; i++) {
                var startIndex = i * chunkSize;
                var endIndex = Mathf.Min((i + 1) * chunkSize, length);

                if(startIndex < endIndex)
                    pool.PushJob(() => {
                        for(var j = startIndex; j < endIndex; j++)
                            job.Invoke(j);
                    });
            }

            pool.Start();
            return pool;

        }

        public static ThreadPool ForLoop(int lengthX, int lengthY, Action<int, int> job) {

            var pool = new ThreadPool();
            var totalLength = lengthX * lengthY;
            var chunkSize = Mathf.Max(totalLength / pool.ThreadCount, 1);

            for(int i = 0; i < pool.ThreadCount; i++) {
                var startIndex = i * chunkSize;
                var endIndex = Mathf.Min((i + 1) * chunkSize, totalLength);

                if(startIndex < endIndex)
                    pool.PushJob(() => {
                        for(var j = startIndex; j < endIndex; j++) {
                            var x = j % lengthX;
                            var y = j / lengthX;
                            job.Invoke(x, y);
                        }
                    });
            }

            pool.Start();
            return pool;

        }

        public static ThreadPool ForeachLoop<T>(T[] array, Action<T> job) {

            var pool = new ThreadPool();
            var chunkSize = Mathf.Max(array.Length / pool.ThreadCount, 1);

            for(int i = 0; i < pool.ThreadCount; i++) {
                var startIndex = i * chunkSize;
                var endIndex = Mathf.Min((i + 1) * chunkSize, array.Length);

                if(startIndex < endIndex)
                    pool.PushJob(() => {
                        for(var j = startIndex; j < endIndex; j++)
                            job.Invoke(array[j]);
                    });
            }

            pool.Start();
            return pool;

        }

    }
}