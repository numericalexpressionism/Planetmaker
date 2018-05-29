using System;
using System.Collections.Generic;
using Assets;
using UnityEngine;

class CircleEvent : VoronoiEvent
{
  public readonly Vector2 center;
  public readonly float r;
  private LinkedListNode<BeachLineArc> _midArc; //the arc to remove when event fires
  private CircleEvent(Vector2 center, float r)
  {
    this.center = center;
    this.r = r;
  }

  public override float GetLat()
  {
    return center.y + r;
  }

  public override void Process(LinkedList<BeachLineArc> beachLine, PriorityQueue<VoronoiEvent> events, VoronoiGraph result)
  {
    if (_midArc.List == null)
    {
      //event node already removed
      Debug.Log("ghost event");
      return;
    }
    var leftArc = _midArc.Value.Left.Value;
    var rightArc = _midArc.Value.Right.Value;

    var lx = leftArc.focus.x;
    var ly = leftArc.focus.y;
    var rx = rightArc.focus.x;
    var ry = rightArc.focus.y;
    var mx = _midArc.Value.focus.x;
    var my = _midArc.Value.focus.y;
    var epsilon = Mathf.Epsilon*1024*4;
    if ( (Mathf.Abs(lx - rx) < epsilon && Mathf.Abs(ly - ry) < epsilon)
      || (Mathf.Abs(mx - lx) < epsilon && Mathf.Abs(my - ly) < epsilon)
      || (Mathf.Abs(mx - rx) < epsilon && Mathf.Abs(my - ry) < epsilon))
    {
      Debug.Log("not a 3-point event");
      return;
    }
    _midArc.List.Remove(_midArc);

    var centerCartesian = MathS.SphToCartesian(center);

    result.AddVert(_midArc.Value.focus, leftArc.focus, centerCartesian);
    result.AddVert(_midArc.Value.focus, rightArc.focus, centerCartesian);
    result.AddVert(rightArc.focus, leftArc.focus, centerCartesian);

    BeachLineArc.MakeEvents(events, leftArc, rightArc);
  }

  public override void DrawDebug()
  {
    if (_midArc.List == null)
    {
      //event node already removed
      DebugHelper.DrawPoint(MathS.SphToCartesian(center), 2, Color.yellow);
      DebugHelper.DrawCircle(MathS.SphToCartesian(center), r, Color.yellow, 64);
      return;
    }
    var _leftArc = _midArc.Value.Left.Value;
    var _rightArc = _midArc.Value.Right.Value;

    DebugHelper.DrawPoint(MathS.SphToCartesian(center), 2, Color.green);
    DebugHelper.DrawCircle(MathS.SphToCartesian(center), r, Color.green, 64);

    DebugHelper.DrawPoint(MathS.SphToCartesian(_midArc.Value.focus), 3, Color.red);
    DebugHelper.DrawPoint(MathS.SphToCartesian(_leftArc.focus), 3, Color.blue);
    DebugHelper.DrawPoint(MathS.SphToCartesian(_rightArc.focus), 3, Color.yellow);
  }

  protected bool Equals(CircleEvent other)
  {
    return Mathf.Abs(center.x - other.center.x) < 1f / 4096
           && Mathf.Abs(center.y - other.center.y) < 1f / 4096
           && Mathf.Abs(r - other.r) < 1f / 4096;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return Equals((CircleEvent)obj);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return (center.GetHashCode() * 397) ^ r.GetHashCode();
    }
  }

  public static bool operator ==(CircleEvent left, CircleEvent right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(CircleEvent left, CircleEvent right)
  {
    return !Equals(left, right);
  }

  public static CircleEvent Create(LinkedListNode<BeachLineArc> pLl, LinkedListNode<BeachLineArc> pLc, LinkedListNode<BeachLineArc> plR)
  {
    float r;
    var c = MathS.GetCircleCentreCartesian(pLl.Value.focus, pLc.Value.focus, plR.Value.focus, out r);
    
    var result = new CircleEvent(MathS.CartesianToSph(c), r)
    {
      _midArc = pLc,
    };

    return result;
  }

  public static CircleEvent Inverse(CircleEvent original)
  {
    var copy = new CircleEvent(MathS.CartesianToSph(-MathS.SphToCartesian(original.center)), 180 - original.r)
    {
      _midArc = original._midArc,
    };
    return copy;
  }
}