using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleShape
{
	public Vector3[] PosArray = new Vector3[3];

	public TriangleShape(Vector3 pos1, Vector3 pos2, Vector3 pos3)
	{
		PosArray [0] = pos1;
		PosArray [1] = pos2;
		PosArray [2] = pos3;
		SortPoints();
	}

	public bool IsLineIntersect(Vector3 linePos1, Vector3 linePos2)
	{
		if (linePos1.Equals (PosArray [0]) && linePos2.Equals (PosArray [1]))
			return false;
		if (linePos1.Equals (PosArray [1]) && linePos2.Equals (PosArray [2]))
			return false;
		if (linePos1.Equals (PosArray [2]) && linePos2.Equals (PosArray [0]))
			return false;
		if (Utils.IsLineIntersection (linePos1, linePos2, PosArray [0], PosArray [1]))
			return true;
		if (Utils.IsLineIntersection (linePos1, linePos2, PosArray [1], PosArray [2]))
			return true;
		if (Utils.IsLineIntersection (linePos1, linePos2, PosArray [2], PosArray [0]))
			return true;
		return false;
	}

	protected void SortPoints()
	{
		Vector3 cross = Vector3.Cross (PosArray [1] - PosArray [0], PosArray [2] - PosArray [1]);
		if (Vector3.Dot (cross, Vector3.up) < 0) {
			Vector3 temp = PosArray [2];
			PosArray [2] = PosArray [1];
			PosArray [1] = temp;
		}
	}
//
//	public List<TriangleShape> CullWithTriangle(List<TriangleShape> cullTriangleList)
//	{
//		List<TriangleShape> ret = new List<TriangleShape> ();
//		for (int j = 0; j < cullTriangleList.Count; ++j) {
//			TriangleShape cullTriangle = cullTriangleList [j];
//			int[] cullSamePointIndexArray = new int[3];
//			int[] mySamePointIndexArray = new int[3];
//			int samePoint = GetSamePointCount (cullTriangle, cullSamePointIndexArray, mySamePointIndexArray);
//			if (samePoint == 3) {
//				//三角形完全重合,说明自己是不需要的，不处理
//				continue;
//			}
//			if (samePoint == 2) {
//				int cullNotSamePointIndex = GetNotContainIndex (cullSamePointIndexArray, 2);
//				int myNotSamePointIndex = GetNotContainIndex (mySamePointIndexArray, 2);
//				Vector3 cullNotSamePoint = cullTriangle.PosArray[cullNotSamePointIndex];
//				Vector3 myNotSamePoint = PosArray[myNotSamePointIndex];
//				if (IsPointInTriangle (cullNotSamePoint)) {
//					//不重合的点在三角形内部,需要用不重合的点和当前三角形划分出连个新的三角形
//					TriangleShape newTriangle1 = new TriangleShape (cullTriangle.PosArray [cullSamePointIndexArray [0]], myNotSamePoint, cullNotSamePoint);
//					TriangleShape newTriangle2 = new TriangleShape (cullTriangle.PosArray [cullSamePointIndexArray [1]], cullNotSamePoint, myNotSamePoint);
//					ret.Add (newTriangle1);
//					ret.Add (newTriangle2);
//				} else if (cullTriangle.IsPointInTriangle (myNotSamePoint)) {
//					//裁剪的三角形包含了自己,说明自己是不需要的，不处理
//				} else {
//					//两点重合式三角形相交,找到相交点
//					for (int i = 0; i < 2; ++i) {
//						for (int k = 0; k < 2; ++k) {
//							if (Utils.IsLineIntersection (PosArray [mySamePointIndexArray [i]], myNotSamePoint
//								, cullTriangle.PosArray [cullSamePointIndexArray [k]], cullNotSamePoint)) {
//								//如果这两条线段相交,计算出交点
//
//								Vector3 intersectionPoint = Utils.GetLineIntersectionPoint (PosArray [mySamePointIndexArray [i]], myNotSamePoint
//									, cullTriangle.PosArray [cullSamePointIndexArray [k]], cullNotSamePoint);
//								//找到自身不相交的那条线段的点
//								int index = i == 0 ? 1 : 0;
//								TriangleShape newTriangle = new TriangleShape (PosArray [index], myNotSamePoint, intersectionPoint);
//								ret.Add (newTriangle);
//							}
//						}
//					}
//				}
//				continue;
//			}
//
//			if (samePoint == 1) {
//
//				continue;
//			}
//		}
//		return null;
//	}

	int GetNotContainIndex(int[] index, int dataCount)
	{
		for (int i = 0; i < PosArray.Length; ++i) {
			bool exit = false;
			for (int j = 0; j < dataCount; ++j) {
				if (i == index [j]) {
					exit = true;
					break;
				}
			}
			if (!exit)
				return i;
		}
		return -1;
	}

	bool IsNearWithVertex(Vector3 point, out int index)
	{
		for (int i = 0; i < PosArray.Length; ++i) {
			if (!PosArray [i].Equals (point))
				continue;
			index = i;
			return true;
		}
		index = -1;
		return false;
	}

	bool IsPointInTriangle(Vector3 point)
	{
		for (int i = 0; i < PosArray.Length; ++i) {
			Vector3 curPos = PosArray [i];
			Vector3 nextPos = PosArray[Utils.GetRingNextIndex(i, PosArray.Length, false)];
			Vector3 crossDir = Vector3.Cross (nextPos - curPos, point - curPos);
			if (crossDir.y < 0)
				return false;
		}
		return true;
	}

	int GetSamePointCount(TriangleShape triangle, int[] otherSameIndexArray, int[] mySameIndexArray){
		int count = 0;
		for (int i = 0; i < triangle.PosArray.Length; ++i) {
			int mySameIndex = 0;
			if(IsNearWithVertex(triangle.PosArray[i], out mySameIndex))
			{
				mySameIndexArray [count] = mySameIndex;
				otherSameIndexArray [count++] = i;
			}
		}
		return count;
	}

	public override string ToString ()
	{
		return string.Format ("({0} , {1}, {2}) ({3} , {4}, {5}) ({6} , {7}, {8}) "
			, PosArray [0].x, PosArray [0].y, PosArray [0].z
			, PosArray [1].x, PosArray [1].y, PosArray [1].z
			, PosArray [2].x, PosArray [2].y, PosArray [2].z);
	}
}
