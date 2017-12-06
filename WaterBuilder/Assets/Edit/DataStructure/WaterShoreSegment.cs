using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//组成水岸线的线段
public class WaterShoreSegment
{
	private const float SegmentJoinMinDistant = 0.0001f;
	
	public Vector3 posOne;
	public Vector3 posTwo;

	/// <summary>
	/// 传入的水的边界线是否能够连接在当前线段的前面，如果能够相连，是否需要自动变换前后坐标
	/// </summary>
	/// <returns><c>true</c> if this instance can join water line the specified waterLine; otherwise, <c>false</c>.</returns>
	/// <param name="waterLine">Water line.</param>
	public bool CanJoinAtFront(WaterShoreSegment segment, out bool needExchangePos)
	{
		needExchangePos = false;
		if (IsPointNear (posOne, segment.posOne)) {
			needExchangePos = true;
			return true;
		}
		if(IsPointNear(posOne, segment.posTwo))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// 传入的水的边界线是否能够连接在当前线段的后面，如果能够相连，是否需要自动变换前后坐标
	/// </summary>
	/// <returns><c>true</c>, if in front water line was joined, <c>false</c> otherwise.</returns>
	/// <param name="waterLine">Water line.</param>
	public bool CanJoinAtEnd(WaterShoreSegment segment, out bool needExchangePos)
	{
		needExchangePos = false;
		if (IsPointNear (posTwo, segment.posTwo)) {
			needExchangePos = true;
			return true;
		}
		if(IsPointNear(posTwo, segment.posOne))
		{
			return true;
		}
		return false;
	}


	/// <summary>
	/// 两个点是否足够近，能够认为是同一个点
	/// </summary>
	/// <returns><c>true</c> if this instance is point near the specified pos1 pos2; otherwise, <c>false</c>.</returns>
	/// <param name="pos1">Pos1.</param>
	/// <param name="pos2">Pos2.</param>
	public bool IsPointNear(Vector3 pos1, Vector3 pos2)
	{
		return Vector3.Distance (pos1, pos2) < SegmentJoinMinDistant;	
	}

	/// <summary>
	/// 交换前后坐标
	/// </summary>
	public void ExchangePos()
	{
		Vector3 temp = posOne;
		posOne = posTwo;
		posTwo = temp;
	}

	/// <summary>
	/// 获取线段的长度
	/// </summary>
	/// <returns>The length.</returns>
	public float GetLength()
	{
		return Vector3.Distance (posOne, posTwo);
	}

	/// <summary>
	/// 获取Log信息
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="WaterShoreSegment"/>.</returns>
	public override string ToString ()
	{
		return "(" + posOne.x.ToString("F4") + "," + posOne.y.ToString("F4") + "," + posOne.z.ToString("F4") + ")"
			+ " -->  "
			+ "(" + posTwo.x.ToString("F4") + "," + posTwo.y.ToString("F4") + "," + posTwo.z.ToString("F4") + ")";
	}
}