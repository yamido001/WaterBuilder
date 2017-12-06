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
	}

	/// <summary>
	/// 确保传入的线段相交
	/// </summary>
	/// <returns>The line intersection point.</returns>
	/// <param name="line1Pos1">Line1 pos1.</param>
	/// <param name="line1Pos2">Line1 pos2.</param>
	/// <param name="line2Pos1">Line2 pos1.</param>
	/// <param name="line2Pos2">Line2 pos2.</param>
	public static Vector3 GetLineIntersectionPoint(Vector3 line1Pos1, Vector3 line1Pos2, Vector3 line2Pos1, Vector3 line2Pos2)
	{
		//假设线段1和线段2的交点是P http://blog.csdn.net/dgq8211/article/details/7952825
		Vector3 cross1 = Vector3.Cross(line2Pos1 - line1Pos1, line1Pos2 - line1Pos1);
		Vector3 cross2 = Vector3.Cross(line2Pos2 - line1Pos1, line1Pos2 - line1Pos1);
		float percent = cross1.sqrMagnitude / cross2.sqrMagnitude;
		return Vector3.Lerp (line2Pos1, line2Pos2, percent / 1 + percent);
	}

	public static List<TriangleShape> GenerateTriangles(List<Vector3> posList)
	{
		List<TriangleShape> retList = new List<TriangleShape> ();
		List<Vector3> pointList = new List<Vector3> ();
		pointList.AddRange (posList);

		HashSet<int> aoPointList = new HashSet<int>();
		int[] findPointIndexArray = new int[3];

		while (true) {
			aoPointList.Clear ();
			//这个Cross判断的方法需要pointList是逆时针存在的，但是现在哪一步保证了pointList是逆时针的，不清楚
			for (int i = 0; i < pointList.Count; ++i) {
				Vector3 curPoint = pointList [i];
				Vector3 nextPoint = pointList[GetRingNextIndex(i, pointList.Count, false)];
				Vector3 prePoint = pointList[GetRingNextIndex(i, pointList.Count, true)];

				Vector3 crossDir = Vector3.Cross (nextPoint - curPoint, curPoint - prePoint);
				if (crossDir.y < 0) {
					aoPointList.Add (i);
				}
			}

			if (aoPointList.Count == 0)
				break;

			findPointIndexArray [0] = -1;
			//找到当前是凹点，并且下一个点不为凹点
			foreach(int curIndex in aoPointList)
			{
				findPointIndexArray [1] = GetRingNextIndex(curIndex, pointList.Count, false);
				findPointIndexArray [2] = GetRingNextIndex(findPointIndexArray [1] , pointList.Count, false);
				if(!aoPointList.Contains(findPointIndexArray [1])){
					findPointIndexArray [0] = curIndex;
					break;
				}
			}
			if(findPointIndexArray [0] < 0)
			{
				Debug.LogError("没有找到合适的切割点");
				return null;
			}

			TriangleShape triangleShape = new TriangleShape (pointList [findPointIndexArray [0]], pointList [findPointIndexArray [1]], pointList [findPointIndexArray [2]]);
			retList.Add (triangleShape);
			pointList.RemoveAt (findPointIndexArray[1]);
		}

		//分割完毕后，再把凸多边形分割成三角形
		Vector3 centerPos = GetCenterPos(pointList);
		for (int i = 0; i < pointList.Count; ++i) {
			Vector3 curPoint = pointList [i];
			Vector3 nextPoint = pointList[GetRingNextIndex(i, pointList.Count, false)];
			TriangleShape triangleShape = new TriangleShape (curPoint, centerPos, nextPoint);
			retList.Add (triangleShape);
		}
		return retList;
	}

	public static Vector3 GetCenterPos(List<Vector3> pointList)
	{
		Vector3 centerPos = Vector3.zero;
		for (int i = 0; i < pointList.Count; ++i) {
			centerPos += pointList [i];
		}
		return centerPos / pointList.Count;
	}

	public static Vector4 GetRangeSize(List<TriangleShape> triangleList){
		Vector4 ret;
		ret.x = float.MaxValue;
		ret.y = float.MinValue;
		ret.z = float.MaxValue;
		ret.w = float.MinValue;

		for (int i = 0; i < triangleList.Count; ++i) {
			TriangleShape triangle = triangleList [i];
			ret.x = GetMinValue (ret.x, triangle.PosArray [0].x, triangle.PosArray [1].x, triangle.PosArray [2].x);
			ret.y = GetMaxValue (ret.x, triangle.PosArray [0].x, triangle.PosArray [1].x, triangle.PosArray [2].x);
			ret.z = GetMinValue (ret.z, triangle.PosArray [0].z, triangle.PosArray [1].z, triangle.PosArray [2].z);
			ret.w = GetMaxValue (ret.w, triangle.PosArray [0].z, triangle.PosArray [1].z, triangle.PosArray [2].z);
		}
		return ret;
	}

	public static void CopyList(List<Vector3> srcList, List<Vector3> dstList, int beginIndex, bool isRevert)
	{
		int copyCount = 0;
		while (copyCount < srcList.Count) {
			copyCount++;
			dstList.Add (srcList [beginIndex]);
			beginIndex = GetRingNextIndex (beginIndex, srcList.Count, isRevert);
		}
	}
}
