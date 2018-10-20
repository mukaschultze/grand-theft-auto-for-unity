using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class StackUtility {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MoveStack<T>(Stack<T> from, Stack<T> to) {
        var obj = from.SafePop();
        to.Push(obj);
        return obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafePop<T>(this Stack<T> stack) {
        if(stack.Count > 0)
            return stack.Pop();
        else
            return default(T);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafePeek<T>(this Stack<T> stack) {
        if(stack.Count > 0)
            return stack.Peek();
        else
            return default(T);
    }

}