[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

public static class LockClass
{
    public static object LockObject = new object();
}