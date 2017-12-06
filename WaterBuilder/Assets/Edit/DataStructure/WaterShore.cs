using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 水岸线
/// </summary>
public class WaterShore
{
	/// <summary>
	/// 在List中的前后两条线段依次相连
	/// </summary>
	private List<WaterShoreSegment> mWaterShoreSegmentList = new List<WaterShoreSegment> ();
	private List<Vector3> mWaterShorePointList;
	private float mWaterHeight;
	private static int IdIndex = 0;

	public WaterShore(float waterHeight)
	{
		mWaterHeight = waterHeight;
		ID = ++IdIndex;
	}

	public int ID {
		get;
		private set;
	}

	public bool IsInvalidShore {
		get;
		private set;
	}

	public Vector4 EdgePos
	{
		get {
			float xMin = float.MaxValue;
			float yMin = float.MaxValue;
			float xMax = float.MinValue;
			float yMax = float.MinValue;
			for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
				WaterShoreSegment segement = mWaterShoreSegmentList [i];
				xMin = Utils.GetMinValue (xMin, segement.posOne.x, segement.posTwo.x);
				xMax = Utils.GetMaxValue (xMin, segement.posOne.x, segement.posTwo.x);
				yMin = Utils.GetMinValue (yMin, segement.posOne.y, segement.posTwo.y);
				yMax = Utils.GetMaxValue (yMax, segement.posOne.y, segement.posTwo.y);
			}
			return new Vector4 (xMin, xMax, yMin, yMax);
		}
	}

	public float EdgeSize
	{
		get{
			return Mathf.Sqrt ((EdgePos.y - EdgePos.x) * (EdgePos.y - EdgePos.x) + (EdgePos.w - EdgePos.z) * (EdgePos.w - EdgePos.z));
		}
	}

	/// <summary>
	/// 把传入的线放入现有的链表中
	/// </summary>
	/// <returns><c>true</c>, if water line was joined, <c>false</c> otherwise.</returns>
	/// <param name="line">Line.</param>
	public bool  JoinWaterShoreSegment(WaterShoreSegment line)
	{
		if (mWaterShoreSegmentList.Count == 0) {
			mWaterShoreSegmentList.Add (line);
			return true;
		}
		bool needExchange = false;
		if (mWaterShoreSegmentList [0].CanJoinAtFront (line, out needExchange)) {
			if (needExchange) {
				line.ExchangePos ();
			}
			mWaterShoreSegmentList.Insert (0, line);
			return true;
		}
		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (line, out needExchange)) {
			if (needExchange) {
				line.ExchangePos ();
			}
			mWaterShoreSegmentList.Add(line);
			return true;
		}
		return false;
	}

	/// <summary>
	/// 把传入的海岸线和当前的相连起来
	/// </summary>
	/// <returns><c>true</c>, if water shore was joined, <c>false</c> otherwise.</returns>
	/// <param name="waterShore">Water shore.</param>
	public bool JoinWaterShore(WaterShore waterShore)
	{
		if (mWaterShoreSegmentList.Count == 0) {
			Debug.LogError ("Water shore with no segment");
			return false;
		}
		if (waterShore.mWaterShoreSegmentList.Count == 0) {
			Debug.LogError ("Input Water shore with no segment");
			return false;
		}
		bool needExchange = false;
		if (mWaterShoreSegmentList [0].CanJoinAtFront (waterShore.mWaterShoreSegmentList [waterShore.mWaterShoreSegmentList.Count - 1], out needExchange)) {
			if (!needExchange) {
				JoinWaterShoreAtFront (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [0].CanJoinAtFront (waterShore.mWaterShoreSegmentList [0], out needExchange)) {
			if (needExchange) {
				waterShore.RevertWaterLines ();
				JoinWaterShoreAtFront (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (waterShore.mWaterShoreSegmentList [0], out needExchange)) {
			if (!needExchange) {
				JoinWaterShoreAtEnd (waterShore);
				return true;
			}
		}

		if (mWaterShoreSegmentList [mWaterShoreSegmentList.Count - 1].CanJoinAtEnd (waterShore.mWaterShoreSegmentList [waterShore.mWaterShoreSegmentList.Count - 1], out needExchange)) {
			if (needExchange) {
				waterShore.RevertWaterLines ();
				JoinWaterShoreAtEnd (waterShore);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 在前边插入新的水岸线
	/// </summary>
	/// <param name="waterShore">Water shore.</param>
	private void JoinWaterShoreAtFront(WaterShore waterShore)
	{
		List<WaterShoreSegment> waterShoreSegmentList = waterShore.mWaterShoreSegmentList;
		waterShoreSegmentList.AddRange (mWaterShoreSegmentList);
		mWaterShoreSegmentList = waterShoreSegmentList;
		waterShore.mWaterShoreSegmentList = null;
	}

	/// <summary>
	/// 在后边加入新的水岸线
	/// </summary>
	/// <param name="waterShore">Water shore.</param>
	private void JoinWaterShoreAtEnd(WaterShore waterShore)
	{
		mWaterShoreSegmentList.AddRange (waterShore.mWaterShoreSegmentList);
		waterShore.mWaterShoreSegmentList = null;
	}

	/// <summary>
	/// 获取水岸线中所有的线段
	/// </summary>
	/// <returns>The all water shore segment.</returns>
	public List<WaterShoreSegment> GetAllWaterShoreSegment()
	{
		return mWaterShoreSegmentList;
	}

	/// <summary>
	/// 获取所有水岸线的点
	/// </summary>
	/// <returns>The all shore point.</returns>
	public List<Vector3> GetAllShorePoint()
	{
		if (null == mWaterShorePointList) {
			mWaterShorePointList = new List<Vector3> ();
			for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
				WaterShoreSegment shoreSegment = mWaterShoreSegmentList [i];
				if (mWaterShorePointList.Count == 0) {
					mWaterShorePointList.Add (shoreSegment.posOne);
					mWaterShorePointList.Add (shoreSegment.posTwo);
				} else {
					if (!shoreSegment.IsPointNear (shoreSegment.posOne, mWaterShorePointList [mWaterShorePointList.Count - 1])) {
						mWaterShorePointList.Add (shoreSegment.posOne);
					}
					mWaterShorePointList.Add (shoreSegment.posTwo);
				}
			}
		}
		if(mWaterShorePointList[0].Equals(mWaterShorePointList[mWaterShorePointList.Count - 1]))
			mWaterShorePointList.RemoveAt(mWaterShorePointList.Count - 1);	
		return mWaterShorePointList;
	}

	/// <summary>
	/// 倒叙水岸线的相连的线段
	/// </summary>
	private void RevertWaterLines()
	{
		mWaterShoreSegmentList.Reverse ();
		for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
			mWaterShoreSegmentList [i].ExchangePos ();
		}
	}

	/// <summary>
	/// 检查是否有问题
	/// </summary>
	public void CheckError()
	{
		for (int i = 0; i < mWaterShoreSegmentList.Count - 1; ++i) {
			WaterShoreSegment curSegment = mWaterShoreSegmentList [i];
			WaterShoreSegment nextSegment = mWaterShoreSegmentList [i + 1];
			bool needExchange = false;
			if (curSegment.CanJoinAtEnd (nextSegment, out needExchange)) {
				if (!needExchange) {
					continue;
				}
			}
			if (Vector3.Distance (curSegment.posTwo, nextSegment.posOne) > Vector3.Distance (curSegment.posTwo, nextSegment.posTwo)) {
				Debug.LogError ("CheckError    1:  " + curSegment.ToString() + "   " + nextSegment.ToString());
			}
			Debug.LogError ("CheckError    2:  " + curSegment.ToString() + "   " + nextSegment.ToString());
		}
	}

	/// <summary>
	/// 裁减掉极小的不必要的线段
	/// </summary>
	/// <param name="waterLineLength">Water line length.</param>
	public void CullSmallShoreSegment(float waterLineLength)
	{
		if (mWaterShoreSegmentList.Count == 1)
			return;
		for (int i = mWaterShoreSegmentList.Count - 1; i >= 1; --i) {
			WaterShoreSegment curSegment = mWaterShoreSegmentList [i];
			WaterShoreSegment frontSegment = mWaterShoreSegmentList [i - 1];
			if (curSegment.GetLength () < waterLineLength) {
				mWaterShoreSegmentList.RemoveAt (i);
				frontSegment.posTwo = curSegment.posTwo;
			}
		}
		GetAllShorePoint ();
	}

	/// <summary>
	/// 传入的水岸线是否完全在当前的内部
	/// </summary>
	/// <returns><c>true</c> if this instance is water shore inside the specified other; otherwise, <c>false</c>.</returns>
	/// <param name="other">Other.</param>
	public bool IsWaterShoreInside(WaterShore other)
	{
		List<WaterShoreSegment> otherSegments = other.GetAllWaterShoreSegment ();
		for (int i = 0; i < otherSegments.Count; ++i) {
			WaterShoreSegment waterSegment = otherSegments [i];
			if (!IsPointInSide (waterSegment.posOne)) {
				return false;
			}
			if (!IsPointInSide (waterSegment.posTwo)) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 水是否在水岸线的内圈
	/// </summary>
	/// <value><c>true</c> if this instance is inside water; otherwise, <c>false</c>.</value>
	public bool IsInsideWater
	{
		get{
			Vector3 onePointInside = GetInsidePoint ();
//			Debug.LogError (ID.ToString() + "  获取到内部的点  " + onePointInside.x + "  " + onePointInside.z);
			float terrainHeight = Terrain.activeTerrain.SampleHeight(onePointInside);
//			Debug.LogError ( "  地形高度" + terrainHeight);
			return terrainHeight < mWaterHeight;
		}
	}

	public bool IsLineIntersection(Vector3 pos1, Vector3 pos2)
	{
		for (int i = 0; i < mWaterShorePointList.Count; ++i) {
			Vector3 curCheckInside = mWaterShorePointList[i];
			Vector3 nextCheckInside = (i == mWaterShorePointList.Count - 1) ? mWaterShorePointList [0] : mWaterShorePointList [i + 1];
			if (Utils.IsLineIntersection (pos1, pos2, curCheckInside, nextCheckInside))
				return true;
		}
		return false;
	}

	/// <summary>
	/// 获取圈中的任意一点
	/// </summary>
	/// <returns>The inside point.</returns>
	Vector3 GetInsidePoint()
	{
		//TODO 并不能保证这个点不是在其他的圈内部
		System.Text.StringBuilder sbLog = new System.Text.StringBuilder();
		for (int j = 1; j < 2; ++j) {
			float offsetDis = j * 0.01f;
			for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
				WaterShoreSegment waterShore = mWaterShoreSegmentList [i];
				Vector3 centerPos = (waterShore.posOne + waterShore.posTwo) / 2;
				Vector3 verticalDir = Vector3.Cross ((waterShore.posOne - centerPos).normalized, Vector3.up).normalized;
				Vector3 offsetPoint = centerPos + verticalDir * offsetDis;
				if (IsPointInSide (offsetPoint)) {
					return offsetPoint;
				}
				sbLog.Append (offsetPoint.x + "  " + offsetPoint.z + "\n");
				offsetPoint = centerPos - verticalDir * offsetDis;
				if (IsPointInSide (offsetPoint)) {
					return offsetPoint;
				}
				sbLog.Append (offsetPoint.x + "  " + offsetPoint.z + "\n");
			}
		}
		//TODO 这里真的出现过，需要调查
		IsInvalidShore = true;
		Debug.LogError ("居然没有找到在圈内的点  " + ID + "\n" + sbLog.ToString() );
		return Vector3.zero;
	}

	bool IsPointInSide(Vector3 point)
	{
		int intersectionCount = 0;
		for (int i = 0; i < mWaterShoreSegmentList.Count; ++i) {
			WaterShoreSegment shoreSegment = mWaterShoreSegmentList [i];
			if (Utils.IsLineIntersection (point, new Vector3 (100f, mWaterHeight, 100f)
				, shoreSegment.posOne, shoreSegment.posTwo))
				intersectionCount++;
		}
		return intersectionCount % 2 != 0;
	}
}