using System.Collections.Generic;

public static class StackUtility {

    public static T MoveStack<T>(Stack<T> from, Stack<T> to) {
        var obj = from.SafePop();
        to.Push(obj);
        return obj;
    }

    public static T SafePop<T>(this Stack<T> stack) {
        if(stack.Count > 0)
            return stack.Pop();
        else
            return default(T);
    }

    public static T SafePeek<T>(this Stack<T> stack) {
        if(stack.Count > 0)
            return stack.Peek();
        else
            return default(T);
    }

}