namespace CommandLine.Infrastructure;

internal sealed class Pair<TLeft, TRight> where TLeft : class where TRight : class
{
	private readonly TLeft _left;

	private readonly TRight _right;

	public TLeft Left => _left;

	public TRight Right => _right;

	public Pair(TLeft left, TRight right)
	{
		_left = left;
		_right = right;
	}

	public override int GetHashCode()
	{
		int num;
		if (_left != null)
		{
			TLeft left = _left;
			num = left.GetHashCode();
		}
		else
		{
			num = 0;
		}
		int num2 = num;
		int num3;
		if (_right != null)
		{
			TRight right = _right;
			num3 = right.GetHashCode();
		}
		else
		{
			num3 = 0;
		}
		int num4 = num3;
		return num2 ^ num4;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Pair<TLeft, TRight> pair))
		{
			return false;
		}
		if (object.Equals(_left, pair._left))
		{
			return object.Equals(_right, pair._right);
		}
		return false;
	}
}
