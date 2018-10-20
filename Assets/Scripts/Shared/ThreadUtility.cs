using System;
using System.Threading.Tasks;

namespace GrandTheftAuto.Shared {
    public class ThreadUtility {

        public static void ForLoop(int length, Action<int> body) {
            if(body == null)
                throw new ArgumentNullException("body");

            var result = Parallel.For(0, length, (i) => body(i));
        }

        public static void For(int lengthX, int lengthY, Action<int, int> body) {
            if(body == null)
                throw new ArgumentNullException("body");

            var result = Parallel.For(0, lengthX * lengthY, (i) => body(i % lengthX, i / lengthY));
        }

        public static void Foreach<T>(T[] array, Action<T> body) {
            if(body == null)
                throw new ArgumentNullException("body");

            var result = Parallel.ForEach(array, (i) => body(i));
        }

    }
}