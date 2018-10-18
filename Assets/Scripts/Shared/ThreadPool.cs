using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GrandTheftAuto.Diagnostics;
using UnityEngine;

namespace GrandTheftAuto.Shared {
    public delegate void ThreadJob();

    public delegate void IndexedThreadJob(int index);

    public delegate void ParameterizedThreadJob<T>(T obj);

    public class ThreadPool {

        private const int DEFAULT_QUEUE_SIZE = 4096;

        private Thread[] threads;
        private Queue<ThreadJob> jobs;

        public bool Started { get; private set; }
        public int ThreadCount { get { return threads.Length; } }

        public ThreadPool() {
            jobs = new Queue<ThreadJob>(DEFAULT_QUEUE_SIZE);
            threads = new Thread[Settings.Instance.numberOfThreads];

            for(var i = 0; i < threads.Length; i++)
                threads[i] = new Thread(ThreadBody);
        }

        public void PushJob(ThreadJob job) {
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

        public void WaitUntilAllFinished() {
            for(var i = 0; i < threads.Length; i++)
                threads[i].Join();
        }

        private void ThreadBody() {
            ThreadJob job = null;

            while(true) {
                lock(jobs) {
                    if(jobs.Count == 0)
                        break;

                    job = jobs.Dequeue();
                }

                try {
                    job.Invoke();
                    Debug.Log("Processing job");
                }
                catch(Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        public static ThreadPool ForLoop(int length, IndexedThreadJob job) {

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

        public static ThreadPool ForeachLoop<T>(T[] array, ParameterizedThreadJob<T> job) {

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