using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils{

	public static float GetMinValue(float value1, float value2, float value3)
	{
		float ret = Mathf.Min (value1, value2);
		ret = Mathf.Min (ret, value3);
		return ret;
	}

	public static float GetMinValue(float value1, float value2, float value3, float value4)
	{
		float ret = GetMinValue(value1, value2, value3);
		ret = Mathf.Min (ret, value4);
		return ret;
	}

	public static float GetMaxValue(float value1, float value2, float value3)
	{
		float ret = Mathf.Max (value1, value2);
		ret = Mathf.Max (ret, value3);
		return ret;
	}

	public static float GetMaxValue(float value1, float value2, float value3, float value4)
	{
		float ret = GetMaxValue(value1, value2, value3);
		ret = Mathf.Max (ret, value4);
		return ret;
	}

	public static int GetRingNextIndex(int curIndex, int maxCount, bool isReverse)
	{
		if (isReverse) {
			//倒叙
			curIndex = curIndex == 0 ? maxCount - 1 : curIndex - 1;
		} else {
			curIndex = curIndex == maxCount - 1 ? 0 : curIndex + 1;
		}
		return curIndex;
	}

	/// <summary>
	/// 两个线段是否相交，点重合时会返回false
	/// </summary>
	/// <returns><c>true</c> if is line intersection the specified line1Pos1 line1Pos2 line2Pos1 line2Pos2; otherwise, <c>false</c>.</returns>
	/// <param name="line1Pos1">Line1 pos1.</param>
	/// <param name="line1Pos2">Line1 pos2.</param>
	/// <param name="line2Pos1">Line2 pos1.</param>
	/// <param name="line2Pos2">Line2 pos2.</param>

	public static bool IsLineIntersection(Vector3 line1Pos1, Vector3 line1Pos2, Vector3 line2Pos1, Vector3 line2Pos2)
	{
		//相同的线段，直接返回
		if (line1Pos1.Equals (line2Pos1) && line1Pos2.Equals(line2Pos2))
			return false;
		if (line1Pos1.Equals (line2Pos2) && line1Pos2.Equals(line2Pos1))
			return false;

		//位置在不同值域内，不相交
		if (line1Pos1.x <= line2Pos1.x && line1Pos1.x <= line2Pos2.x && line1Pos2.x <= line2Pos1.x && line1Pos2.x <= line2Pos2.x)
			return false;
		if (line1Pos1.x >= line2Pos1.x && line1Pos1.x >= line2Pos2.x && line1Pos2.x >= line2Pos1.x && line1Pos2.x >= line2Pos2.x)
			return false;
		if (line1Pos1.z <= line2Pos1.z && line1Pos1.z <= line2Pos2.z && line1Pos2.z <= line2Pos1.z && line1Pos2.z <= line2Pos2.z)
			return false;
		if (line1Pos1.z >= line2Pos1.z && line1Pos1.z >= line2Pos2.z && line1Pos2.z >= line2Pos1.z && line1Pos2.z >= line2Pos2.z)
			return false;

		if (line1Pos1.Equals (line2Pos1)) {
			//一个点重合并且方向相同，认为相交
			return (line1Pos2 - line1Pos1).normalized.Equals (line2Pos2 - line2Pos1);
		}
		if (line1Pos1.Equals (line2Pos2)) {
			//一个点重合并且方向相同，认为相交
			return (line1Pos2 - line1Pos1).normalized.Equals (line2Pos1 - line2Pos2);
		}
		if (line1Pos2.Equals (line2Pos1)) {
			//一个点重合并且方向相同，认为相交
			return (line1Pos1 - line1Pos2).normalized.Equals (line2Pos2 - line2Pos1);
		}
		if (line1Pos2.Equals (line2Pos2)) {
			//一个点重合并且方向相同，认为相交
			return (line1Pos1 - line1Pos2).normalized.Equals (line2Pos1 - line2Pos2);
		}

		float dot1 = Vector3.Dot (Vector3.Cross(line1Pos1 - line2Pos1, line2Pos2 - line2Pos1), Vector3.Cross(line1Pos2 - line2Pos1, line2Pos2 - line2Pos1));
		float dot2 = Vector3.Dot (Vector3.Cross(line2Pos2 - line1Pos2, line1Pos1 - line1Pos2), Vector3.Cross(line2Pos1 - line1Pos2, line1Pos1 - line1Pos2));
		if (dot1 < 0 && dot2 < 0)
			return true;

		if (dot1 == 0 || dot2 == 0)
			return true;
		return false;
		//以line1Pos1为原点,判断line1Pos2是否在line2pos1和line2pos2同原点组成的夹角内
		line1Pos2 -= line1Pos1;
		line2Pos1 -= line1Pos1;
		line2Pos2 -= line1Pos1;

		Vector3 cross1 = Vector3.Cross (line2Pos1, line1Pos2);
		Vector3 cross2 = Vector3.Cross (line1Pos2, line2Pos2);
		if (cross1.y * cross2.y < 0) {
			//不在夹角内，不想交
			return false;
		}
		//判断是否是反方向在夹角内
		float dotValue = Vector3.Dot(line1Pos2, line2Pos1);
		return dotValue > 0f;
	}
}
