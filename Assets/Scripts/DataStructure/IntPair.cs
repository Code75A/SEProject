using System;

//KeyValuePair<int, int>无法序列化，
//在静态储存/SLManager进行动态->静态储存时需要以IntPair形式转储
[Serializable]
public class IntPair
{
    public int Key;
    public int Value;
}
