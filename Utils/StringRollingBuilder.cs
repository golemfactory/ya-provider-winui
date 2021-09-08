using System;
using System.Collections.Generic;

/// <summary>
/// A pseudo-StringBuilder with maximum number of lines.
/// </summary>
public class StringRollingBuilder
{
    private Queue<string> content = new Queue<string>();

    private int maxLines;

    public StringRollingBuilder(int maxLines)
    {
        this.maxLines = maxLines;
    }

    public StringRollingBuilder Append(string line)
    {
        if (this.content.Count == maxLines)
            this.content.Dequeue();

        this.content.Enqueue(line);

        return this;
    }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, this.content);
    }
}
